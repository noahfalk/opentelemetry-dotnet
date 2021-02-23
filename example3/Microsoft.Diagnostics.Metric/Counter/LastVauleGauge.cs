using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Metric
{
    public class LastVauleGauge : MetricBase
    {
        private static readonly object lockValues = new();

        private GaugeValueType status = GaugeValueType.Empty;
        private long lvalue = 0;
        private double dvalue = 0.0;

        private int periodInSeconds;
        private Task task;
        private CancellationTokenSource tokenSrc = new();

        public LastVauleGauge(MetricSource source, string name, MetricLabelSet labels) 
            : base(source, name, "LastValueGauge", labels)
        {
            this.periodInSeconds = 0;
            var token = tokenSrc.Token;
        }

        public LastVauleGauge(MetricSource source, string name, int periodInSeconds, MetricLabelSet labels) 
            : base(source, name, "LastValueGauge", labels)
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

        ~LastVauleGauge()
        {
            tokenSrc.Cancel();
            if (task is not null)
            {
                task.Wait();
            }
        }

        public void Report<T>(T delta)
        {
            if (delta is int ival)
            {
                lock (lockValues)
                {
                    this.lvalue = ival;
                    this.status = GaugeValueType.LongValue;
                }
            }
            else if (delta is long lval)
            {
                lock (lockValues)
                {
                    this.lvalue = lval;
                    this.status = GaugeValueType.LongValue;
                }
            }
            else if (delta is double dval)
            {
                lock (lockValues)
                {
                    this.dvalue = dval;
                    this.status = GaugeValueType.DoubleValue;
                }
            }

            if (this.periodInSeconds == 0)
            {
                this.Flush();
            }
        }

        private void Flush()
        {
            GaugeValueType status;
            long lvalue;
            double dvalue;

            lock (lockValues)
            {
                status = this.status;
                lvalue = this.lvalue;
                dvalue = this.dvalue;
                this.status = GaugeValueType.Empty;
            }

            switch (status)
            {
                case GaugeValueType.LongValue:
                    RecordMetricData(lvalue, MetricLabelSet.DefaultLabel);
                    break;

                case GaugeValueType.DoubleValue:
                    RecordMetricData(dvalue, MetricLabelSet.DefaultLabel);
                    break;

                default:
                    break;
            }
        }

        protected enum GaugeValueType
        {
            Empty,
            LongValue,
            DoubleValue
        }
    }
}