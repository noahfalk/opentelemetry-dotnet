using System;
using System.Threading;
using Microsoft.Diagnostics.Metric;

namespace OpenTelemetry.Metric.Api
{
    public class Guage : MeterBase
    {
        public Guage(string name) 
            : base(MetricSource.DefaultSource, name, "Guage", MetricLabelSet.DefaultLabelSet, MetricLabelSet.DefaultLabelSet)
        {
        }

        public Guage(string name, MetricLabelSet labels) 
            : base(MetricSource.DefaultSource, name, "Guage", labels, MetricLabelSet.DefaultLabelSet)
        {
        }

        public Guage(MetricSource source, string name) 
            : base(source, name, "Guage", MetricLabelSet.DefaultLabelSet, MetricLabelSet.DefaultLabelSet)
        {
        }

        public Guage(MetricSource source, string name, MetricLabelSet labels) 
            : base(source, name, "Guage", labels, MetricLabelSet.DefaultLabelSet)
        {
        }

        public Guage(MetricSource source, string name, MetricLabelSet labels, MetricLabelSet hints) 
            : base(source, name, "Guage", labels, hints)
        {
        }

        public void Record(int num)
        {
            RecordMetricData(num, MetricLabelSet.DefaultLabelSet);
        }

        public void Record(int num, MetricLabelSet labels)
        {
            RecordMetricData(num, labels);
        }

        public void Record(double num)
        {
            RecordMetricData(num, MetricLabelSet.DefaultLabelSet);
        }

        public void Record(double num, MetricLabelSet labels)
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

        public static Guage CreateGuage(this MetricSource source, string name, MetricLabelSet labels)
        {
            return new Guage(source, name, labels);
        }

        public static Guage CreateGuage(this MetricSource source, string name, MetricLabelSet labels, MetricLabelSet hints)
        {
            return new Guage(source, name, labels, hints);
        }
    }

}