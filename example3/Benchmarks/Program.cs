using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Engines;

namespace MyBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                switch (arg.ToLower())
                {
                    case "metricvalue":
                        var metricValueSummary = BenchmarkRunner.Run<MetricValueBench>();
                        break;

                    case "labelset":
                        var labelsetSummary = BenchmarkRunner.Run<LabelSetBench>();
                        break;

                    case "proto":
                        var proto = BenchmarkRunner.Run<MetricProtoBench>();
                        break;

                    case "tostring":
                        var toString = BenchmarkRunner.Run<StringConversionBench>();
                        break;

                    case "stronglabel":
                        var strongLabel = BenchmarkRunner.Run<StrongLabelTypeBench>();
                        break;
                }
            }
        }
    }
}
