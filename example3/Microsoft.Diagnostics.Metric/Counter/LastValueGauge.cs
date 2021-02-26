using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Metric
{
    public class LastValueGauge : MetricBase
    {
        private static readonly object lockValues = new();

        private GaugeValueType status = GaugeValueType.Empty;
        private long lvalue = 0;
        private double dvalue = 0.0;

        private int periodInSeconds;
        private Task task;
        private CancellationTokenSource tokenSrc = new();

        public LastValueGauge(MetricSource source, string name, MetricLabelSet labels, MetricLabelSet hints)
            : base(source, name, nameof(LastValueGauge), labels, AddDefaultHints(hints))
        {
            init(0, hints);
        }

        public LastValueGauge(MetricSource source, string name, int periodInSeconds, MetricLabelSet labels, MetricLabelSet hints)
            : base(source, name, nameof(LastValueGauge), labels, AddDefaultHints(hints))
        {
            init(Math.Max(periodInSeconds,1), hints);
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
                newHints.Add(("DefaultAggregator", "LastValueAggregator"));
                hints = new MetricLabelSet(newHints.ToArray());
            }

            return hints;
        }

        private void init(int periodInSeconds, MetricLabelSet hints)
        {
            this.periodInSeconds = periodInSeconds;
            var token = tokenSrc.Token;

            if (periodInSeconds > 0)
            {
                this.task = Task.Run(async () => {
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
        }

        ~LastValueGauge()
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
                    RecordMetricData(lvalue, MetricLabelSet.DefaultLabelSet);
                    break;

                case GaugeValueType.DoubleValue:
                    RecordMetricData(dvalue, MetricLabelSet.DefaultLabelSet);
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