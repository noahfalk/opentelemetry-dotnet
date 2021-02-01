using System;
using System.Collections.Generic;

namespace Microsoft.OpenTelemetry.Sdk
{
    public class PipeBuilder
    {
        private List<Func<IList<DataItem>,IList<DataItem>>> agg = new();

        public IList<DataItem> RunAll(IList<DataItem> collection)
        {
            foreach (var func in agg)
            {
                collection = func(collection);
            }

            return collection;
        }

        public PipeBuilder DropLabel(string name)
        {
            agg.Add((collected) => {
                List<DataItem> outp = new();

                foreach (var item in collected)
                {
                    bool isAdd = true;

                    if (item is EventItem evnt)
                    {
                        var ls = item.labels.GetLabels();
                        if (ls.ContainsKey(name))
                        {
                            ls.Remove(name);
                            outp.Add(item);

                            isAdd = false;
                        }
                    }

                    if (isAdd)
                    {
                        outp.Add(item);
                    }
                }

                return outp;
            });
            return this;
        }

        public PipeBuilder FilterEvent(string name, string value)
        {
            agg.Add((collected) => {
                List<DataItem> outp = new();

                foreach (var item in collected)
                {
                    bool isAdd = true;

                    if (item is EventItem evnt)
                    {
                        var ls = item.labels.GetLabels();
                        if (ls.TryGetValue(name, out string itemvalue))
                        {
                            if (itemvalue == value)
                            {
                                isAdd = false;
                            }
                        }
                    }

                    if (isAdd)
                    {
                        outp.Add(item);
                    }
                }

                return outp;
            });
            return this;
        }

        public PipeBuilder AggCountSumMinMax()
        {
            agg.Add((collected) => {
                List<DataItem> outp = new();

                Dictionary<string, CountSumMinMax> buckets = new();

                foreach (var item in collected)
                {
                    if (item is EventItem evnt)
                    {
                        var bucketkey = $"{evnt.name}\t*=*";
                        CountSumMinMax csmm;
                        if (buckets.TryGetValue(bucketkey, out var bucket))
                        {
                            csmm = bucket;
                        }
                        else
                        {
                            csmm = new CountSumMinMax(evnt.name);
                            buckets[bucketkey] = csmm;
                        }
                        csmm.Update(evnt.value);

                        // Update for each labelset

                        foreach (var kv in evnt.labels.GetLabels())
                        {
                            var key = kv.Key;
                            var value = kv.Value;
                            bucketkey = $"{evnt.name}\t{key}={value}";

                            if (buckets.TryGetValue(bucketkey, out bucket))
                            {
                                csmm = bucket;
                            }
                            else
                            {
                                csmm = new CountSumMinMax(evnt.name);
                                buckets[bucketkey] = csmm;
                            }
                            csmm.Update(evnt.value);
                        }
                    }
                    else
                    {
                        outp.Add(item);
                    }
                }
                collected.Clear();

                foreach (var kv in buckets)
                {
                    var summary = new SummaryItem()
                    {
                        name = kv.Value.name,
                        count = kv.Value.count,
                        sum = kv.Value.sum,
                        min = kv.Value.min,
                        max = kv.Value.max,
                    };
                    var fields = kv.Key.Split("\t");
                    summary.labels = new LabelSet(fields[1]);

                    outp.Add(summary);
                }

                return outp;
            });

            return this;
        }

        public PipeBuilder Export()
        {
            agg.Add((collected) => {
                foreach (var item in collected)
                {
                    var ls = new LabelSet(item.labels);
                    var key = ls.Export();

                    var msg = "Unknown";
                    if (item is EventItem evnt)
                    {
                        msg = $"Counter: {evnt.name} [{key}]: { evnt.value.ToString() }";
                    }
                    else if (item is SummaryItem summary)
                    {
                        msg = $"Summary: {summary.name} [{key}]: {summary.count}, {summary.sum}, {summary.min}, {summary.max}";
                    }

                    Console.WriteLine(msg);
                }

                return collected;
            });
            return this;
        }

        public SampleMetricProvider Build()
        {
            return new SampleMetricProvider(this);
        }
    }

    public class CountSumMinMax
    {
        public string name;
        public int count = 0;
        public double sum = 0.0;
        public double min = 0.0;
        public double max = 0.0;

        public CountSumMinMax(string name)
        {
            this.name = name;
        }

        public void Update(IValueItem value)
        {
            double val = 0.0;
            if (value is IntValue ival)
            {
                val = ival.value;
            }
            else if (value is DoubleValue dval)
            {
                val = dval.value;
            }

            count++;
            sum += val;
            min = Math.Min(min, val);
            max = Math.Max(max, val);
        }
    }
}