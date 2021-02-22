using System;
using System.Threading;
using Microsoft.Diagnostics.Metric;

namespace OpenTelemetry.Metric.Api
{
    public class Counter : MeterBase
    {
        public Counter(string name) 
            : base(MetricSource.DefaultSource, name, "Counter", MetricLabel.DefaultLabel, MetricLabel.DefaultLabel)
        {
        }

        public Counter(string name, MetricLabel labels) 
            : base(MetricSource.DefaultSource, name, "Counter", labels, MetricLabel.DefaultLabel)
        {
        }

        public Counter(MetricSource source, string name) 
            : base(source, name, "Counter", MetricLabel.DefaultLabel, MetricLabel.DefaultLabel)
        {
        }

        public Counter(MetricSource source, string name, MetricLabel labels) 
            : base(source, name, "Counter", labels, MetricLabel.DefaultLabel)
        {
        }

        public Counter(MetricSource source, string name, MetricLabel labels, MetricLabel hints) 
            : base(source, name, "Counter", labels, hints)
        {
        }

        public void Add(int num)
        {
            // TODO: Do we need to support passing native numbers to SDK?
            RecordMetricData(num, MetricLabel.DefaultLabel);
        }

        public void Add(int num, MetricLabel labels)
        {
            RecordMetricData(num, labels);
        }

        public void Add(double num)
        {
            RecordMetricData(num, MetricLabel.DefaultLabel);
        }

        public void Add(double num, MetricLabel labels)
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

        public static Counter CreateCounter(this MetricSource source, string name, MetricLabel labels)
        {
            return new Counter(source, name, labels);
        }

        public static Counter CreateCounter(this MetricSource source, string name, MetricLabel labels, MetricLabel hints)
        {
            return new Counter(source, name, labels, hints);
        }
    }
}