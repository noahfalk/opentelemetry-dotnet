using System;
using System.Threading;

namespace OpenTelmetry.Api
{
    public class Counter : MeterBase
    {
        public Counter(string name) : base(MetricProvider.DefaultProvider, name, "Counter", LabelSet.Empty)
        {
        }

        public Counter(string name, LabelSet labels) : base(MetricProvider.DefaultProvider, name, "Counter", labels)
        {
        }

        public Counter(MetricProvider provider, string name) : base(provider, name, "Counter", LabelSet.Empty)
        {
        }

        public Counter(MetricProvider provider, string name, LabelSet labels) : base(provider, name, "Counter", labels)
        {
        }

        public void Add(int num)
        {
            // TODO: Do we need to support passing native numbers to SDK?
            RecordMetricData(new MetricValue(num), LabelSet.Empty);
        }

        public void Add(int num, LabelSet labels)
        {
            RecordMetricData(new MetricValue(num), labels);
        }

        public void Add(double num)
        {
            RecordMetricData(new MetricValue(num), LabelSet.Empty);
        }

        public void Add(double num, LabelSet labels)
        {
            RecordMetricData(new MetricValue(num), labels);
        }
    }

    public static partial class MetricProviderExtensions
    {
        public static Counter CreateCounter(this MetricProvider provider, string name)
        {
            return new Counter(provider, name);
        }

        public static Counter CreateCounter(this MetricProvider provider, string name, LabelSet labels)
        {
            return new Counter(provider, name, labels);
        }
    }
}