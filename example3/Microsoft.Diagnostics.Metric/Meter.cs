using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;

namespace Microsoft.Diagnostics.Metric
{
    public abstract class Meter : MeterBase
    {
        public static Dictionary<string, string> EmptyStaticLabels { get; } = new Dictionary<string, string>();

        public override MetricSource Source { get; }
        public override string Name { get; }

        public override string[] LabelNames { get; }

        public override Dictionary<string, string> StaticLabels { get; }

        protected Meter(MetricSource source, string name, string[] labelNames) 
            : this(source, name, EmptyStaticLabels, labelNames)
        {
        }

        protected Meter(MetricSource source, string name, Dictionary<string,string> staticLabels, string[] labelNames)
        {
            Source = source;
            Name = name;
            StaticLabels = staticLabels;
            LabelNames = labelNames;
            MeterCollection.Instance.AddMetric(this);
        }

        
    }
}
