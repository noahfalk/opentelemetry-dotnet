using System;
using System.Threading.Tasks;
using Microsoft.OpenTelemetry.Api;
using Microsoft.OpenTelemetry.Sdk;

namespace example2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Started");

            var provider = new PipeBuilder()
                .DropLabel("bound")
                .FilterEvent("op", "3")
                .Export()
                .AggCountSumMinMax()
                .Export()
                .Build();

            MetricProvider.SetProvider(provider);

            var lib = new Library();

            lib.DoOperation();
            lib.DoOperation();
            for (int c = 0; c < 20; c++)
            {
                //lib.DoOperation();
                Task.Delay(300).Wait();
            }

            lib.EndOperation();

            Console.WriteLine("Done");
        }
    }
}
