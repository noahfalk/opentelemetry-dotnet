using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Metric;

namespace MyLibrary
{
    public class Library
    {

        static Counter s_c1 = new Counter("c1", new string[] { "Label1", "Label2" });

        LabeledCounter _c1;
        Counter counter_request;
        Counter counter_request2;
        Counter counter_request3;
        Gauge gauge_qsize;
        int count = 0;

        public Library(string name, CancellationToken token)
        {
            _c1 = s_c1.WithLabels(name, "Tomato");

            var staticLabels = new Dictionary<string, string>()
            {
                {  "Program", "Test" },
                { "LibraryInstanceName", name }
            };

            counter_request = new Counter("MyLibrary.request2", staticLabels);

            gauge_qsize = new Gauge("MyLibrary.queue_size");

            //TODO: make this async
            counter_request3 = new Counter("MyLibrary.request3");

            counter_request2 = new Counter("MyLibrary.requests", staticLabels,
                new string[] { "OperNum" });

            var counter_registered = new Counter("MyLibrary.registered",
                labelNames: new string[] { "Program", "LibraryInstanceName" });
            counter_registered.Add(1, "test", name);
        }

        public void DoOperation()
        {

            _c1.Add(5);

            // Example of recording 1 measurment

            var opernum = count % 3;

            //var labels = new MetricLabelSet(("OperNum", $"{opernum}"));

            
            counter_request2.Add(1);

            counter_request2.Add(0.15, $"{opernum}");

            //counter_request3.Observe();


            //I am proposing there is no batching API
            /*
            // Example of recording a batch of measurements

            var labels2 = new MetricLabelSet(
                ("OperNum", $"{opernum}"),
                ("Mode", "Batch"));

            
            new BatchMetricBuilder(labels2)
                .RecordMetric(counter_request, 1.0)
                .RecordMetric(gauge_qsize, count)
                .RecordMetric(counter_request3, 1)
                .RecordMetric(counter_request3, 0.1)
                .Record();
            */

            count++;
        }
    }
}
