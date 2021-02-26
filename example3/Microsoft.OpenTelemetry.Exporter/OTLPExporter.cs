using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using OpenTelemetry.Metric.Api;
using OpenTelemetry.Metric.Sdk;

namespace Microsoft.OpenTelemetry.Export
{
    public class OTLPExporter : Exporter
    {
        private Task exportTask;
        private ConcurrentQueue<ExportItem> queue = new();
        private int periodMilli;

        public OTLPExporter(int periodMilli)
        {
            this.periodMilli = periodMilli;
        }

        public override void Export(ExportItem[] exports)
        {
            foreach (var export in exports)
            {
                queue.Enqueue(export);
            }
        }

        public override void Start(CancellationToken token)
        {
            exportTask = Task.Run(async () => {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(this.periodMilli, token);
                    }
                    catch (TaskCanceledException)
                    {
                        // Do Nothing
                    }

                    Process();
                }
            });
        }

        public override void Stop()
        {
            exportTask.Wait();
        }

        public void Process()
        {
            Console.WriteLine("OTLP Exporter...");

            var que = Interlocked.Exchange(ref queue, new ConcurrentQueue<ExportItem>());

            var groups = que.GroupBy(
                k => (k.ProviderName, k.MeterName, k.InstrumentType, k.InstrumentName), 
                v => v,
                (k,v) => (k,v)
                );

            var sortedList = groups.ToList();
            sortedList.Sort((x,y) => {
                int ret;

                ret = String.Compare(x.k.ProviderName, y.k.ProviderName, true);
                if (ret != 0) return ret;

                ret = String.Compare(x.k.MeterName, y.k.MeterName, true);
                if (ret != 0) return ret;

                ret = String.Compare(x.k.InstrumentName, y.k.InstrumentName, true);
                if (ret != 0) return ret;

                ret = String.Compare(x.k.InstrumentType, y.k.InstrumentType, true);
                if (ret != 0) return ret;

                return 0;
            });

            foreach (var group in sortedList)
            {
                Console.WriteLine($"{group.k.MeterName}.{group.k.InstrumentName} [Kind={group.k.InstrumentType}] [Provider={group.k.ProviderName}]");

                var items = new List<string>();

                foreach (var q in group.v)
                {
                    var aggdata = q.AggData.Select(k => $"{k.name}={k.value}");
                    var dim = String.Join( " | ", q.Labels.GetLabels().Select(k => $"{k.name}={k.value}"));
                    if (dim == "")
                    {
                        dim = "{_Total}";
                    }
                    items.Add($"    {dim}{Environment.NewLine}" +
                        $"        {q.AggType}: {String.Join("|", aggdata)}");
                }

                items.Sort();
                foreach (var item in items)
                {
                    Console.WriteLine(item);
                }
            }

            // foreach (var q in que)
            // {
            //     var aggdata = q.AggData.Select(k => $"{k.name}={k.value}");
            //     var dim = String.Join( " | ", q.Labels.GetLabels().Select(k => $"{k.name}={k.value}"));
            //     Console.WriteLine($"    {q.ProviderName} {q.MeterName} {q.InstrumentType} {q.InstrumentName} | {dim}{Environment.NewLine}" +
            //         $"        {q.AggType}: {String.Join("|", aggdata)}");
            // }
        }
    }
}
