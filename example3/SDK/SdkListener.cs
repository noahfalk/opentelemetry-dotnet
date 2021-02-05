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

        public override bool OnRecord(MeterBase meter, MetricValue value, LabelSet labels)
        {
            var dt = DateTimeOffset.UtcNow;

            return sdk.OnRecord(meter, dt, value, labels);
        }

        public override bool OnRecord(IList<Tuple<MeterBase, MetricValue>> records, LabelSet labels)
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