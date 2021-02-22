using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;
using Microsoft.Diagnostics.Metric;

namespace OpenTelemetry.Metric.Api
{
    public abstract class MeterBase : MetricBase
    {
        public MetricLabel Hints { get; }

        protected Func<MeterBase, Tuple<object,MetricLabel>> observer;

        // Allow custom Meters to store their own state
        public MeterState state { get; set; }

        protected MeterBase(MetricSource source, string name, string type, MetricLabel labels, MetricLabel hints)
            : base(source, name, type, labels)
        {
            Hints = hints;
        }

        public void SetObserver(Func<MeterBase, Tuple<object,MetricLabel>> func)
        {
            observer = func;
        }

        public void Observe()
        {
            if (Enabled && observer is not null)
            {
                var tup = observer(this);
                RecordMetricData(tup.Item1, tup.Item2);
            }
        }
    }
}
