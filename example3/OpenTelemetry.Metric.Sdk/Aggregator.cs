using System;
using System.Collections.Generic;
using Microsoft.Diagnostics.Metric;
using OpenTelemetry.Metric.Sdk;

namespace OpenTelemetry.Metric.Sdk
{
    public abstract class Aggregator
    {
        public abstract AggregatorState CreateState();
    }

    public abstract class AggregatorState
    {
        public abstract void Update(MeterBase meter, double num);

        public abstract (string key, string value)[] Serialize();
    }

    public struct AggregatorKey : IEquatable<AggregatorKey>
    {
        public MetricSource source;
        public string name;
        public AggregationConfiguration AggregationConfig;
        public MetricLabelSet labels;

        public AggregatorKey(MetricSource source, string name, AggregationConfiguration aggregationConfig, MetricLabelSet labels)
        {
            this.source = source;
            this.name = name;
            this.AggregationConfig = aggregationConfig;
            this.labels = labels;
        }

        public bool Equals(AggregatorKey other)
        {
            var ret = this.name.Equals(other.name) &&
                this.source.Equals(other.source) &&
                this.AggregationConfig.Equals(other.AggregationConfig) &&
                this.labels.Equals(other.labels);
            return ret;
        }

        public override bool Equals(Object obj)
        {
            if (obj is AggregatorKey other)
            {
                return this.Equals(other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.name, this.source, this.AggregationConfig, this.labels);
        }
    }
}
