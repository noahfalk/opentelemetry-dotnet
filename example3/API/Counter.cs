using System;
using System.Threading;
using Microsoft.Diagnostics.Metric;

namespace OpenTelmetry.Api
{
    public class Counter : MeterBase
    {
        public Counter(string name) 
            : base(MetricSource.DefaultSource, name, "Counter", LabelSet.Empty, LabelSet.Empty)
        {
        }

        public Counter(string name, LabelSet labels) 
            : base(MetricSource.DefaultSource, name, "Counter", labels, LabelSet.Empty)
        {
        }

        public Counter(MetricSource source, string name) 
            : base(source, name, "Counter", LabelSet.Empty, LabelSet.Empty)
        {
        }

        public Counter(MetricSource source, string name, LabelSet labels) 
            : base(source, name, "Counter", labels, LabelSet.Empty)
        {
        }

        public Counter(MetricSource source, string name, LabelSet labels, LabelSet hints) 
            : base(source, name, "Counter", labels, hints)
        {
        }

        public void Add(int num)
        {
            // TODO: Do we need to support passing native numbers to SDK?
            RecordMetricData(num, LabelSet.Empty);
        }

        public void Add(int num, LabelSet labels)
        {
            RecordMetricData(num, labels);
        }

        public void Add(double num)
        {
            RecordMetricData(num, LabelSet.Empty);
        }

        public void Add(double num, LabelSet labels)
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

        public static Counter CreateCounter(this MetricSource source, string name, LabelSet labels)
        {
            return new Counter(source, name, labels);
        }

        public static Counter CreateCounter(this MetricSource source, string name, LabelSet labels, LabelSet hints)
        {
            return new Counter(source, name, labels, hints);
        }
    }
}