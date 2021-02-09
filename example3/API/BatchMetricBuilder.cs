using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;

namespace OpenTelmetry.Api
{
    /// <summary>
    /// Allow Batching of recordings
    /// </summary>
    public class BatchMetricBuilder
    {
        private List<Tuple<MeterBase, object>> batches = new();
        private LabelSet labels;

        public BatchMetricBuilder(LabelSet labels)
        {
            this.labels = labels;
        }

        public BatchMetricBuilder RecordMetric<T>(MeterBase meter, T value)
        {
            batches.Add(Tuple.Create(meter, (object) value));
            return this;
        }

        public void Record()
        {
            // TODO: Need to optimize this

            var groupings = batches.GroupBy((k) => k.Item1.provider, (j) => j, (k,v) => new {
                provider = k,
                batch = v,
            });

            foreach (var group in groupings)
            {
                if (group.batch.Count() > 0)
                {
                    foreach (var listener in group.provider.GetListeners())
                    {
                        listener.OnRecord(group.batch.ToList(), labels);
                    }
                }
            }
        }
    }
}