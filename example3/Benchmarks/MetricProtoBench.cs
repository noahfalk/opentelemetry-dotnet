using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Engines;

using Microsoft.Diagnostics.Metric;
using Microsoft.OpenTelemetry.Export;
using OpenTelemetry.Metric.Api;
using OpenTelemetry.Metric.Sdk;

/*
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19042
Intel Core i7-1065G7 CPU 1.30GHz, 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.103
  [Host]     : .NET Core 5.0.3 (CoreCLR 5.0.321.7212, CoreFX 5.0.321.7212), X64 RyuJIT
  Job-GMTSFC : .NET Core 5.0.3 (CoreCLR 5.0.321.7212, CoreFX 5.0.321.7212), X64 RyuJIT

IterationCount=15  LaunchCount=3  WarmupCount=3

|         Method |     Mean |    Error |   StdDev |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|--------------- |---------:|---------:|---------:|--------:|-------:|------:|----------:|
|     SendMetric | 40.98 us | 0.601 us | 1.113 us |  7.6294 |      - |     - |  31.23 KB |
|    SendMetric2 | 40.14 us | 0.328 us | 0.592 us |  7.9956 | 0.0610 |     - |   32.8 KB |
|  ReceiveMetric | 49.04 us | 0.623 us | 1.185 us | 14.2822 | 0.0610 |     - |  58.52 KB |
| ReceiveMetric2 | 48.42 us | 0.846 us | 1.547 us | 14.6484 |      - |     - |  60.09 KB |
*/

namespace MyBenchmark
{
    [SimpleJob(launchCount: 3, warmupCount: 3, targetCount: 15)]
    [MemoryDiagnoser]
    public class MetricProtoBench
    {
        private ProtoBufClient client;
        ExportItem[] items;
        byte[] bytes1;
        byte[] bytes2;

        [GlobalSetup]
        public void Setup()
        {
            var items = new List<ExportItem>();

            for (int i = 0; i < 10; i++)
            {
                var item = new ExportItem();
                item.dt = DateTimeOffset.Parse("2020-01-01T10:12:13Z");
                item.LibName = "Test";
                item.LibVersion = "0.0.1";
                item.MeterName = $"MyTest.request_{i}";
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

            this.items = items.ToArray();

            client = new ProtoBufClient();

            bytes1 = client.Send(items.ToArray(), true);
            bytes2 = client.Send(items.ToArray(), false);
        }

        [Benchmark]
        public byte[] SendMetric()
        {
            return client.Send(items, true);
        }

        [Benchmark]
        public byte[] SendMetric2()
        {
            return client.Send(items, false);
        }

        [Benchmark]
        public ProtoBufClient.ParseRecord[] ReceiveMetric()
        {
            return client.ParsePayload(bytes1);
        }

        [Benchmark]
        public ProtoBufClient.ParseRecord[] ReceiveMetric2()
        {
            return client.ParsePayload(bytes2);
        }
    }
}
