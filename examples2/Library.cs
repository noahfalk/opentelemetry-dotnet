using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.OpenTelemetry.Api;

namespace example2
{
    public class Library
    {
        IMeter meter;
        ICounter counter;
        ICounter counter2;
        IBoundCounter boundCounter;

        int oper = 0;

        public Library()
        {
            meter = MetricProvider.GetProvider().GetMeter("Library", "1.0");

            counter = meter.NewCounter("test");

            counter2 = meter.NewCounter("test2");

            var ls = new DictionaryLabelSet(new Dictionary<string,string>(){{"bound","true"}});
            boundCounter = counter.Bind(ls);
        }

        public void DoOperation()
        {
            var localcounter = meter.NewCounter($"oper_{oper%3}");
            var ls = new DictionaryLabelSet(new Dictionary<string,string>()
                {
                    { "add", "basic" }
                });
            localcounter.Add(50, ls);

            var ls2 = new DictionaryLabelSet(new Dictionary<string,string>()
                {
                    { "add", "basic" },
                    { "op", $"{oper%5}" }
                });
            counter.Add(10, ls2);
            counter.Add(10.2, ls2);

            boundCounter.Add(20);

            var ls3 = new DictionaryLabelSet(new Dictionary<string,string>()
                {
                    { "batch", "true" },
                    { "op", $"{oper%5}" }
                });
            meter.RecordBatch(ls3)
                .Add(counter, 30)
                .Add(counter2, 40.2)
                .Record();

            oper++;
        }

        public void EndOperation()
        {
            boundCounter.Unbind();
        }
    }
}
