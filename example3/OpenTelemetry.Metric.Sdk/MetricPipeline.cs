using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Linq;
using Microsoft.Diagnostics.Metric;
using OpenTelemetry.Metric.Api;

namespace OpenTelemetry.Metric.Sdk
{
    public class MetricPipeline
    {
        private readonly object lockMeters = new();
        private List<MeterBase> meters = new();

        private ConcurrentDictionary<string, AggregatorState> aggregateDict = new();

        private string name;
        private int collectPeriod_ms = 2000;
        private bool isBuilt = false;
        private CancellationTokenSource cancelTokenSrc = new();

        private List<Tuple<string,string>> metricFilterList = new();

        private List<(Aggregator agg, MetricLabel[] labels)> aggregateByLabelSet = new();

        private List<Exporter> exporters = new();

        private Task collectTask;
        private Task dequeueTask;

        private SdkListener listener;

        private HashSet<MetricSource> sources = new();

        private ConcurrentQueue<Tuple<MeterBase,DateTimeOffset,object,MetricLabel>> incomingQueue = new();
        private bool useQueue = false;

        public MetricPipeline Name(string name)
        {
            this.name = name;
            this.listener = new SdkListener(this);

            return this;
        }

        public MetricPipeline AttachSource(MetricSource source)
        {
            sources.Add(source);
            return this;
        }

        public MetricPipeline AttachSource(string ns)
        {
            var source = MetricSource.GetSource(ns);
            sources.Add(source);
            return this;
        }

        public MetricPipeline AddMetricInclusion(string term)
        {
            metricFilterList.Add(Tuple.Create("Include", term));
            return this;
        }

        public MetricPipeline AddMetricExclusion(string term)
        {
            metricFilterList.Add(Tuple.Create("Exclude", term));
            return this;
        }

        public MetricPipeline AggregateByLabels(Aggregator agg, params MetricLabel[] labelset)
        {
            aggregateByLabelSet.Add((agg, labelset));
            return this;
        }

        public MetricPipeline AddExporter(Exporter exporter)
        {
            exporters.Add(exporter);
            return this;
        }

        public MetricPipeline SetCollectionPeriod(int milliseconds)
        {
            collectPeriod_ms = milliseconds;
            return this;
        }

        public MetricPipeline UseQueue()
        {
            useQueue = true;
            return this;
        }

        public MetricPipeline Build()
        {
            // Start Periodic Collection Task

            var token = cancelTokenSrc.Token;

            collectTask = Task.Run(async () => {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(this.collectPeriod_ms, token);
                    }
                    catch (TaskCanceledException)
                    {
                        // Do Nothing
                    }

                    var export = Collect();

                    foreach (var exporter in exporters)
                    {
                        exporter.Export(export);
                    }
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
                            try
                            {
                                await Task.Delay(100, token);
                            }
                            catch (TaskCanceledException)
                            {
                                // Do Nothing
                            }
                        }
                    }
                });
            }

            foreach (var source in sources)
            {
                source.AttachListener(listener, "OTel SDK");
            }

            foreach (var exporter in exporters)
            {
                exporter.Start(token);
            }

            isBuilt = true;

            return this;
        }

        public void Stop()
        {
            cancelTokenSrc.Cancel();

            foreach (var source in sources)
            {
                source.DettachListener(listener, "OTel SDK");
            }

            collectTask.Wait();

            foreach (var exporter in exporters)
            {
                exporter.Stop();
            }
        }

        private List<(string labelKey, Aggregator agg)> ExpandLabels(MeterBase meter, MetricLabel labels)
        {
            var ns = meter.source.Name;
            var name = meter.MetricName;
            var type = meter.MetricType;

            // TODO: Area for performance improvements

            // TODO: Find a more performant way to avoid string interpolation.  Maybe class for segmented string list.  Reuse Labelset?

            var qualifiedName = ($"{ns}/{type}/{name}");

            // Merge Bound and Ad-Hoc labels into one

            Dictionary<string,string> labelDict = new();

            var boundLabels = meter.Labels.GetLabels();
            foreach (var label in boundLabels)
            {
                labelDict[label.name] = label.value;
            }

            var adhocLabels = labels.GetLabels();
            foreach (var label in adhocLabels)
            {
                labelDict[label.name] = label.value;
            }

            // Get Hints

            Dictionary<string,string> hints = new();

            var hintLabels = meter.Hints.GetLabels();
            foreach (var label in hintLabels)
            {
                hints[label.name] = label.value;
            }

            // Determine how to expand into different aggregates instances

            List<(string labelKey, Aggregator aggregator)> label_aggregates = new();

            // TODO: Use Meter.Hints to determine how to expand labels...
            var defaultAggType = hints.GetValueOrDefault("DefaultAggregator", "Sum");
            Aggregator defaultAgg = 
                defaultAggType == "Sum" ? new SumCountMinMax()
                : defaultAggType == "Histogram" ? new LabelHistogram()
                : new SumCountMinMax();
            var defaultAggName = defaultAgg.GetType().Name;

            // Meter for total (dropping all labels)
            label_aggregates.Add(($"{qualifiedName}/{defaultAggName}/_Total", defaultAgg));

            // Meter for each configured dimension
            foreach (var aggSet in aggregateByLabelSet)
            {
                var aggName = aggSet.agg.GetType().Name;
                var agg = aggSet.agg;

                foreach (var ls in aggSet.labels)
                {
                    List<string> paths = new();

                    foreach (var kv in ls.GetLabels())
                    {
                        var lskey = kv.Item1;
                        var lsval = kv.Item2;

                        if (labelDict.TryGetValue(lskey, out var val))
                        {
                            if (lsval == "*")
                            {
                                paths.Add($"{lskey}={val}");
                            }
                            else
                            {
                                var itemval = lsval.Split(",");
                                if (itemval.Contains(val))
                                {
                                    paths.Add($"{lskey}={val}");
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (paths.Count() > 0)
                    {
                        paths.Sort();
                        var dim = String.Join("/", paths);
                        label_aggregates.Add(($"{qualifiedName}/{aggName}/{dim}", agg));
                    }
                }

                if (aggSet.labels.Length == 0)
                {
                    label_aggregates.Add(($"{qualifiedName}/{aggName}/_Total", agg));
                }
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

        public bool OnRecord<T>(MeterBase meter, DateTimeOffset dt, T value, MetricLabel labels)
        {
            if (useQueue)
            {
                incomingQueue.Enqueue(Tuple.Create(meter, dt, (object) value, labels));
                return true;
            }

            return ProcessRecord<T>(meter, dt, value, labels);
        }

        private bool ProcessRecord<T>(MeterBase meter, DateTimeOffset dt, T value, MetricLabel labels)
        {
            if (isBuilt && meter.Enabled)
            {
                // Expand out all the aggregates we need to update based on this measurement
                var label_aggregates = ExpandLabels(meter, labels);

                foreach (var kv in label_aggregates)
                {
                    AggregatorState aggState;
                    if (!aggregateDict.TryGetValue(kv.labelKey, out aggState))
                    {
                        aggState = kv.agg.CreateState();
                        aggregateDict[kv.labelKey] = aggState;
                    }

                    aggState.Update(meter, value, labels);
                }

                return true;
            }

            return false;
        }

        private ExportItem[] Collect()
        {
            List<ExportItem> ret = new();

            Console.WriteLine($"*** Collect {name}...");

            if (isBuilt)
            {
                List<string> exports = new();

                // Reset all aggregate states!
                var oldAggStates = Interlocked.Exchange(ref aggregateDict, new ConcurrentDictionary<string, AggregatorState>());

                foreach (var kv in oldAggStates)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine($"{name}: {kv.Key}");

                    sb.Append($"    {kv.Value.GetType().Name}: ");

                    var data = kv.Value.Serialize();
                    var details = String.Join(", ", data.Select(x => $"{x.key}={x.value}"));
                    sb.AppendLine(details);

                    exports.Add(sb.ToString());
                }

                exports.Sort();
                foreach (var item in exports)
                {
                    ret.Add(new StringExportItem(item));
                }
            }

            return ret.ToArray();
        }
   }
}