using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Metric
{
    public class SumCounter : MetricBase
    {
        private long icount = 0;
        private long isum = 0;

        private long dcount = 0;
        private double dsum = 0.0;
        private static readonly object lockSum = new();

        private int periodInSeconds;
        private Task task;
        private CancellationTokenSource tokenSrc = new();

        public SumCounter(MetricSource source, string name, int periodInSeconds, MetricLabel labels) 
            : base(source, name, "SumCounter", labels)
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
                Interlocked.Add(ref isum, ival);
                Interlocked.Increment(ref icount);
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
            var icount = Interlocked.Exchange(ref this.icount, 0);
            if (icount > 0)
            {
                var isum = Interlocked.Exchange(ref this.isum, 0);
                RecordMetricData(isum, MetricLabel.DefaultLabel);
            }

            var dcount = Interlocked.Exchange(ref this.dcount, 0);
            if (dcount > 0)
            {
                var dsum = Interlocked.Exchange(ref this.dsum, 0.0);
                RecordMetricData(dsum, MetricLabel.DefaultLabel);
            }
        }
    }
}