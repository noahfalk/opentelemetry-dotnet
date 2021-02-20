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

            if (meter is MeterBase otelMeter && labels is LabelSet labelset)
            {
                return sdk.OnRecord(otelMeter, dt, value, labelset);
            }

            return false;
        }

        public override bool OnRecord<T>(MetricSource source, IList<Tuple<MetricBase, T>> records, MetricLabel labels)
        {
            var dt = DateTimeOffset.UtcNow;

            // TODO: Need to handle as Atomic batch rather than dispatch individually

            foreach (var record in records)
            {
                if (record.Item1 is MeterBase otelMeter && labels is LabelSet labelset)
                {
                    var value = record.Item2;
                    sdk.OnRecord(otelMeter, dt, value, labelset);
                }
            }

            return true;
        }
    }
}