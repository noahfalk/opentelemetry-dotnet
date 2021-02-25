using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;

namespace Microsoft.Diagnostics.Metric
{
    public abstract class MetricBase
    {
        public MetricSource source { get; }

        public string MetricName { get; }

        public string MetricType { get; }

        public MetricLabelSet Labels { get; }

        public bool Enabled { get; set; } = true;

        public MetricLabelSet Hints { get; protected set; }

        protected MetricBase(MetricSource source, string name, string type, MetricLabelSet labels, MetricLabelSet hints)
        {
            this.MetricName = name;
            this.MetricType = type;
            this.Labels = labels;
            this.Hints = hints;
            this.source = source;

            // TODO: How to handle attach/detach of sources and listeners?
            source.ReportCreate(this);
        }

        protected void RecordMetricData<T>(T val, MetricLabelSet labels)
        {
            if (Enabled)
            {
                source.ReportValue(this, val, labels);
            }
        }
    }
}
