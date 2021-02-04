using OpenTelmetry.Api;

namespace MyLibrary
{
    public class Library
    {
        Counter counter;
        Counter counter2;
        Guage guage;
        int count = 0;

        public Library(string name)
        {
            var labels = new LabelSet( new string[] { "LibraryInstanceName", name });

            // Create in custom provider

            var provider = new MetricProvider("MyLibrary");
            counter = provider.CreateCounter("requests", labels);

            // Create in Default provider

            counter2 = MetricProvider.Default.CreateCounter("request2", labels);

            guage = new Guage("queue_size");
        }

        public void DoOperation()
        {
            var labels = new LabelSet( new string[] { "OperNum", $"{count%3}" });

            // Example of recording 1 measurment

            counter.Add(1.15, labels);

            counter.Add(2);

            counter2.Add(3.14);

            guage.Record(count);

            // Example of recording a batch of measurements

            new MeterBase.BatchBuilder(labels)
                .RecordMeasurement(counter, 100)
                .RecordMeasurement(guage, 10.4)
                .Record();

            count++;
        }
    }
}