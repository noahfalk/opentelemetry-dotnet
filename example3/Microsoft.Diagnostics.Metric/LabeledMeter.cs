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
        public T Unlabeled { get; }
        public override MetricSource Source => Unlabeled.Source;
        public override string Name => Unlabeled.Name;
        public override Dictionary<string, string> StaticLabels => Unlabeled.StaticLabels;
        public override AggregationConfiguration DefaultAggregation => Unlabeled.DefaultAggregation;
        public override string[] LabelNames => Unlabeled.LabelNames;
        
        protected LabeledMeter(T unlabeledMeter, string[] labelValues) : base(labelValues)
        {
            Unlabeled = unlabeledMeter;
            MeterCollection.Instance.AddMetric(this);
        }
    }
}
