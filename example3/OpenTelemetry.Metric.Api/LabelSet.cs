using System;
using Microsoft.Diagnostics.Metric;

namespace OpenTelemetry.Metric.Api
{
    public class LabelSet : MetricLabel
    {
        private (string name, string value)[] labels = {};

        public LabelSet(MetricLabel label)
        {
            this.labels = label.GetLabels();
        }

        public LabelSet(params (string name, string value)[] labels)
        {
            this.labels = labels;
        }

        /// <summary>
        /// Return Array of Tuple&lt;Key,Value&gt;.
        /// </summary>
        public override (string name, string value)[] GetLabels()
        {
            return labels;
        }
    }
}