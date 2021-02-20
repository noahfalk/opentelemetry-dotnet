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
                    await Task.Delay(this.periodInSeconds * 1000);
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
            Interlocked.Increment(ref count);
        }

        private void Flush()
        {
            var curTick = DateTimeOffset.UtcNow.UtcTicks;

            var lastTick = Interlocked.Exchange(ref this.lastTick, curTick);
            var cnt = Interlocked.Exchange(ref count, 0);

            long elapsed = (curTick - lastTick) / 100000;

            double rate = 0.0;
            if (elapsed > 0)
            {
                rate = cnt * 100 / elapsed;
            }

            RecordMetricData(rate, MetricLabel.DefaultLabel);
        }
    }
}