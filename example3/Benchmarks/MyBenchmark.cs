using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Reports;

using OpenTelmetry.Api;
using OpenTelmetry.Sdk;

namespace MyBenchmark
{
    public class Program
    {
        public static void Run(string[] args)
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
                }
            }
        }
    }
}