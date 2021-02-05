using System;
using System.Threading;

namespace OpenTelmetry.Api
{
    public class Guage : MeterBase
    {
        public Guage(string name) 
            : base(MetricProvider.DefaultProvider, name, "Guage", LabelSet.Empty, LabelSet.Empty)
        {
        }

        public Guage(string name, LabelSet labels) 
            : base(MetricProvider.DefaultProvider, name, "Guage", labels, LabelSet.Empty)
        {
        }

        public Guage(MetricProvider provider, string name) 
            : base(provider, name, "Guage", LabelSet.Empty, LabelSet.Empty)
        {
        }

        public Guage(MetricProvider provider, string name, LabelSet labels) 
            : base(provider, name, "Guage", labels, LabelSet.Empty)
        {
        }

        public Guage(MetricProvider provider, string name, LabelSet labels, LabelSet hints) 
            : base(provider, name, "Guage", labels, hints)
        {
        }

        public void Record(int num)
        {
            RecordMetricData(new MetricValue(num), LabelSet.Empty);
        }

        public void Record(int num, LabelSet labels)
        {
            RecordMetricData(new MetricValue(num), labels);
        }

        public void Record(double num)
        {
            RecordMetricData(new MetricValue(num), LabelSet.Empty);
        }

        public void Record(double num, LabelSet labels)
        {
            RecordMetricData(new MetricValue(num), labels);
        }
    }

    public static partial class MetricProviderExtensions
    {
        public static Guage CreateGuage(this MetricProvider provider, string name)
        {
            return new Guage(provider, name);
        }

        public static Guage CreateGuage(this MetricProvider provider, string name, LabelSet labels)
        {
            return new Guage(provider, name, labels);
        }
    }

}