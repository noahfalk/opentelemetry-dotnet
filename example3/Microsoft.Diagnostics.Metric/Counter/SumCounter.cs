using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Metric
{
    public class SumCounter : MetricBase
    {
        private long lcount = 0;
        private long lsum = 0;

        private long dcount = 0;
        private double dsum = 0.0;
        private static readonly object lockSum = new();

        private int periodInSeconds;
        private Task task;
        private CancellationTokenSource tokenSrc = new();

        public SumCounter(MetricSource source, string name, int periodInSeconds, MetricLabelSet labels) 
            : base(source, name, "SumCounter", labels)
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
        }

        ~SumCounter()
        {
            tokenSrc.Cancel();
            task.Wait();
        }

        public void Add<T>(T delta)
        {
            if (delta is int ival)
            {
                Interlocked.Add(ref lsum, (long) ival);
                Interlocked.Increment(ref lcount);
            }
            else if (delta is long lval)
            {
                Interlocked.Add(ref lsum, lval);
                Interlocked.Increment(ref lcount);
            }
            else if (delta is double dval)
            {
                lock (lockSum)
                {
                    dsum += dval;
                }
                Interlocked.Increment(ref dcount);
            }
        }

        private void Flush()
        {
            var icount = Interlocked.Exchange(ref this.lcount, 0);
            if (icount > 0)
            {
                var isum = Interlocked.Exchange(ref this.lsum, 0);
                RecordMetricData(isum, MetricLabelSet.DefaultLabel);
            }

            var dcount = Interlocked.Exchange(ref this.dcount, 0);
            if (dcount > 0)
            {
                var dsum = Interlocked.Exchange(ref this.dsum, 0.0);
                RecordMetricData(dsum, MetricLabelSet.DefaultLabel);
            }
        }
    }
}