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
    public class SampleMetricProvider : IMetricProvider
    {
        private PipeBuilder pipe = null;

        public SampleMetricProvider(PipeBuilder pipe)
        {
            this.pipe = pipe;
        }

        public IMeter GetMeter(string name, string version)
        {
            return new SampleMeter(name, version, pipe);
        }
    }

    public class SampleMeter : IMeter
    {
        private string name;
        private string version;
        private Task collector;

        ConcurrentDictionary<string,Instrument> registry = new();

        PipeBuilder pipe;

        public SampleMeter(string name, string version, PipeBuilder pipe)
        {
            this.name = name;
            this.version = version;
            this.pipe = pipe;

            this.collector = Task.Run(async ()=>
            {
                while (true)
                {
                    Console.WriteLine("Summary...");
                    var summary = Collect();
                    Console.WriteLine(summary);

                    await Task.Delay(1000);
                }
            });
        }

        public ICounter NewCounter(string name)
        {
            var instrument = registry.GetOrAdd($"ES:{name}", k => new EventSourceCounter(k));
            var counter = instrument as ICounter;

            return counter;
        }

        public IBatchBuilder RecordBatch(ILabelSet labelSet)
        {
            return new BatchBuilder(labelSet);
        }

        private string Collect()
        {
            StringBuilder sb = new();

            List<DataItem> aggList = new();

            foreach (var kv in registry)
            {
                var name = kv.Key;
                var summary = kv.Value.Collect(pipe);
                aggList.AddRange(summary);
            }

            pipe.RunAll(aggList);

            return sb.ToString();
        }
    }

    public class BatchBuilder : IBatchBuilder
    {
        private readonly object l = new();

        private LabelSet labels;

        List<Action<LabelSet>> actions = new();

        public BatchBuilder(ILabelSet labels)
        {
            this.labels = new LabelSet(labels);
        }

        public IBatchBuilder Add(ICounter counter, int value)
        {
            Action<LabelSet> act = ls => counter.Add(value, ls);
            actions.Add(act);

            return this;
        }

        public IBatchBuilder Add(ICounter counter, double value)
        {
            Action<LabelSet> act = ls => counter.Add(value, ls);
            actions.Add(act);

            return this;
        }

        public void Record()
        {
            lock (l)
            {
                foreach (var act in actions)
                {
                    act(labels);
                }
            }
        }
    }

    public class BoundedCounter : IBoundCounter
    {
        BoundValue value;
        ICounter counter;

        static private readonly object lockRegistry = new();

        static private Dictionary<string,BoundValue> registry = new();

        public BoundedCounter(ICounter counter, ILabelSet labels)
        {
            this.counter = counter;
            var bondLabels = new LabelSet(labels);
            var key = bondLabels.Export();

            lock (lockRegistry)
            {
                if (registry.TryGetValue(key, out var v))
                {
                    value = v;
                }
                else
                {
                    value = new BoundValue(key, bondLabels);
                    registry.Add(key, value);
                }

                if (value.counters.Add(counter))
                {
                    value.count++;
                }
            }
        }

        public void Add(int increment)
        {
            counter.Add(increment, value.labels);
        }

        public void Add(double increment)
        {
            counter.Add(increment, value.labels);
        }

        public void Unbind()
        {
            lock (lockRegistry)
            {
                if (value.counters.Remove(counter))
                {
                    value.count--;

                    if (value.count == 0)
                    {
                        registry.Remove(value.key);
                    }
                }
            }
        }

        public class BoundValue
        {
            public HashSet<ICounter> counters = new();

            public int count = 0;

            public LabelSet labels;

            public string key;

            public BoundValue(string key, LabelSet labels)
            {
                this.labels = labels;
                this.key = key;
            }
        }
    }
}
