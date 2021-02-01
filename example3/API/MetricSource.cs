using System;

namespace OpenTelmetry.Api
{
    public abstract class MetricSource
    {
        public MetricSource()
        {
            CounterBase.RegisterSDK(this);
        }

        public abstract void OnCreate(CounterBase counter);

        public abstract bool Record(CounterBase counter, int num);
    }
}