using System;

namespace Microsoft.Diagnostics.Metric
{
    public class MetricLabelSet
    {
        static private (string name, string value)[] emptyLabel = {};

        static private MetricLabelSet defaultLabel = new MetricLabelSet();

        static public MetricLabelSet DefaultLabel
        {
            get => defaultLabel;
        }

        /// <summary>
        /// Return Array of Tuple&lt;Key,Value&gt;.
        /// </summary>
        public virtual (string name, string value)[] GetLabels()
        {
            return emptyLabel;
        }
    }
}