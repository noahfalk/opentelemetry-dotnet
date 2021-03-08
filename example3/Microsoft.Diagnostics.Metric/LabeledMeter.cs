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

    public abstract class LabeledMeter<UnlabledMeterType> : LabeledMeter where UnlabledMeterType : Meter
    {
        public UnlabledMeterType Unlabeled { get; }
        public override MetricSource Source => Unlabeled.Source;
        public override string Name => Unlabeled.Name;
        public override Dictionary<string, string> StaticLabels => Unlabeled.StaticLabels;
        public override AggregationConfiguration DefaultAggregation => Unlabeled.DefaultAggregation;
        public override string[] LabelNames => Unlabeled.LabelNames;
        
        protected LabeledMeter(UnlabledMeterType unlabeledMeter, string[] labelValues) : base(labelValues)
        {
            Unlabeled = unlabeledMeter;
            MeterCollection.Instance.AddMetric(this);
        }
    }

    public abstract class LabeledMeter<UnlabeledMeterType, LabelType> : MeterBase<LabelType> where UnlabeledMeterType : Meter<LabelType>
    {
        public UnlabeledMeterType Unlabeled { get; }
        public LabelType LabelValues { get; }
        public override MetricSource Source => Unlabeled.Source;
        public override string Name => Unlabeled.Name;
        public override Dictionary<string, string> StaticLabels => Unlabeled.StaticLabels;
        public override AggregationConfiguration DefaultAggregation => Unlabeled.DefaultAggregation;
        public override string[] LabelNames => Unlabeled.LabelNames;

        protected LabeledMeter(UnlabeledMeterType unlabeledMeter, LabelType labelValues)
        {
            Unlabeled = unlabeledMeter;
            LabelValues = labelValues;
            MeterCollection.Instance.AddMetric(this);
        }
    }
}
