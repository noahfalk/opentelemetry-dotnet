using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Linq;
using OpenTelmetry.Api;

namespace OpenTelmetry.Sdk
{
    public class SdkListener : MetricListener
    {
        SampleSdk sdk;

        public SdkListener(SampleSdk sdk)
        {
            this.sdk = sdk;
        }

        public override bool OnCreate(MeterBase meter, LabelSet labels)
        {
            // This SDK can store additional state data per meter
            meter.state = new ExtraSDKState();

            return true;
        }

        public override bool OnRecord<T>(MeterBase meter, T value, LabelSet labels)
        {
            var dt = DateTimeOffset.UtcNow;

            return sdk.OnRecord(meter, dt, value, labels);
        }

        public override bool OnRecord<T>(IList<Tuple<MeterBase, T>> records, LabelSet labels)
        {
            var dt = DateTimeOffset.UtcNow;

            // TODO: Need to handle as Atomic batch rather than dispatch individually

            foreach (var record in records)
            {
                var meter = record.Item1;
                var value = record.Item2;
                sdk.OnRecord(meter, dt, value, labels);
            }

            return true;
        }
    }
}