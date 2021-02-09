using System;
using System.Threading;

namespace OpenTelmetry.Api
{
    public class Guage : MeterBase
    {
        public Guage(string name) 
            : base(MetricSource.DefaultSource, name, "Guage", LabelSet.Empty, LabelSet.Empty)
        {
        }

        public Guage(string name, LabelSet labels) 
            : base(MetricSource.DefaultSource, name, "Guage", labels, LabelSet.Empty)
        {
        }

        public Guage(MetricSource source, string name) 
            : base(source, name, "Guage", LabelSet.Empty, LabelSet.Empty)
        {
        }

        public Guage(MetricSource source, string name, LabelSet labels) 
            : base(source, name, "Guage", labels, LabelSet.Empty)
        {
        }

        public Guage(MetricSource source, string name, LabelSet labels, LabelSet hints) 
            : base(source, name, "Guage", labels, hints)
        {
        }

        public void Record(int num)
        {
            RecordMetricData(num, LabelSet.Empty);
        }

        public void Record(int num, LabelSet labels)
        {
            RecordMetricData(num, labels);
        }

        public void Record(double num)
        {
            RecordMetricData(num, LabelSet.Empty);
        }

        public void Record(double num, LabelSet labels)
        {
            RecordMetricData(num, labels);
        }
    }

    public static partial class MetricSourceExtensions
    {
        public static Guage CreateGuage(this MetricSource source, string name)
        {
            return new Guage(source, name);
        }

        public static Guage CreateGuage(this MetricSource source, string name, LabelSet labels)
        {
            return new Guage(source, name, labels);
        }
    }

}