using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;
using Microsoft.Diagnostics.Metric;

namespace OpenTelmetry.Api
{
    /// <summary>
    /// Allow Batching of recordings
    /// </summary>
    public class BatchMetricBuilder
    {
        private List<Tuple<MetricBase, object>> batches = new();
        private LabelSet labels;

        public BatchMetricBuilder(LabelSet labels)
        {
            this.labels = labels;
        }

        public BatchMetricBuilder RecordMetric<T>(MetricBase meter, T value)
        {
            batches.Add(Tuple.Create(meter, (object) value));
            return this;
        }

        public void Record()
        {
            // TODO: Need to optimize this

            var groupings = batches.GroupBy((k) => k.Item1.source, (j) => j, (k,v) => new {
                source = k,
                batch = v,
            });

            foreach (var group in groupings)
            {
                if (group.batch.Count() > 0)
                {
                    group.source.ReportValue(group.batch.ToList(), labels);
                }
            }
        }
    }
}