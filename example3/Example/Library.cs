using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Metric;
using OpenTelemetry.Metric.Api;

namespace MyLibrary
{
    public class Library
    {
        Counter counter_request;
        Counter counter_request2;
        Counter counter_request3;
        Guage guage_qsize;
        int count = 0;

        public Library(string name, CancellationToken token)
        {
            var labels = new MetricLabelSet(
                ("Program", "Test"),
                ("LibraryInstanceName", name));

            // Create in Default source

            counter_request = MetricSource.DefaultSource.CreateCounter("request2", labels);

            guage_qsize = new Guage(MetricSource.DefaultSource, "queue_size", MetricLabelSet.DefaultLabel, 
                new MetricLabelSet(
                    ("Description", "A measure of Queue size"),
                    ("DefaultAggregator", "Histogram"))
                );

            counter_request3 = new Counter("request3");

            // Setup a callback Observer for a meter
            counter_request3.SetObserver((m) => {
                int val = count;
                MetricLabelSet labels = new MetricLabelSet(
                    ("LibraryInstanceName", name),
                    ("Mode", "Observer"));
                return Tuple.Create((object)val, labels);
            });

            // Setup a task to observe Meter periodically
            Task t = new MeterObserverBuilder()
                .SetMetronome(1000)
                .AddMeter(counter_request3)
                .Run(token);

            // Create in custom source

            var source = MetricSource.GetSource("MyLibrary");

            counter_request2 = source.CreateCounter("requests", labels);

            var counter_registered = new Counter(source, "registered");
            counter_registered.Add(1, labels);
        }

        public void DoOperation()
        {
            // Example of recording 1 measurment

            var opernum = count % 3;

            var labels = new MetricLabelSet(("OperNum", $"{opernum}"));

            counter_request2.Add(1);

            counter_request2.Add(0.15, labels);

            //counter_request3.Observe();

            // Example of recording a batch of measurements

            var labels2 = new MetricLabelSet(
                ("OperNum", $"{opernum}"),
                ("Mode", "Batch"));

            new BatchMetricBuilder(labels2)
                .RecordMetric(counter_request, 1.0)
                .RecordMetric(guage_qsize, count)
                .RecordMetric(counter_request3, 1)
                .RecordMetric(counter_request3, 0.1)
                .Record();

            count++;
        }
    }
}