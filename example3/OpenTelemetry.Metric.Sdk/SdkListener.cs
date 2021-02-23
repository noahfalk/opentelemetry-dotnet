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
        MetricPipeline sdk;

        public SdkListener(MetricPipeline sdk)
        {
            this.sdk = sdk;
        }

        public override bool OnCreate(MetricSource source, MetricBase meter, MetricLabel labels)
        {
            // This SDK can store additional state data per meter
            if (meter is MeterBase otelMeter)
            {
                otelMeter.state = new ExtraSDKState();
            }

            return true;
        }

        public override bool OnRecord<T>(MetricSource source, MetricBase meter, T value, MetricLabel labels)
        {
            var dt = DateTimeOffset.UtcNow;

            return sdk.OnRecord(meter, dt, value, labels);
        }

        public override bool OnRecord<T>(MetricSource source, IList<Tuple<MetricBase, T>> records, MetricLabel labels)
        {
            var dt = DateTimeOffset.UtcNow;

            // TODO: Need to handle as Atomic batch rather than dispatch individually

            foreach (var record in records)
            {
                sdk.OnRecord(record.Item1, dt, record.Item2, labels);
            }

            return true;
        }
    }
}