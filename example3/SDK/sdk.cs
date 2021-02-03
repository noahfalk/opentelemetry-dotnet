using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using OpenTelmetry.Api;

namespace OpenTelmetry.Sdk
{
    public class SampleSdk : MetricSource
    {
        private readonly object lockCounters = new();
        private List<MetricBase> counters = new();

        private string name;
        private int collectPeriod_ms = 2000;
        private bool isBuilt = false;
        private bool isExitRequest = false;

        private List<string> namespaceExclusionList = new();

        public SampleSdk Name(string name)
        {
            this.name = name;
            return this;
        }

        public SampleSdk AddNamespaceExclusion(string ns)
        {
            namespaceExclusionList.Add(ns);
            return this;
        }

        public SampleSdk SetCollectionPeriod(int milliseconds)
        {
            collectPeriod_ms = milliseconds;
            return this;
        }

        public SampleSdk Build()
        {
            isBuilt = true;

            // Start Periodic Collection Task
            Task.Run(async () => {
                while (!isExitRequest)
                {
                    await Task.Delay(this.collectPeriod_ms);
                    Collect();
                }
            });

            base.RegisterSDK();

            return this;
        }

        public bool IsStop()
        {
            return isExitRequest;
        }
        
        public void Stop()
        {
            isExitRequest = true;

            Task.Delay(2 * collectPeriod_ms).Wait();
        }

        public override bool OnCreate(MetricBase counter, LabelSet labels)
        {
            lock (lockCounters)
            {
                counters.Add(counter);
                counter.state = new SumDataState();
            }

            return true;
        }

        private List<string> ExpandLabels(LabelSet boundLabels, LabelSet labels)
        {
            List<string> label_aggregates = new();

            label_aggregates.Add("_Total");

            var lbl = labels.GetKeyValues();
            for (int n = 0; n < lbl.Length; n += 2)
            {
                if (lbl[n] == "OperNum")
                {
                    label_aggregates.Add($"{lbl[n]}={lbl[n+1]}");
                }
            }

            lbl = boundLabels.GetKeyValues();
            for (int n = 0; n < lbl.Length; n += 2)
            {
                if (lbl[n] == "LibraryInstanceName")
                {
                    label_aggregates.Add($"{lbl[n]}={lbl[n+1]}");
                }
            }

            return label_aggregates;
        }

        public override bool OnRecord(MetricBase counter, int num, LabelSet boundLabels, LabelSet labels)
        {
            return OnRecord(counter, new MetricValue(num), boundLabels, labels);
        }

        public override bool OnRecord(MetricBase counter, double num, LabelSet boundLabels, LabelSet labels)
        {
            return OnRecord(counter, new MetricValue(num), boundLabels, labels);
        }

        public bool OnRecord(MetricBase counter, MetricValue num, LabelSet boundLabels, LabelSet labels)
        {
            if (isBuilt && counter.Enabled)
            {
                if (counter.state is SumDataState data)
                {
                    var label_aggregates = ExpandLabels(boundLabels, labels);

                    lock (data.lockState)
                    {
                        foreach (var key in label_aggregates)
                        {
                            AggregationState aggdata;
                            if (!data.aggregates.TryGetValue(key, out aggdata))
                            {
                                // TODO: How to specifying the type of Aggregation we're doing here!
                                aggdata = new CountSumMinMax();

                                data.aggregates[key] = aggdata;
                            }

                            aggdata.Update(num);
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        private void Collect()
        {
            if (isBuilt)
            {
                StringBuilder sb = new StringBuilder();

                foreach (var counter in counters)
                {
                    sb.Clear();

                    if (counter.state is SumDataState data)
                    {
                        if (data.aggregates.Count > 0)
                        {
                            var oldLabels = Interlocked.Exchange(ref data.aggregates, new Dictionary<string, AggregationState>());

                            var ns = counter.MetricNamespace;
                            var name = counter.MetricName;
                            var type = counter.MetricType;
                            var counterLabels = counter.Labels.GetKeyValues();

                            sb.AppendLine($"{type}/{ns}/{name}/[{String.Join(",", counterLabels)}]");

                            foreach (var kv in oldLabels)
                            {
                                // TODO: Print out each specific type of Aggregation.
                                if (kv.Value is CountSumMinMax cnt)
                                {
                                    sb.AppendLine($"  {kv.Key} = n={cnt.count}, sum={cnt.sum}, min={cnt.min}, max={cnt.max}");
                                }
                                else
                                {
                                    sb.AppendLine($"  {kv.Key} = UNKNOWN");
                                }
                            }

                            Console.WriteLine(sb.ToString());
                        }
                    }
                }
            }
        }

        public class SumDataState : MetricState
        {
            public readonly object lockState = new();
            public Dictionary<string, AggregationState> aggregates = new();
        }

        public abstract class AggregationState
        {
            public abstract void Update(MetricValue num);
        }

        public class CountSumMinMax : AggregationState
        {
            public int count = 0;
            public double sum = 0;
            public double max = 0;
            public double min = 0;

            public override void Update(MetricValue value)
            {
                double num = 0;
                if (value.value is int i)
                {
                    num = i;
                }
                if (value.value is double d)
                {
                    num = d;
                }

                count++;
                sum += num;
                if (count == 1)
                {
                    min = num;
                    max = num;
                }
                else
                {
                    min = Math.Min(min, num);
                    max = Math.Max(max, num);
                }
            }
        }
    }
}