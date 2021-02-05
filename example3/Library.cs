using OpenTelmetry.Api;

namespace MyLibrary
{
    public class Library
    {
        Counter counter_request;
        Counter counter_request2;
        Counter counter_request3;
        Guage guage_qsize;
        int count = 0;

        public Library(string name)
        {
            var labels = new LabelSet(new string[] { 
                "Program", "Test",
                "LibraryInstanceName", name,
                });

            // Create in Default provider

            counter_request = MetricProvider.DefaultProvider.CreateCounter("request2", labels);

            guage_qsize = new Guage(MetricProvider.DefaultProvider, "queue_size", LabelSet.Empty, 
                new LabelSet(new string[] { 
                    "Description", "A measure of Queue size",
                    "DefaultAggregator", "Histogram",
                    })
                );

            counter_request3 = new Counter("request3");


            // Create in custom provider

            var provider = MetricProvider.GetProvider("MyLibrary");

            counter_request2 = provider.CreateCounter("requests", labels);

            var counter_registered = new Counter(provider, "registered");
            counter_registered.Add(1, labels);
        }

        public void DoOperation()
        {
            // Example of recording 1 measurment

            var opernum = count % 3;

            var labels = new LabelSet(new string[] { 
                "OperNum", $"{opernum}",
                });

            counter_request2.Add(1);

            counter_request2.Add(0.15, labels);


            // Example of recording a batch of measurements

            var labels2 = new LabelSet(new string[] { 
                "OperNum", $"{opernum}",
                "Mode", "Batch",
                });

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