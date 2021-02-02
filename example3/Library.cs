using OpenTelmetry.Api;

namespace MyLibrary
{
    public class Library
    {
        Counter counter;
        int count = 0;

        public Library(string name)
        {
            var labels = new LabelSet( new string[] { "LibraryInstanceName", name });

            counter = new Counter("MyLibrary", "requests", labels);
        }

        public void DoOperation()
        {
            var labels = new LabelSet( new string[] { "OperNum", $"{count%3}" });
            counter.Add(10.1, labels);

            counter.Add(2);

            count++;
        }
    }
}