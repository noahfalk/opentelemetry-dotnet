using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Metric
{
    public class Gauge : Meter
    {
        public Gauge(string libname, string libver, string name) 
            : base(libname, libver, name, Array.Empty<string>())
        {
        }

        public Gauge(string libname, string libver, string name, Dictionary<string, string> staticLabels) :
            base(libname, libver, name, staticLabels, Array.Empty<string>())
        {
        }

        public Gauge(string libname, string libver, string name, string[] labelNames) 
            : base(libname, libver, name, labelNames)
        {
        }

        public Gauge(string libname, string libver, string name, Dictionary<string,string> staticLabels, string[] labelNames) 
            : base(libname, libver, name, staticLabels, labelNames)
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
