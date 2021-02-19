using System;

namespace Microsoft.Diagnostics.Metric
{
    public sealed class EmptyMetricLabel : MetricLabel
    {
        static private (string name, string value)[] emptyTuple = {};

        public override (string name, string value)[] GetLabels()
        {
            return emptyTuple;
        }
    }
}