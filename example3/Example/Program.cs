using System;
using System.Threading;
using System.Threading.Tasks;
using MyLibrary;
using Microsoft.Diagnostics.Metric;
using OpenTelemetry.Metric.Api;
using OpenTelemetry.Metric.Sdk;

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

            var sdk = new MetricPipeline()
                .Name("MyProgram")
                .SetCollectionPeriod(4000)

                // Use Queue to ensure constant time/space per measurement recording
                .UseQueue()

                // Add Metric Sources
                .AttachSource(MetricSource.DefaultSource)
                .AttachSource("MyLibrary")

                // Add Filters.  Order matters.  Can be stacked.
                //.AddMetricInclusion("/MyLibrary/")
                //.AddMetricExclusion("/queue_size/")
                //.AddMetricInclusion("/_Total")

                // Configure what Labels are important
                .AggregateByLabels(new SumCountMinMax(), 
                    new LabelSet(
                        ("LibraryInstanceName", "*")),
                    new LabelSet(
                        ("LibraryInstanceName", "*"), 
                        ("Mode", "*")),
                    new LabelSet(
                        ("OperName", "*"), 
                        ("Mode", "Batch")))

                .AddExporter(new ConsoleExporter("export1", 6000))

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

            Task t1 = Task.Run(async () => {
                var lib = new Library("Library_1", token);
                while (!token.IsCancellationRequested)
                {
                    lib.DoOperation();
                    await Task.Delay((rand.Next() % 10) * 100);
                }
            });

            Task t2 = Task.Run(async () => {
                var lib = new Library("Library_2", token);

                while (!token.IsCancellationRequested)
                {
                    lib.DoOperation();
                    await Task.Delay(200);
                }
            });

            Task t3 = Task.Run(async () => {
                var rate = new RateCounter(MetricSource.DefaultSource, "RateCounter", 1, MetricLabel.DefaultLabel);

                while (!token.IsCancellationRequested)
                {
                    rate.Mark();
                    await Task.Delay(50);
                }
            });

            await Task.Delay(periodMilliseconds);
            cancelToken.Cancel();

            await t1;
            await t2;
            await t3;
        }
    }
}
