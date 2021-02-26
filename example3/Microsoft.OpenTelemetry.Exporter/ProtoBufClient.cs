using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using OpenTelemetry.Metric.Sdk;
using Opentelemetry.Proto.Metrics.V1;
using Opentelemetry.Proto.Common.V1;

namespace Microsoft.OpenTelemetry.Export
{
    public class ProtoBufClient
    {
        public ProtoBufClient()
        {
        }

        public byte[] Send(ExportItem item)
        {
            if (item.AggType == "SumCountMinMax")
            {
                Metric metric = new Metric();
                metric.Name = $"{item.ProviderName}::{item.MeterName}.{item.InstrumentName} [{item.InstrumentType}]";
                var sum = new DoubleSum();
                metric.DoubleSum = sum;
                sum.IsMonotonic = true;
                var datapoints = sum.DataPoints;

                var datapoint = new DoubleDataPoint();
                datapoint.StartTimeUnixNano = (ulong) item.dt.ToUnixTimeMilliseconds() * 100000;
                datapoint.TimeUnixNano = (ulong) item.dt.ToUnixTimeMilliseconds() * 100000;

                foreach (var l in item.Labels.GetLabels())
                {
                    var kv = new StringKeyValue();
                    kv.Key = l.name;
                    kv.Value = l.value;
                    datapoint.Labels.Add(kv);
                }

                foreach (var d in item.AggData)
                {
                    if (d.name == "sum")
                    {
                        datapoint.Value = double.Parse(d.value);
                        break;
                    }
                }
                datapoints.Add(datapoint);

                var bytes = new byte[1000];
                var outstream = new CodedOutputStream(bytes);
                metric.WriteTo(outstream);

                var msg = new Span<byte>(bytes, 0, (int) outstream.Position);

                return msg.ToArray();
            }

            return new byte[0] {};
        }

        public void Receive(byte[] bytes)
        {
            if (bytes.Length > 0)
            {
                Console.WriteLine($"Received {bytes.Length} bytes");

                var parser = new Google.Protobuf.MessageParser<Metric>(() => new Metric());
                var inMetric = parser.ParseFrom(bytes);

                Console.WriteLine($"  Name: {inMetric.Name}");
                if (inMetric.DoubleSum is not null)
                {
                    foreach (var dp in inMetric.DoubleSum.DataPoints)
                    {
                        var items = new List<string>();

                        if (dp.Labels is not null)
                        {
                            foreach (var l in dp.Labels)
                            {
                                items.Add($"{l.Key}={l.Value}");
                            }
                        }

                        items.Sort();

                        Console.WriteLine($"  {dp.Value} [{String.Join("|", items)}]");
                    }
                }
            }
        }
    }
}