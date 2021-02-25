using System;

namespace Microsoft.Diagnostics.Metric
{
    public class MetricLabelSet
    {
        static private (string name, string value)[] emptyLabel = {};
        
        static private MetricLabelSet defaultLabel = new MetricLabelSet();

        private (string name, string value)[] labels = {};

        static public MetricLabelSet DefaultLabelSet
        {
            get => defaultLabel;
        }

        public MetricLabelSet()
        {
            labels = emptyLabel;
        }

        public MetricLabelSet(params (string name, string value)[] labels)
        {
            this.labels = labels;
        }

        /// <summary>
        /// Return Array of Tuple&lt;Key,Value&gt;.
        /// </summary>
        public virtual (string name, string value)[] GetLabels()
        {
            return this.labels;
        }
    }
}