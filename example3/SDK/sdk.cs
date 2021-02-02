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

        public override bool OnRecord(MetricBase counter, int num, LabelSet boundLabels, LabelSet labels)
        {
            // TODO: Start with a SumData<int> and promoted to SumData<double> as necessary.

            return OnRecord(counter, (double) num, boundLabels, labels);
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

        public override bool OnRecord(MetricBase counter, double num, LabelSet boundLabels, LabelSet labels)
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
                            CountSumMinMax aggdata;
                            if (!data.aggregates.TryGetValue(key, out aggdata))
                            {
                                aggdata = new CountSumMinMax();
                                data.aggregates[key] = aggdata;
                            }

                            aggdata.count++;
                            aggdata.sum += num;
                            if (aggdata.count == 1)
                            {
                                aggdata.min = num;
                                aggdata.max = num;
                            }
                            else
                            {
                                aggdata.min = Math.Min(aggdata.min, num);
                                aggdata.max = Math.Max(aggdata.max, num);
                            }
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
                            var oldLabels = Interlocked.Exchange(ref data.aggregates, new Dictionary<string, CountSumMinMax>());

                            var ns = counter.MetricNamespace;
                            var name = counter.MetricName;
                            var type = counter.MetricType;
                            var counterLabels = counter.Labels.GetKeyValues();

                            sb.AppendLine($"{type}/{ns}/{name}/[{String.Join(",", counterLabels)}]");

                            foreach (var kv in oldLabels)
                            {
                                var labels = kv.Key;
                                var cnt = kv.Value;
                                sb.AppendLine($"  {labels} = n={cnt.count}, sum={cnt.sum}, min={cnt.min}, max={cnt.max}");
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
            public Dictionary<string, CountSumMinMax> aggregates = new();
        }

        public class CountSumMinMax
        {
            public int count = 0;
            public double sum = 0;
            public double max = 0;
            public double min = 0;
        }
    }
}