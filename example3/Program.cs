using System;
using MyLibrary;
using OpenTelmetry.Sdk;

namespace example3
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start...");

            var sdk = new SampleSdk()
                .Name("MyProgram")
                .Build();

            var lib = new Library();

            for (int c = 0; c < 40; c++)
            {
                lib.DoOperation();

                if ((c % 10) == 9)
                {
                    sdk.Collect();
                }
            }

            sdk.Collect();

            Console.WriteLine("Done.");
        }
    }
}
