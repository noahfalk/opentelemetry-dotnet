using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.OpenTelemetry.Api;

namespace Microsoft.OpenTelemetry.Sdk
{
    public class EventSourceCounter : Instrument, ICounter
    {
        ConcurrentQueue<EventItem> events = new();

        public EventSourceCounter(string name)
        {
            this.name = name;
        }

        public void Add(int increment, ILabelSet labels)
        {
            long ticks = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var value = ValueItemExtensions.Assign<int>(increment);
            var item = new EventItem() { name = name, ticks = ticks, value = value, labels = labels };
            events.Enqueue(item);
        }

        public void Add(double increment, ILabelSet labels)
        {
            long ticks = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var value = ValueItemExtensions.Assign<double>(increment);
            var item = new EventItem() { name = name, ticks = ticks, value = value, labels = labels };
            events.Enqueue(item);
        }

        public IBoundCounter Bind(ILabelSet labels)
        {
            return new BoundedCounter(this, labels);
        }

        public override List<DataItem> Collect(PipeBuilder pipe)
        {
            var collected = Interlocked.Exchange(ref events, new ConcurrentQueue<EventItem>());

            return collected.ToList<DataItem>();
        }
    }
}
