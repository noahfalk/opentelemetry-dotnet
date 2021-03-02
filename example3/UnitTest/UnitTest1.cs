using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Microsoft.Diagnostics.Metric;
using Microsoft.OpenTelemetry.Export;
using OpenTelemetry.Metric.Api;
using OpenTelemetry.Metric.Sdk;

namespace UnitTest
{
    public class UnitTest1
    {
        [Fact]
        public void CheckSizesOfMetricVsMetric2()
        {
            var items = new List<ExportItem>();

            for (int n = 0; n < 5; n++)
            {
                var item = new ExportItem();
                item.dt = DateTimeOffset.Parse("2020-01-01T10:12:13Z");
                item.LibName = "Test";
                item.LibVersion = "0.0.1";
                item.MeterName = $"MyTest.request_{n}";
                item.Labels = new MetricLabelSet(("Host", "Test"), ("Mode", "Test"));
                item.AggregationConfig = new SumAggregation();
                item.AggData = new (string,string)[] {
                    ("sum","100.5"),
                    ("count","100"),
                    ("min","10.2"),
                    ("max","100")
                };
                items.Add(item);
            }

            var client = new ProtoBufClient();

            var bytes1 = client.Send(items.ToArray(), true);
            Assert.Equal(1818, bytes1.Length);
            //Console.WriteLine(BitConverter.ToString(bytes1));
            client.Receive(bytes1);

            var bytes2 = client.Send(items.ToArray(), false);
            Assert.Equal(1818, bytes2.Length);
            //Console.WriteLine(BitConverter.ToString(bytes2));
            client.Receive(bytes2);
        }
    }
}
