using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;

namespace OpenTelmetry.Api
{
    public abstract class MetricBase
    {
        static protected List<MetricSource> registered = new();

        public MetricSource source { get; }

        public string MetricName { get; }

        public string MetricNamespace { get; }

        public string MetricType { get; }

        public LabelSet Labels { get; }

        public virtual bool Enabled { get; set; }

        public MetricState state { get; set; }

        protected MetricBase(string ns, string name, string type, LabelSet labels)
        {
            MetricName = name;
            MetricNamespace = ns;
            MetricType = type;
            Labels = labels;

            var sources = registered;
            if (sources.Count > 0)
            {
                source = sources[sources.Count-1];
                Enabled = source.OnCreate(this, labels);
            }
        }

        protected void RecordMetricData(int num, LabelSet labels)
        {
            if (Enabled)
            {
                source?.OnRecord(this, DateTimeOffset.UtcNow, num, labels);
            }
        }

        protected void RecordMetricData(double num, LabelSet labels)
        {
            if (Enabled)
            {
                source?.OnRecord(this, DateTimeOffset.UtcNow, num, labels);
            }
        }

        protected void RecordMetricData(MetricValue val, LabelSet labels)
        {
            if (Enabled)
            {
                if (val.value is int i)
                {
                    source?.OnRecord(this, DateTimeOffset.UtcNow, i, labels);
                }
                else if (val.value is double d)
                {
                    source?.OnRecord(this, DateTimeOffset.UtcNow, d, labels);
                }
            }
        }

        static public void RegisterSDK(MetricSource source)
        {
            while (true)
            {
                var oldSources = registered;

                var newSources = new List<MetricSource>();
                newSources.AddRange(oldSources);
                newSources.Add(source);

                var orgSources = Interlocked.CompareExchange(ref MetricBase.registered, newSources, oldSources);
                if (orgSources == oldSources)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Allow Batching of recordings
        /// </summary>
        public class BatchBuilder
        {
            private List<Tuple<MetricBase, MetricValue>> batches = new();
            private LabelSet labels;

            public BatchBuilder(LabelSet labels)
            {
                this.labels = labels;
            }

            public BatchBuilder Add(MetricBase meter, int value)
            {
                batches.Add(Tuple.Create(meter, new MetricValue(value)));

                return this;
            }

            public void Record()
            {
                DateTimeOffset dt = DateTimeOffset.UtcNow;

                foreach (var recording in batches)
                {
                    var meter = recording.Item1;
                    var val = recording.Item2;

                    if (meter.Enabled)
                    {
                        if (val.value is int i)
                        {
                            meter.source?.OnRecord(meter, dt, i, labels);
                        }
                        else if (val.value is double d)
                        {
                            meter.source?.OnRecord(meter, dt, d, labels);
                        }
                    }
                }
            }
        }

    }
}