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

        public abstract bool OnCreate(MetricBase counter);

        public abstract bool OnRecord(MetricBase counter, int num);

        public abstract bool OnRecord(MetricBase counter, double num);
    }
}