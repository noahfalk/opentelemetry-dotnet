using System;
using System.Threading;

namespace OpenTelmetry.Api
{
    public class Counter : MeterBase
    {
        public Counter(string ns, string name) : base(ns, name, "Counter", LabelSet.Empty)
        {
        }

        public Counter(string ns, string name, LabelSet labels) : base(ns, name, "Counter", labels)
        {
        }

        public void Add(int num)
        {
            RecordMetricData(num, LabelSet.Empty);
        }

        public void Add(int num, LabelSet labels)
        {
            RecordMetricData(num, labels);
        }

        public void Add(double num)
        {
            RecordMetricData(num, LabelSet.Empty);
        }

        public void Add(double num, LabelSet labels)
        {
            RecordMetricData(num, labels);
        }
    }
}