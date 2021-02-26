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
        public abstract void Update<T>(MetricBase meter, T num, MetricLabelSet labels);

        public abstract (string key, string value)[] Serialize();
    }

    public struct AggregatorKey : IEquatable<AggregatorKey>
    {
        public string ns;
        public string name;
        public string type;
        public string aggType;
        public MetricLabelSet labels;

        public AggregatorKey(string ns, string name, string type, string aggType, MetricLabelSet labels)
        {
            this.ns = ns;
            this.name = name;
            this.type = type;
            this.aggType = aggType;
            this.labels = labels;
        }

        public bool Equals(AggregatorKey other)
        {
            var ret = this.ns.Equals(other.ns) &&
                this.name.Equals(other.name) &&
                this.type.Equals(other.type) &&
                this.aggType.Equals(other.aggType) &&
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
            return HashCode.Combine(this.ns, this.name, this.type, this.aggType, this.labels);
        }
    }

    public class ExtraSDKState : MeterState
    {
        // TODO: SDK can store additional state data for each meter
    }
}
