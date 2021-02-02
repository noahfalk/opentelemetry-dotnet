using OpenTelmetry.Api;

namespace MyLibrary
{
    public class Library
    {
        Counter counter;

        public Library(string name)
        {
            counter = new Counter($"MyLibrary/{name}", "requests");
        }

        public void DoOperation()
        {
            counter.Add(10.1);
        }
    }
}