using System;
using System.Threading;
using Microsoft.Diagnostics.Metric;

namespace OpenTelemetry.Metric.Api
{
    public class Guage : MeterBase
    {
        public Guage(string name) 
            : base(MetricSource.DefaultSource, name, "Guage", MetricLabel.DefaultLabel, MetricLabel.DefaultLabel)
        {
        }

        public Guage(string name, MetricLabel labels) 
            : base(MetricSource.DefaultSource, name, "Guage", labels, MetricLabel.DefaultLabel)
        {
        }

        public Guage(MetricSource source, string name) 
            : base(source, name, "Guage", MetricLabel.DefaultLabel, MetricLabel.DefaultLabel)
        {
        }

        public Guage(MetricSource source, string name, MetricLabel labels) 
            : base(source, name, "Guage", labels, MetricLabel.DefaultLabel)
        {
        }

        public Guage(MetricSource source, string name, MetricLabel labels, MetricLabel hints) 
            : base(source, name, "Guage", labels, hints)
        {
        }

        public void Record(int num)
        {
            RecordMetricData(num, MetricLabel.DefaultLabel);
        }

        public void Record(int num, MetricLabel labels)
        {
            RecordMetricData(num, labels);
        }

        public void Record(double num)
        {
            RecordMetricData(num, MetricLabel.DefaultLabel);
        }

        public void Record(double num, MetricLabel labels)
        {
            RecordMetricData(num, labels);
        }
    }

    public static partial class MeterExtensions
    {
        public static Guage CreateGuage(this MetricSource source, string name)
        {
            return new Guage(source, name);
        }

        public static Guage CreateGuage(this MetricSource source, string name, MetricLabel labels)
        {
            return new Guage(source, name, labels);
        }

        public static Guage CreateGuage(this MetricSource source, string name, MetricLabel labels, MetricLabel hints)
        {
            return new Guage(source, name, labels, hints);
        }
    }

}