using Microsoft.OpenTelemetry.Api;

namespace Microsoft.OpenTelemetry.Sdk
{
    public class DataItem
    {
        public string name;
        public long ticks;
        public ILabelSet labels;
    }

    public class EventItem : DataItem
    {
        public IValueItem value;
    }

    public class SummaryItem : DataItem
    {
        public long count;
        public double sum;
        public double min;
        public double max;
    }
}