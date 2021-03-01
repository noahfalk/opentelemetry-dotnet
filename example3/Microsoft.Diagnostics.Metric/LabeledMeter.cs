using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Metric
{
    public abstract class LabeledMeter : MeterBase
    {
        public string[] LabelValues { get; }

        protected LabeledMeter(string[] labelValues)
        {
            LabelValues = labelValues;
        }
    }

    public abstract class LabeledMeter<T> : LabeledMeter where T : Meter
    {
        public T Unlabled { get; }
        public override string Name => Unlabled.Name;
        public override Dictionary<string, string> StaticLabels => Unlabled.StaticLabels;
        public override AggregationConfiguration DefaultAggregation => Unlabled.DefaultAggregation;
        public override string[] LabelNames => Unlabled.LabelNames;
        
        protected LabeledMeter(T unlabeledMeter, string[] labelValues) : base(labelValues)
        {
            Unlabled = unlabeledMeter;
            MeterCollection.Instance.AddMetric(this);
        }
    }
}
