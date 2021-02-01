using System;
using System.Threading.Tasks;
using MyLibrary;
using OpenTelmetry.Sdk;

namespace example3
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Start...");

            var rand = new Random();

            var sdk = new SampleSdk()
                .Name("MyProgram")
                .SetCollectionPeriod(2000)
                .Build();

            Task t1 = Task.Run(async () => {
                var lib = new Library("Library_1");
                while (!sdk.IsStop())
                {
                    lib.DoOperation();
                    await Task.Delay((rand.Next() % 10) * 100);
                }
            });

            Task t2 = Task.Run(async () => {
                var lib = new Library("Library_2");
                while (!sdk.IsStop())
                {
                    lib.DoOperation();
                    await Task.Delay(200);
                }
            });

            await Task.Delay(5000);
            sdk.Stop();

            await t1;
            await t2;

            Console.WriteLine("Done.");
        }
    }
}
