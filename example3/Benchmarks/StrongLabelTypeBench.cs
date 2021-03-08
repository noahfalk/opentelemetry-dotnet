using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Engines;

using OpenTelemetry.Metric.Sdk;
using Microsoft.Diagnostics.Metric;

namespace MyBenchmark
{
    /*
     *  BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19042
        Intel Core i7-9700K CPU 3.60GHz (Coffee Lake), 1 CPU, 8 logical and 8 physical cores
        .NET Core SDK=5.0.103
          [Host]     : .NET Core 5.0.3 (CoreCLR 5.0.321.7212, CoreFX 5.0.321.7212), X64 RyuJIT
          DefaultJob : .NET Core 5.0.3 (CoreCLR 5.0.321.7212, CoreFX 5.0.321.7212), X64 RyuJIT


        |                            Method |      Mean |     Error |    StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
        |---------------------------------- |----------:|----------:|----------:|-------:|------:|------:|----------:|
        |                   WeakTypedRecord |  7.808 ns | 0.0690 ns | 0.0612 ns | 0.0076 |     - |     - |      48 B |
        |                 StrongTypedRecord |  2.975 ns | 0.0243 ns | 0.0227 ns |      - |     - |     - |         - |
        |       WeakTypedRecordWithListener |  9.054 ns | 0.0872 ns | 0.0728 ns | 0.0076 |     - |     - |      48 B |
        |     StrongTypedRecordWithListener | 18.628 ns | 0.1522 ns | 0.1423 ns | 0.0076 |     - |     - |      48 B |
        |   WeakTypedRecordWithFastListener |  8.152 ns | 0.0803 ns | 0.0671 ns | 0.0076 |     - |     - |      48 B |
        | StrongTypedRecordWithFastListener |  8.417 ns | 0.0597 ns | 0.0558 ns |      - |     - |     - |         - |
     */

    [MemoryDiagnoser]
    public class StrongLabelTypeBench
    {
        static Gauge weakTyped = new Gauge("weak", new string[] { "Dim1", "Dim2", "Dim3" });
        static Gauge weakTypedWithListener = new Gauge("weak_listener", new string[] { "Dim1", "Dim2", "Dim3" });
        static Gauge weakTypedWithFastListener = new Gauge("weak_fastlistener", new string[] { "Dim1", "Dim2", "Dim3" });
        struct Labels
        {
            public string Dim1 { get; set; }
            public string Dim2 { get; set; }
            public string Dim3 { get; set; }
        }
        static Gauge<Labels> strongTyped = new Gauge<Labels>("strong");
        static Gauge<Labels> strongTypedWithListener = new Gauge<Labels>("strong_listener");
        static Gauge<Labels> strongTypedWithFastListener = new Gauge<Labels>("strong_fastlistener");

        static MeterListener listener;
        static FastListener fastListener;

        static StrongLabelTypeBench()
        {
            listener = new MeterListener()
            {
                MeterPublished = (m, opt) =>
                {
                    if (m.Name.EndsWith("_listener")) opt.Subscribe();
                },
                MeasurementRecorded = (m,meas,l,c) => { }
            };
            listener.Start();
            fastListener = new FastListener()
            {
                MeterPublished = (m, opt) =>
                {
                    if (m.Name.EndsWith("_fastlistener")) opt.Subscribe();
                }
            };
            fastListener.Start();
        }

        [Benchmark]
        public void WeakTypedRecord()
        {
            weakTyped.Set(19.3, "a", "b", "c");
        }

        [Benchmark]
        public void StrongTypedRecord()
        {
            strongTyped.Set(19.3, new Labels() { Dim1 = "a", Dim2 = "b", Dim3 = "c" });
        }

        [Benchmark]
        public void WeakTypedRecordWithListener()
        {
            weakTypedWithListener.Set(19.3, "a", "b", "c");
        }

        [Benchmark]
        public void StrongTypedRecordWithListener()
        {
            strongTypedWithListener.Set(19.3, new Labels() { Dim1 = "a", Dim2 = "b", Dim3 = "c" });
        }

        [Benchmark]
        public void WeakTypedRecordWithFastListener()
        {
            weakTypedWithFastListener.Set(19.3, "a", "b", "c");
        }

        [Benchmark]
        public void StrongTypedRecordWithFastListener()
        {
            strongTypedWithFastListener.Set(19.3, new Labels() { Dim1 = "a", Dim2 = "b", Dim3 = "c" });
        }
    }

    class FastListener : MeterListener
    {
        protected override void RecordMeasurement<LabelsType>(MeterBase<LabelsType> meter, double measurement, LabelsType labelValues, object listenerCookie)
        {
            
        }
    }
}
