using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Engines;

using OpenTelemetry.Metric.Sdk;

namespace MyBenchmark
{
    /*
     *  BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19042
        Intel Core i7-9700K CPU 3.60GHz (Coffee Lake), 1 CPU, 8 logical and 8 physical cores
        .NET Core SDK=5.0.103
          [Host]     : .NET Core 5.0.3 (CoreCLR 5.0.321.7212, CoreFX 5.0.321.7212), X64 RyuJIT
          DefaultJob : .NET Core 5.0.3 (CoreCLR 5.0.321.7212, CoreFX 5.0.321.7212), X64 RyuJIT


        |      Method |     Mean |     Error |    StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
        |------------ |---------:|----------:|----------:|-------:|------:|------:|----------:|
        | IntToString | 7.219 ns | 0.0379 ns | 0.0296 ns | 0.0051 |     - |     - |      32 B |
    */

    [MemoryDiagnoser]
    public class StringConversionBench
    {
        int x = 503;

        [Benchmark]
        public string IntToString()
        {
            return x.ToString();
        }
    }
}
