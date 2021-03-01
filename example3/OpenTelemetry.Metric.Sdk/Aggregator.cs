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
        public string name;
        public string libname;
        public string libver;
        public AggregationConfiguration AggregationConfig;
        public MetricLabelSet labels;

        public AggregatorKey(string libName, string libVer, string name, AggregationConfiguration aggregationConfig, MetricLabelSet labels)
        {
            this.libname = libName;
            this.libver = libVer;
            this.name = name;
            this.AggregationConfig = aggregationConfig;
            this.labels = labels;
        }

        public bool Equals(AggregatorKey other)
        {
            var ret = this.name.Equals(other.name) &&
                this.libname.Equals(other.libname) &&
                this.libver.Equals(other.libver) &&
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
            return HashCode.Combine(this.name, this.libname, this.libver, this.AggregationConfig, this.labels);
        }
    }
}
