using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Metric
{
    public class Gauge : Meter
    {
        public Gauge(string name, MetricSource source = null) 
            : base(source, name, Array.Empty<string>())
        {
        }

        public Gauge(string name, Dictionary<string, string> staticLabels, MetricSource source = null) :
            base(source, name, staticLabels, Array.Empty<string>())
        {
        }

        public Gauge(string name, string[] labelNames, MetricSource source = null) 
            : base(source, name, labelNames)
        {
        }

        public Gauge(string name, Dictionary<string,string> staticLabels, string[] labelNames, MetricSource source = null) 
            : base(source, name, staticLabels, labelNames)
        {
        }

        public override AggregationConfiguration DefaultAggregation => AggregationConfigurations.LastValue;

        public void Set(double d) => Set(d, Array.Empty<string>());

        public void Set(double d, params string[] labelValues)
        {
            RecordMeasurement(d, labelValues);
        }
    }
}
