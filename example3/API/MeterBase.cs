using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;

namespace OpenTelmetry.Api
{
    public abstract class MeterBase
    {
        public MetricProvider provider { get; }
        public string MetricName { get; }
        public string MetricNamespace { get; }
        public string MetricType { get; }
        public LabelSet Labels { get; }

        public virtual bool Enabled { get; set; } = true;

        // Allow custom Meters to store their own state
        public MeterState state { get; set; }

        protected MeterBase(MetricProvider provider, string name, string type, LabelSet labels)
        {
            MetricName = name;
            MetricNamespace = provider.GetName();
            MetricType = type;
            Labels = labels;
            this.provider = provider;

            // TODO: How to handle attach/detach of providers and listeners?
            foreach (var listener in provider.GetListeners())
            {
                listener?.OnCreate(this, labels);
            }
        }

        protected void RecordMetricData(MetricValue val, LabelSet labels)
        {
            if (Enabled)
            {
                foreach (var listener in provider.GetListeners())
                {
                    listener?.OnRecord(this, val, labels);
                }
            }
        }
    }
}
