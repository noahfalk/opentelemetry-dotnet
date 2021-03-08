using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Metric
{
    public class Counter : Meter
    {
        public Counter(string name, MetricSource source = null) :
            base(source, name, Array.Empty<string>())
        {
        }

        public Counter(string name, Dictionary<string, string> staticLabels, MetricSource source = null) :
            base(source, name, staticLabels, Array.Empty<string>())
        {
        }

        public Counter(string name, string[] labelNames, MetricSource source = null) :
            base(source, name, labelNames)
        {
        }

        public Counter(string name, Dictionary<string, string> staticLabels, string[] labelNames, MetricSource source = null) :
            base(source, name, staticLabels, labelNames)
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

    public class Counter<LabelType> : Meter<LabelType>
    {
        public Counter(string name, MetricSource source = null) :
    base(source, name)
        {
        }

        public Counter(string name, Dictionary<string, string> staticLabels, MetricSource source = null) :
            base(source, name, staticLabels)
        {
        }

        public override AggregationConfiguration DefaultAggregation => AggregationConfigurations.Sum;

        public void Add(double d, LabelType labelValues)
        {
            base.RecordMeasurement(d, labelValues);
        }

        public LabeledCounter<LabelType> WithLabels(LabelType labelValues)
        {
            //TODO: we should probably memoize this
            return new LabeledCounter<LabelType>(this, labelValues);
        }
    }

    public class LabeledCounter<LabelType> : LabeledMeter<Counter<LabelType>, LabelType>
    {
        internal LabeledCounter(Counter<LabelType> unlabled, LabelType labelValues) : base(unlabled, labelValues) { }

        public void Add(double d)
        {
            base.RecordMeasurement(d, LabelValues);
        }
    }
}
