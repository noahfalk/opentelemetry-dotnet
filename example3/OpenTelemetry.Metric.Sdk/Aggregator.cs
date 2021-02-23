using System;
using System.Collections.Generic;
using Microsoft.Diagnostics.Metric;
using OpenTelemetry.Metric.Api;
using OpenTelemetry.Metric.Sdk;

namespace OpenTelemetry.Metric.Sdk
{
    public abstract class Aggregator
    {
        public abstract AggregatorState CreateState();
    }

    public abstract class AggregatorState
    {
        public abstract void Update<T>(MetricBase meter, T num, MetricLabel labels);

        public abstract (string key, string value)[] Serialize();
    }

    public class ExtraSDKState : MeterState
    {
        // TODO: SDK can store additional state data for each meter
    }
}
