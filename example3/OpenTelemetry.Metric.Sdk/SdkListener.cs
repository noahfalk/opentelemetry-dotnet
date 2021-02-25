using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Linq;
using Microsoft.Diagnostics.Metric;
using OpenTelemetry.Metric.Api;

namespace OpenTelemetry.Metric.Sdk
{
    public class SdkListener : MetricListener
    {
        MetricProvider provider;

        public SdkListener(MetricProvider provider)
        {
            this.provider = provider;
        }

        public override bool OnCreate(MetricSource source, MetricBase meter)
        {
            // This SDK can store additional state data per meter
            if (meter is MeterBase otelMeter)
            {
                otelMeter.state = new ExtraSDKState();
            }

            return true;
        }

        public override bool OnRecord<T>(MetricSource source, MetricBase meter, T value, MetricLabelSet labels)
        {
            var dt = DateTimeOffset.UtcNow;

            return provider.OnRecord(meter, dt, value, labels);
        }

        public override bool OnRecord<T>(MetricSource source, IList<Tuple<MetricBase, T>> records, MetricLabelSet labels)
        {
            var dt = DateTimeOffset.UtcNow;

            // TODO: Need to handle as Atomic batch rather than dispatch individually

            foreach (var record in records)
            {
                provider.OnRecord(record.Item1, dt, record.Item2, labels);
            }

            return true;
        }
    }
}