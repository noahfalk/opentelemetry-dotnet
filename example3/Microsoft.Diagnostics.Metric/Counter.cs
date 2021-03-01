using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Metric
{
    public class Counter : Meter
    {
        public Counter(string name) :
            base(name, Array.Empty<string>())
        {
        }

        public Counter(string name, Dictionary<string, string> staticLabels) :
            base(name, staticLabels, Array.Empty<string>())
        {
        }

        public Counter(string name, string[] labelNames) :
            base(name, labelNames)
        {
        }

        public Counter(string name, Dictionary<string, string> staticLabels, string[] labelNames) :
            base(name, staticLabels, labelNames)
        {
        }

        public override AggregationConfiguration DefaultAggregation => AggregationConfigurations.Sum;

        public void Add(double d) => Add(d, Array.Empty<string>());

        public void Add(double d, params string[] labelValues)
        {
            base.RecordMeasurement(d, labelValues);
        }

        public LabeledCounter WithLabels(params string[] labelValues)
        {
            //TODO: we should probably memoize this
            return new LabeledCounter(this, labelValues);
        }
    }

    public class LabeledCounter : LabeledMeter<Counter>
    {
        internal LabeledCounter(Counter unlabled, string[] labelValues) : base(unlabled, labelValues) { }

        public void Add(double d)
        {
            base.RecordMeasurement(d, LabelValues);
        }
    }
}
