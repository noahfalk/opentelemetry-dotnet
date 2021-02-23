using System;
using System.Threading;
using Microsoft.Diagnostics.Metric;

namespace OpenTelemetry.Metric.Api
{
    public class Counter : MeterBase
    {
        public Counter(string name) 
            : base(MetricSource.DefaultSource, name, "Counter", MetricLabelSet.DefaultLabel, MetricLabelSet.DefaultLabel)
        {
        }

        public Counter(string name, MetricLabelSet labels) 
            : base(MetricSource.DefaultSource, name, "Counter", labels, MetricLabelSet.DefaultLabel)
        {
        }

        public Counter(MetricSource source, string name) 
            : base(source, name, "Counter", MetricLabelSet.DefaultLabel, MetricLabelSet.DefaultLabel)
        {
        }

        public Counter(MetricSource source, string name, MetricLabelSet labels) 
            : base(source, name, "Counter", labels, MetricLabelSet.DefaultLabel)
        {
        }

        public Counter(MetricSource source, string name, MetricLabelSet labels, MetricLabelSet hints) 
            : base(source, name, "Counter", labels, hints)
        {
        }

        public void Add(int num)
        {
            // TODO: Do we need to support passing native numbers to SDK?
            RecordMetricData(num, MetricLabelSet.DefaultLabel);
        }

        public void Add(int num, MetricLabelSet labels)
        {
            RecordMetricData(num, labels);
        }

        public void Add(double num)
        {
            RecordMetricData(num, MetricLabelSet.DefaultLabel);
        }

        public void Add(double num, MetricLabelSet labels)
        {
            RecordMetricData(num, labels);
        }
    }

    public static partial class MeterExtensions
    {
        public static Counter CreateCounter(this MetricSource source, string name)
        {
            return new Counter(source, name);
        }

        public static Counter CreateCounter(this MetricSource source, string name, MetricLabelSet labels)
        {
            return new Counter(source, name, labels);
        }

        public static Counter CreateCounter(this MetricSource source, string name, MetricLabelSet labels, MetricLabelSet hints)
        {
            return new Counter(source, name, labels, hints);
        }
    }
}