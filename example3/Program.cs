using System;
using System.Threading.Tasks;
using MyLibrary;
using OpenTelmetry.Api;
using OpenTelmetry.Sdk;

namespace example3
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Example of setting up a SDK

            var sdk = new SampleSdk()
                .Name("MyProgram")
                .SetCollectionPeriod(4000)
                .AttachProvider(MetricProvider.DefaultProvider)
                .AttachProvider("MyLibrary")
                //.AddNamespaceExclusion("MyLibrary/Library_2")
                .Build()
                ;

            // Do our operations
            var pgm = new Program();
            await pgm.Run(5000);

            // Stop our SDK
            sdk.Stop();
        }

        public async Task Run(int periodMilliseconds)
        {
            var rand = new Random();

            bool isQuit = false;

            Task t1 = Task.Run(async () => {
                var lib = new Library("Library_1");
                while (!isQuit)
                {
                    lib.DoOperation();
                    await Task.Delay((rand.Next() % 10) * 100);
                }
            });

            Task t2 = Task.Run(async () => {
                var lib = new Library("Library_2");
                while (!isQuit)
                {
                    lib.DoOperation();
                    await Task.Delay(200);
                }
            });

            await Task.Delay(periodMilliseconds);
            isQuit = true;

            await t1;
            await t2;
        }
    }
}
