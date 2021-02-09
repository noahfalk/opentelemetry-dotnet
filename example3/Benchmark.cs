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

        MetricValueField iField;
        MetricValueField dField;

        MetricValueGeneric<int> iGeneric;
        MetricValueGeneric<double> dGeneric;

        [GlobalSetup]
        public void Setup()
        {
            iVal = new MetricValue(10);
            dVal = new MetricValue(10.5);

            iSpan = new MetricValueSpan(10);
            dSpan = new MetricValueSpan(10.5);

            iField = new MetricValueField(10);
            dField = new MetricValueField(10.5);

            iGeneric = new MetricValueGeneric<int>(10);
            dGeneric = new MetricValueGeneric<double>(10.5);
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

        [Benchmark]
        public MetricValueField newIntField()
        {
            return new MetricValueField(10);
        }

        [Benchmark]
        public MetricValueField newDoubleField()
        {
            return new MetricValueField(10.1);
        }

        [Benchmark]
        public int toIntField()
        {
            return iField.ToInt32();
        }

        [Benchmark]
        public double toDoubleField()
        {
            return dField.ToDouble();
        }

        [Benchmark]
        public MetricValueGeneric<int> newIntGeneric()
        {
            return new MetricValueGeneric<int>(10);
        }

        [Benchmark]
        public MetricValueGeneric<double> newDoubleGeneric()
        {
            return new MetricValueGeneric<double>(10.1);
        }

        [Benchmark]
        public int toIntGeneric()
        {
            return iGeneric.ToInt32();
        }

        [Benchmark]
        public double toDoubleGeneric()
        {
            return dGeneric.ToDouble();
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