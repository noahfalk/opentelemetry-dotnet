using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;

namespace OpenTelmetry.Api
{
    public abstract class MeterBase
    {
        static protected List<MetricListener> registered = new();

        public MetricListener listener { get; }

        public string MetricName { get; }

        public string MetricNamespace { get; }

        public string MetricType { get; }

        public LabelSet Labels { get; }

        public virtual bool Enabled { get; set; }

        public MetricState state { get; set; }

        protected MeterBase(MetricProvider provider, string name, string type, LabelSet labels)
        {
            MetricName = name;
            MetricNamespace = provider.GetNamespace();
            MetricType = type;
            Labels = labels;

            var sources = registered;
            if (sources.Count > 0)
            {
                listener = sources[sources.Count-1];
                Enabled = listener.OnCreate(this, labels);
            }
        }

        protected void RecordMetricData(MetricValue val, LabelSet labels)
        {
            if (Enabled)
            {
                listener?.OnRecord(this, val, labels);
            }
        }

        static public void RegisterSDK(MetricListener source)
        {
            while (true)
            {
                var oldSources = registered;

                var newSources = new List<MetricListener>();
                newSources.AddRange(oldSources);
                newSources.Add(source);

                var orgSources = Interlocked.CompareExchange(ref MeterBase.registered, newSources, oldSources);
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
            private List<Tuple<MeterBase, MetricValue>> batches = new();
            private LabelSet labels;
            private MetricListener listener;

            public BatchBuilder(LabelSet labels)
            {
                this.labels = labels;
            }

            public BatchBuilder RecordMeasurement(MeterBase meter, int value)
            {
                // TODO: Handle case where we mix meters from different listeners!
                if (meter.Enabled)
                {
                    if (listener is null)
                    {
                        listener = meter.listener;
                    }

                    if (meter.listener == listener)
                    {
                        batches.Add(Tuple.Create(meter, new MetricValue(value)));
                    }
                }

                return this;
            }

            public BatchBuilder RecordMeasurement(MeterBase meter, double value)
            {
                // TODO: Handle case where we mix meters from different listeners!
                if (meter.Enabled)
                {
                    if (listener is null)
                    {
                        listener = meter.listener;
                    }

                    if (meter.listener == listener)
                    {
                        batches.Add(Tuple.Create(meter, new MetricValue(value)));
                    }
                }

                return this;
            }

            public void Record()
            {
                if (batches.Count > 0)
                {
                    listener.OnRecord(batches, labels);
                }
            }
        }

    }
}