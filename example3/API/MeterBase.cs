using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;

namespace OpenTelmetry.Api
{
    public abstract class MeterBase
    {
        public MetricSource source { get; }
        public string MetricName { get; }
        public string MetricNamespace { get; }
        public string MetricType { get; }
        public LabelSet Labels { get; }
        public LabelSet Hints { get; }

        public virtual bool Enabled { get; set; } = true;

        protected Func<MeterBase, Tuple<object,LabelSet>> observer;

        // Allow custom Meters to store their own state
        public MeterState state { get; set; }

        protected MeterBase(MetricSource source, string name, string type, LabelSet labels, LabelSet hints)
        {
            MetricName = name;
            MetricNamespace = source.GetName();
            MetricType = type;
            Labels = labels;
            Hints = hints;
            this.source = source;

            // TODO: How to handle attach/detach of sources and listeners?
            foreach (var listener in source.GetListeners())
            {
                listener?.OnCreate(this, labels);
            }
        }

        protected void RecordMetricData<T>(T val, LabelSet labels)
        {
            if (Enabled)
            {
                foreach (var listener in source.GetListeners())
                {
                    listener?.OnRecord(this, val, labels);
                }
            }
        }

        public void SetObserver(Func<MeterBase, Tuple<object,LabelSet>> func)
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
