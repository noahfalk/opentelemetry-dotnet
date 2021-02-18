using System;

namespace Microsoft.Diagnostics.Metric
{
    public class MetricLabel
    {
        static private (string name, string value)[] emptyLabel = {};

        static private MetricLabel defaultLabel = new MetricLabel();

        static public MetricLabel DefaultLabel
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