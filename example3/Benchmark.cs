using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Engines;

using OpenTelmetry.Api;
using OpenTelmetry.Sdk;

namespace MyBenchmarks
{
    //[SimpleJob(launchCount: 3, warmupCount: 10, targetCount: 30)]
    [MemoryDiagnoser]
    public class MetricValueBench
    {
        MetricValue iVal;
        MetricValue dVal;

        MetricValueSpan iSpan;
        MetricValueSpan dSpan;

        [GlobalSetup]
        public void Setup()
        {
            iVal = new MetricValue(10);
            dVal = new MetricValue(10.5);

            iSpan = new MetricValueSpan(10);
            dSpan = new MetricValueSpan(10.5);
        }

        [Benchmark]
        public MetricValue newInt()
        {
            return new MetricValue(10);
        }

        [Benchmark]
        public MetricValue newDouble()
        {
            return new MetricValue(10.1);
        }

        [Benchmark]
        public int toInt()
        {
            return iVal.ToInt32();
        }

        [Benchmark]
        public double toDouble()
        {
            return dVal.ToDouble();
        }

        [Benchmark]
        public MetricValueSpan newIntSpan()
        {
            return new MetricValueSpan(10);
        }

        [Benchmark]
        public MetricValueSpan newDoubleSpan()
        {
            return new MetricValueSpan(10.1);
        }

        [Benchmark]
        public int toIntSpan()
        {
            return iSpan.ToInt32();
        }

        [Benchmark]
        public double toDoubleSpan()
        {
            return dSpan.ToDouble();
        }
    }

    public class Program
    {
        public static void Run(string[] args)
        {
            var summary = BenchmarkRunner.Run<MetricValueBench>();
        }
    }
}