using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace OpenTelemetry.Metric.Sdk
{
    public class ConsoleExporter : Exporter
    {
        private string name;
        private Task exportTask;

        private ConcurrentQueue<ExportItem> queue = new();

        public ConsoleExporter(string name, int periodMilli)
        {
            this.name = name;
        }

        public override void Export(ExportItem[] exports)
        {
            foreach (var item in exports)
            {
                queue.Enqueue(item);
            }
        }

        public override void Start(CancellationToken token)
        {
            exportTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        if (!queue.IsEmpty)
                        {
                            var exportQue = Interlocked.Exchange(ref queue, new ConcurrentQueue<ExportItem>());

                            Send(exportQue);

                            await Task.Delay(100, token);
                        }
                        else
                        {
                            await Task.Delay(1000, token);
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        // Do Nothing
                    }

                    Send(queue);
                }
            });
        }

        public override void Stop()
        {
            exportTask.Wait();
        }

        private void Send(ConcurrentQueue<ExportItem> exportQue)
        {
            Console.WriteLine($"*** Export {name}...");

            while (exportQue.TryDequeue(out var item))
            {
                if (item is StringExportItem msg)
                {
                    Console.WriteLine(msg.item);
                }
            }
        }
    }

    public class StringExportItem : ExportItem
    {
        public string item { get; }

        public StringExportItem(string item)
        {
            this.item = item;
        }
    }
}