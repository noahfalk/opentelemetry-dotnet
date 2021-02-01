using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenTelmetry.Api;

namespace OpenTelmetry.Sdk
{
    public class SampleSdk : MetricSource
    {
        private readonly object lockCounters = new();
        private List<CounterBase> counters = new();

        private string name;
        private int collectPeriod_ms = 2000;
        private bool isBuilt = false;
        private bool isExitRequest = false;

        public SampleSdk Name(string name)
        {
            this.name = name;
            return this;
        }

        public SampleSdk SetCollectionPeriod(int milliseconds)
        {
            collectPeriod_ms = milliseconds;
            return this;
        }

        public SampleSdk Build()
        {
            isBuilt = true;

            // Start Periodic Collection Task
            Task.Run(async () => {
                while (!isExitRequest)
                {
                    await Task.Delay(this.collectPeriod_ms);
                    Collect();
                }
            });

            return this;
        }

        public bool IsStop()
        {
            return isExitRequest;
        }
        
        public void Stop()
        {
            isExitRequest = true;

            Task.Delay(2 * collectPeriod_ms).Wait();
        }

        public override bool Record(CounterBase counter, int num)
        {
            if (isBuilt)
            {
                if (counter.sdkdata is CounterData data)
                {
                    lock (data.l)
                    {
                        data.count++;
                        data.sum += num;
                        data.min = Math.Min(data.min, num);
                        data.max = Math.Max(data.max, num);
                    }

                    return true;
                }
            }

            return false;
        }

        public override void OnCreate(CounterBase counter)
        {
            counter.sdkdata = new CounterData();
            lock (lockCounters)
            {
                counters.Add(counter);
            }
        }

        public void Collect()
        {
            if (isBuilt)
            {
                foreach (var counter in counters)
                {
                    if (counter.sdkdata is CounterData data)
                    {
                        if (data.count > 0)
                        {
                            counter.sdkdata = new CounterData();

                            var ns = counter.GetNameSpace();
                            var name = counter.GetName();
                            var type = counter.GetCounterType();

                            Console.WriteLine($"[{type}]{ns}:{name} = n={data.count}, sum={data.sum}, min={data.min}, max={data.max}");
                        }
                    }
                }
            }
        }
    }

    public class CounterData
    {
        public readonly object l = new();

        public int count = 0;
        public int sum = 0;
        public int max = 0;
        public int min = 0;
    }
}