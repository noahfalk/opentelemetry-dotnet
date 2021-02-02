using System;
using System.Threading;

namespace OpenTelmetry.Api
{
    public class Counter : MetricBase
    {
        public Counter(string ns, string name) : base(ns, name, "Counter")
        {
        }

        public void Add(int num)
        {
            RecordMetricData(num);
        }

        public void Add(double num)
        {
            RecordMetricData(num);
        }
    }

    public class Recorder : MetricBase
    {
        public Recorder(string ns, string name) : base(ns, name, "Recorder")
        {
        }

        public void Record(int num)
        {
            base.RecordMetricData(num);
        }
    }
}