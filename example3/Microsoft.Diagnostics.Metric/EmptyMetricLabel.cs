using System;

namespace Microsoft.Diagnostics.Metric
{
    public sealed class EmptyMetricLabel : MetricLabelSet
    {
        static private (string name, string value)[] emptyTuple = {};

        public override (string name, string value)[] GetLabels()
        {
            return emptyTuple;
        }
    }
}