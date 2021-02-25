using System;
using System.Collections.Generic;

namespace Microsoft.Diagnostics.Metric
{
    public abstract class MetricListener
    {
        /// <summary>
        /// Let SDK know when new counters are being created
        /// </summary>
        public abstract bool OnCreate(MetricSource source, MetricBase counter);

        // TODO: Represent int/double as a generic class so we don't need two OnRecord() function
        // TODO: Need discussion of carrying native number or BOX up the number into generic class

        /// <summary>
        /// Let SDK know when new measures are recorded
        /// </summary>
        public abstract bool OnRecord<T>(MetricSource source, MetricBase meter, T value, MetricLabelSet labels);

        /// <summary>
        /// Allow multiple measurements to be recorded atomicly
        /// </summary>
        public abstract bool OnRecord<T>(MetricSource source, IList<Tuple<MetricBase, T>> records, MetricLabelSet labels);
    }
}