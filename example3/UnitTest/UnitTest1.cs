using System;
using System.Collections.Generic;
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

            var item = new ExportItem();
            item.dt = DateTimeOffset.UtcNow;
            item.LibName = "Test";
            item.LibVersion = "0.0.1";
            item.MeterName = "MyTest.request";
            item.Labels = new MetricLabelSet(("Host", "Test"), ("Mode", "Test"));
            item.AggregationConfig = new SumAggregation();
            item.AggData = new (string,string)[] {
                ("sum","100"),
                ("count","100"),
                ("min","10"),
                ("max","100")
            };
            items.Add(item);

            var client = new ProtoBufClient();

            var bytes1 = client.Send(items.ToArray(), true);
            Assert.Equal(410, bytes1.Length);

            var bytes2 = client.Send(items.ToArray(), false);
            Assert.Equal(410, bytes2.Length);
        }
    }
}
