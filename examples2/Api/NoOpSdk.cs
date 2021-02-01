using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Linq;

namespace Microsoft.OpenTelemetry.Api
{
    public class NoOpMetricProvider : IMetricProvider
    {
        public IMeter GetMeter(string name, string version)
        {
            return new NoOpMeter(name, version);
        }
    }

    public class NoOpMeter : IMeter
    {
        public NoOpMeter(string name, string version)
        {
        }

        public ICounter NewCounter(string name)
        {
            return new NoOpCounter(name);
        }

        public IBatchBuilder RecordBatch(ILabelSet labels)
        {
            return new NoOpBatchBulder(labels);
        }
    }

    public class NoOpCounter : ICounter
    {
        public NoOpCounter(string name)
        {
        }

        public void Add(int increment, ILabelSet labels)
        {
            // NOOP
        }

        public void Add(double increment, ILabelSet labels)
        {
            // NOOP
        }

        public IBoundCounter Bind(ILabelSet labels)
        {
            return new NoOpBoundCounter(this, labels);
        }
    }

    public class NoOpBoundCounter : IBoundCounter
    {
        ICounter counter;
        ILabelSet labels;

        public NoOpBoundCounter(ICounter counter, ILabelSet labels)
        {
            this.counter = counter;
            this.labels = labels;
        }

        public void Add(int increment)
        {
            counter.Add(increment, labels);
        }

        public void Add(double increment)
        {
            counter.Add(increment, labels);
        }

        public void Unbind()
        {
            // NOOP
        }
    }

    public class NoOpBatchBulder : IBatchBuilder
    {
        private ILabelSet labels;

        ConcurrentQueue<Action<ILabelSet>> actions = new();

        public NoOpBatchBulder(ILabelSet labels)
        {
            this.labels = labels;
        }

        public IBatchBuilder Add(ICounter counter, int value)
        {
            actions.Enqueue((ls) => counter.Add(value, ls));
            return this;
        }

        public IBatchBuilder Add(ICounter counter, double value)
        {
            actions.Enqueue((ls) => counter.Add(value, ls));
            return this;
        }

        public void Record()
        {
            var oldActions = Interlocked.Exchange(ref actions, new ConcurrentQueue<Action<ILabelSet>>());

            foreach (var act in oldActions)
            {
                act(labels);
            }

            oldActions.Clear();
        }
    }
}
