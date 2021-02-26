using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace OpenTelemetry.Metric.Api
{
    public class MeterObserverBuilder
    {
        private List<MeterBase> meters = new();

        int period;

        public MeterObserverBuilder()
        {
        }

        public MeterObserverBuilder SetMetronome(int periodMilli)
        {
            this.period = periodMilli;
            return this;
        }

        public MeterObserverBuilder AddMeter(MeterBase meter)
        {
            meters.Add(meter);
            return this;
        }

        public Task Run(CancellationToken token)
        {
            return Task.Run(async () => {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(period, token);
                    }
                    catch (TaskCanceledException)
                    {
                        // Do Nothing
                    }

                    RunObserver();
                }
            });
        }

        private void RunObserver()
        {
            foreach (var meter in meters)
            {
                meter.Observe();
            }
        }
    }
}