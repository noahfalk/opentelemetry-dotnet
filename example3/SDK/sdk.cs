using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Linq;
using OpenTelmetry.Api;

namespace OpenTelmetry.Sdk
{
    public class SampleSdk
    {
        private readonly object lockMeters = new();
        private List<MeterBase> meters = new();

        private readonly object lockAggregateDict = new();
        private Dictionary<string, Aggregator> aggregateDict = new();

        private string name;
        private int collectPeriod_ms = 2000;
        private bool isBuilt = false;
        private CancellationTokenSource cancelTokenSrc = new();

        private List<Tuple<string,string>> metricFilterList = new();

        private Task collectTask;
        private Task dequeueTask;

        private SdkListener listener;

        private HashSet<MetricProvider> providers = new();

        private ConcurrentQueue<Tuple<MeterBase,DateTimeOffset,object,LabelSet>> incomingQueue = new();
        private bool useQueue = false;

        public SampleSdk Name(string name)
        {
            this.name = name;
            this.listener = new SdkListener(this);

            return this;
        }

        public SampleSdk AttachProvider(MetricProvider provider)
        {
            providers.Add(provider);
            return this;
        }

        public SampleSdk AttachProvider(string ns)
        {
            var provider = MetricProvider.GetProvider(ns);
            providers.Add(provider);
            return this;
        }

        public SampleSdk AddMetricInclusion(string term)
        {
            metricFilterList.Add(Tuple.Create("Include", term));
            return this;
        }

        public SampleSdk AddMetricExclusion(string term)
        {
            metricFilterList.Add(Tuple.Create("Exclude", term));
            return this;
        }

        public SampleSdk SetCollectionPeriod(int milliseconds)
        {
            collectPeriod_ms = milliseconds;
            return this;
        }

        public SampleSdk UseQueue()
        {
            useQueue = true;
            return this;
        }

        public SampleSdk Build()
        {
            // Start Periodic Collection Task

            var token = cancelTokenSrc.Token;

            collectTask = Task.Run(async () => {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(this.collectPeriod_ms);
                    Collect();
                }
            });

            if (useQueue)
            {
                dequeueTask = Task.Run(async () => {
                    while (!token.IsCancellationRequested)
                    {
                        if (incomingQueue.TryDequeue(out var record))
                        {
                            ProcessRecord(record.Item1, record.Item2, record.Item3, record.Item4);
                        }
                        else
                        {
                            await Task.Delay(100);
                        }
                    }
                });
            }

            foreach (var provider in providers)
            {
                provider.AttachListener(listener);
            }

            isBuilt = true;

            return this;
        }

        public void Stop()
        {
            cancelTokenSrc.Cancel();

            foreach (var provider in providers)
            {
                provider.DettachListener(listener);
            }

            collectTask.Wait();
        }

        private List<Tuple<string,Type>> ExpandLabels(MeterBase meter, LabelSet labels)
        {
            var ns = meter.MetricNamespace;
            var name = meter.MetricName;
            var type = meter.MetricType;

            // TODO: Area for performance improvements

            // TODO: Find a more performant way to avoid string interpolation.  Maybe class for segmented string list.  Reuse Labelset?

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

            // Get Hints

            Dictionary<string,string> hints = new();

            var hintLabels = meter.Hints.GetKeyValues();
            for (int n = 0; n < hintLabels.Length; n += 2)
            {
                hints[hintLabels[n]] = hintLabels[n+1];
            }

            // TODO: Need to make this configurable for all kinds of Pre-Aggregates and Aggregation Types
            // Determine how to expand into different aggregates instances

            List<Tuple<string,Type>> label_aggregates = new();

            // TODO: Use Meter.Hints to determine how to expand labels...
            var defaultAgg = hints.GetValueOrDefault("DefaultAggregator", "Sum");
            var defaultAggType = 
                defaultAgg == "Sum" ? typeof(CountSumMinMax)
                : defaultAgg == "Histogram" ? typeof(LabelHistogram)
                : typeof(CountSumMinMax);

            // Meter for total (dropping all labels)
            label_aggregates.Add(Tuple.Create($"{qualifiedName}/_Total", defaultAggType));

            // Meter for each 1D dimension
            foreach (var kv in labelDict)
            {
                label_aggregates.Add(Tuple.Create($"{qualifiedName}/{kv.Key}={kv.Value}", defaultAggType));
            }

            // Apply inclusion/exclusion filters
            foreach (var filter in metricFilterList)
            {
                // TODO: Need to optimize!

                if (filter.Item1 == "Include")
                {
                    label_aggregates = label_aggregates.Where((k) => k.Item1.Contains(filter.Item2)).ToList();
                }
                else
                {
                    label_aggregates = label_aggregates.Where((k) => !k.Item1.Contains(filter.Item2)).ToList();
                }
            }

            return label_aggregates;
        }

        public bool OnRecord<T>(MeterBase meter, DateTimeOffset dt, T value, LabelSet labels)
        {
            if (useQueue)
            {
                incomingQueue.Enqueue(Tuple.Create(meter, dt, (object) value, labels));
                return true;
            }

            return ProcessRecord<T>(meter, dt, value, labels);
        }

        private bool ProcessRecord<T>(MeterBase meter, DateTimeOffset dt, T value, LabelSet labels)
        {
            if (isBuilt && meter.Enabled)
            {
                // Expand out all the aggregates we need to update based on this measurement
                var label_aggregates = ExpandLabels(meter, labels);

                lock (lockAggregateDict)
                {
                    foreach (var tup in label_aggregates)
                    {
                        var key = tup.Item1;
                        var type = tup.Item2;

                        Aggregator aggdata;
                        if (!aggregateDict.TryGetValue(key, out aggdata))
                        {
                            aggdata = (Aggregator) Activator.CreateInstance(type);
                            aggregateDict[key] = aggdata;
                        }

                        aggdata.Update(meter, value, labels);
                    }
                }

                return true;
            }

            return false;
        }

        private void Collect()
        {
            Console.WriteLine("*** Collect...");

            if (isBuilt)
            {
                // Reset all aggregates!
                var oldAggDict = Interlocked.Exchange(ref aggregateDict, new Dictionary<string, Aggregator>());

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
   }
}