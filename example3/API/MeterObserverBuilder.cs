using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace OpenTelmetry.Api
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
                try
                {
                    await Task.Delay(period, token);

                    while (!token.IsCancellationRequested)
                    {
                        RunObserver();
                        
                        await Task.Delay(period, token);
                    }
                }
                catch (TaskCanceledException)
                {
                    // Ignore
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