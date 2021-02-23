using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Metric
{
    public class RateCounter : MetricBase
    {
        private long count = 0;
        private int periodInSeconds;
        private Task task;
        private CancellationTokenSource tokenSrc = new();

        private long lastTick;

        public RateCounter(MetricSource source, string name, int periodInSeconds, MetricLabel labels) 
            : base(source, name, "RateCounter", labels)
        {
            this.periodInSeconds = periodInSeconds;
            var token = tokenSrc.Token;

            this.task = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(this.periodInSeconds * 1000, token);
                    }
                    catch (TaskCanceledException)
                    {
                        // Do Nothing
                    }

                    this.Flush();
                }
            });

            this.lastTick = DateTimeOffset.UtcNow.UtcTicks;
        }

        ~RateCounter()
        {
            tokenSrc.Cancel();
            task.Wait();
        }

        public void Mark()
        {
            Interlocked.Increment(ref this.count);
        }

        private void Flush()
        {
            var curTick = DateTimeOffset.UtcNow.UtcTicks;
            var lastTick = Interlocked.Exchange(ref this.lastTick, curTick);

            var cnt = Interlocked.Exchange(ref this.count, 0);
            if (cnt > 0)
            {
                long elapsed = curTick - lastTick;

                double rate = (cnt * 100) / (elapsed / 100000);
                RecordMetricData(rate, MetricLabel.DefaultLabel);
            }
        }
    }
}