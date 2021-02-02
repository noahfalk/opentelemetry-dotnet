using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using OpenTelmetry.Api;

namespace OpenTelmetry.Sdk
{
    public class SampleSdk : MetricSource
    {
        private readonly object lockCounters = new();
        private List<MetricBase> counters = new();

        private string name;
        private int collectPeriod_ms = 2000;
        private bool isBuilt = false;
        private bool isExitRequest = false;

        private List<string> namespaceExclusionList = new();

        public SampleSdk Name(string name)
        {
            this.name = name;
            return this;
        }

        public SampleSdk AddNamespaceExclusion(string ns)
        {
            namespaceExclusionList.Add(ns);
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

            base.RegisterSDK();

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

        public override bool OnCreate(MetricBase counter)
        {
            lock (lockCounters)
            {
                counters.Add(counter);
                counter.state = new SumDataState();
            }

            return true;
        }

        public override bool OnRecord(MetricBase counter, int num)
        {
            // TODO: Start with a SumData<int> and promoted to SumData<double> as necessary.

            return OnRecord(counter, (double) num);
        }

        public override bool OnRecord(MetricBase counter, double num)
        {
            // TODO: Start with a SumData<int> and promoted to SumData<double> as necessary.

            if (isBuilt && counter.Enabled)
            {
                if (counter.state is SumDataState data)
                {
                    lock (data.lockSumData)
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

        private void Collect()
        {
            if (isBuilt)
            {
                foreach (var counter in counters)
                {
                    if (counter.state is SumDataState data)
                    {
                        if (data.count > 0)
                        {
                            counter.state = new SumDataState();

                            var ns = counter.MetricNamespace;
                            var name = counter.MetricName;
                            var type = counter.MetricType;

                            Console.WriteLine($"[{type}]{ns}:{name} = n={data.count}, sum={data.sum}, min={data.min}, max={data.max}");
                        }
                    }
                }
            }
        }

        public class SumDataState : MetricState
        {
            public readonly object lockSumData = new();

            public int count = 0;
            public double sum = 0;
            public double max = 0;
            public double min = 0;
        }
    }
}