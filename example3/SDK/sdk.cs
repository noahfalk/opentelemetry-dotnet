using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Linq;
using OpenTelmetry.Api;

namespace OpenTelmetry.Sdk
{
    public class SampleSdk : MetricSource
    {
        private readonly object lockCounters = new();
        private List<MetricBase> counters = new();

        private readonly object lockAggregateDict = new();
        private Dictionary<string, AggregationState> aggregateDict = new();

        private string name;
        private int collectPeriod_ms = 2000;
        private bool isBuilt = false;
        private bool isExitRequest = false;

        private List<string> namespaceExclusionList = new();

        private Task collectTask;

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
            collectTask = Task.Run(async () => {
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

            collectTask.Wait();
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

        private List<Tuple<string,Type>> ExpandLabels(MetricBase meter, LabelSet labels)
        {
            var ns = meter.MetricNamespace;
            var name = meter.MetricName;
            var type = meter.MetricType;

            var qualifiedName = ($"{type}/{ns}/{name}");

            // Merge Bound and Ad-Hoc labels into one

            Dictionary<string,string> labelDict = new();

            var boundLabels = meter.Labels.GetKeyValues();
            for (int n = 0; n < boundLabels.Length; n += 2)
            {
                labelDict[boundLabels[n]] = boundLabels[n+1];
            }

            var adhocLabels = labels.GetKeyValues();
            for (int n = 0; n < adhocLabels.Length; n += 2)
            {
                labelDict[adhocLabels[n]] = adhocLabels[n+1];
            }

            // TODO: Need to make this configurable for all kinds of Pre-Aggregates and Aggregation Types
            // Determine how to expand into different aggregates instances

            List<Tuple<string,Type>> label_aggregates = new();

            // Meter for total (drop all labels)
            label_aggregates.Add(Tuple.Create($"{qualifiedName}/_Total", typeof(CountSumMinMax)));

            label_aggregates.Add(Tuple.Create($"{qualifiedName}", typeof(LabelHistogram)));

            // Meter for each dimension
            foreach (var kv in labelDict)
            {
                label_aggregates.Add(Tuple.Create($"{qualifiedName}/{kv.Key}={kv.Value}", typeof(CountSumMinMax)));
            }

            return label_aggregates;
        }

        public override bool OnRecord(MetricBase counter, DateTimeOffset dt, int num, LabelSet labels)
        {
            return OnRecord(counter, dt, new MetricValue(num), labels);
        }

        public override bool OnRecord(MetricBase counter, DateTimeOffset dt, double num, LabelSet labels)
        {
            return OnRecord(counter, dt, new MetricValue(num), labels);
        }

        public bool OnRecord(MetricBase meter, DateTimeOffset dt, MetricValue num, LabelSet labels)
        {
            if (isBuilt && meter.Enabled)
            {
                if (meter.state is SumDataState data)
                {
                    var label_aggregates = ExpandLabels(meter, labels);

                    lock (lockAggregateDict)
                    {
                        foreach (var tup in label_aggregates)
                        {
                            var key = tup.Item1;
                            var type = tup.Item2;

                            AggregationState aggdata;
                            if (!aggregateDict.TryGetValue(key, out aggdata))
                            {
                                aggdata = (AggregationState) Activator.CreateInstance(type);
                                aggregateDict[key] = aggdata;
                            }

                            aggdata.Update(meter, num);
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
                var oldAggDict = Interlocked.Exchange(ref aggregateDict, new Dictionary<string, AggregationState>());

                StringBuilder sb = new StringBuilder();

                foreach (var kv in oldAggDict)
                {
                    sb.AppendLine(kv.Key);

                    // TODO: Print out each specific type of Aggregation.
                    if (kv.Value is CountSumMinMax cnt)
                    {
                        sb.AppendLine($"  CountSumMinMax: n={cnt.count}, sum={cnt.sum}, min={cnt.min}, max={cnt.max}");
                    }
                    else if (kv.Value is LabelHistogram hgm)
                    {
                        sb.Append($"  LabelHistogram: ");
                        var details = String.Join(", ", hgm.bins.Select(x => $"{x.Key}={x.Value}"));
                        sb.AppendLine(details);
                    }
                    else
                    {
                        sb.AppendLine($"  UNKNOWN");
                    }
                }

                Console.WriteLine(sb.ToString());
            }
        }

        public class SumDataState : MetricState
        {
            public readonly object lockState = new();
            public Dictionary<string, AggregationState> aggregates = new();
        }

        public abstract class AggregationState
        {
            public abstract void Update(MetricBase meter, MetricValue num);
        }

        public class CountSumMinMax : AggregationState
        {
            public int count = 0;
            public double sum = 0;
            public double max = 0;
            public double min = 0;

            public override void Update(MetricBase meter, MetricValue value)
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

        public class LabelHistogram : AggregationState
        {
            public Dictionary<string,int> bins = new();

            public override void Update(MetricBase meter, MetricValue value)
            {
                var labels = meter.Labels.GetKeyValues();

                var keys = new List<string>() { "_total" };

                for (var n = 0; n < labels.Length; n+= 2)
                {
                    keys.Add($"{labels[n]}:{labels[n+1]}");
                }

                foreach (var key in keys)
                {
                    int count;
                    if (!bins.TryGetValue(key, out count))
                    {
                        count = 0;
                    }

                    bins[key] = count + 1;
                }
            }
        }
    }
}