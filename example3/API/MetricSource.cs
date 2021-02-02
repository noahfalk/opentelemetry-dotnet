using System;

namespace OpenTelmetry.Api
{
    public abstract class MetricSource
    {
        protected MetricSource oldSource = null;

        public MetricSource()
        {
        }

        public void RegisterSDK()
        {
            MetricBase.RegisterSDK(this);
        }

        public abstract bool OnCreate(MetricBase counter, LabelSet labels);

        public abstract bool OnRecord(MetricBase counter, int num, LabelSet boundLabels, LabelSet labels);

        public abstract bool OnRecord(MetricBase counter, double num, LabelSet boundLabels, LabelSet labels);
    }
}