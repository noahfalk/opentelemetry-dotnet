using System;
using System.Collections.Generic;
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

        public RateCounter(MetricSource source, string name, int periodInSeconds, MetricLabelSet labels, MetricLabelSet hints)
            : base(source, name, "RateCounter", labels, AddDefaultHints(hints))
        {
            this.periodInSeconds = Math.Min(periodInSeconds, 1);
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

        private static MetricLabelSet AddDefaultHints(MetricLabelSet hints)
        {
            // Add DefaultAggregator hints if does not exists

            var newHints = new List<(string name, string value)>();

            var foundDefaultAggregator = false;
            foreach (var hint in hints.GetLabels())
            {
                if (hint.name == "DefaultAggregator")
                {
                    foundDefaultAggregator = true;
                }

                newHints.Add(hint);
            }
            
            if (!foundDefaultAggregator)
            {
                newHints.Add(("DefaultAggregator", "Histogram"));
                hints = new MetricLabelSet(newHints.ToArray());
            }

            return hints;
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
                RecordMetricData(rate, MetricLabelSet.DefaultLabelSet);
            }
        }
    }
}