using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MyLibrary;
using Microsoft.Diagnostics.Metric;
using OpenTelemetry.Metric.Sdk;
using Microsoft.OpenTelemetry.Export;

namespace Example
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var pgm = new Program();
            await pgm.Run();
        }

        public async Task Run()
        {
            // Example of setting up a SDK

            var sdk = new MetricProvider()
                .Name("MyProgram")
                .SetCollectionPeriod(4000)

                // Use Queue to ensure constant time/space per measurement recording
                .UseQueue()

                // Add Metrics
                .Include("MyLibrary")

                // Add Filters.  Order matters.  Can be stacked.
                //.AddMetricInclusion("/MyLibrary/")
                //.AddMetricExclusion("/queue_size/")
                //.AddMetricInclusion("/_Total")

                // Configure what Labels are important
                /* 
                 * TODO
                .AggregateByLabels(new SumCountMinMax(), 
                    new MetricLabelSet(
                        ("LibraryInstanceName", "*")),
                    new MetricLabelSet(
                        ("LibraryInstanceName", "*"), 
                        ("Mode", "*")),
                    new MetricLabelSet(
                        ("OperName", "*"), 
                        ("Mode", "Batch")))
                */

                //.AddExporter(new ConsoleExporter("export1", 6000))
                .AddExporter(new OTLPExporter(10, 6000))

                // Finalize pipeline
                .Build()
                ;

            // Do our operations
            await RunOperation(5000);

            // Stop our SDK
            sdk.Stop();
        }

        public async Task RunOperation(int periodMilliseconds)
        {
            var rand = new Random();

            var cancelToken = new CancellationTokenSource();
            var token = cancelToken.Token;

            var taskList = new List<Task>();

            taskList.Add(Task.Run(async () => {
                var lib = new Library("Library_1", token);
                while (!token.IsCancellationRequested)
                {
                    lib.DoOperation();
                    await Task.Delay((rand.Next() % 10) * 100);
                }
            }));

            /*
            taskList.Add(Task.Run(async () => {
                var lib = new Library("Library_2", token);

                while (!token.IsCancellationRequested)
                {
                    lib.DoOperation();
                    await Task.Delay(200);
                }
            }));

            taskList.Add(Task.Run(async () => {
                var sum = new Counter("Sum");
                var lastvalue = new Gauge("Last");

                while (!token.IsCancellationRequested)
                {
                    sum.Add(rand.Next() % 100);
                    lastvalue.Set(rand.Next() % 100);

                    await Task.Delay(50);
                }
            }));
            */

            await Task.Delay(periodMilliseconds);
            cancelToken.Cancel();

            await Task.WhenAll(taskList.ToArray());
        }
    }
}
