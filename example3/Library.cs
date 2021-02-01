using OpenTelmetry.Api;

namespace MyLibrary
{
    public class Library
    {
        Counter counter;

        public Library()
        {
            counter = new Counter("MyLibrary", "requests");
        }

        public void DoOperation()
        {
            counter.Add(10);
        }
    }
}