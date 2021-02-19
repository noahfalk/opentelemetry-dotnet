using System;
using Microsoft.Diagnostics.Metric;

namespace OpenTelemetry.Metric.Api
{
    public class LabelSet : MetricLabel
    {
        private (string name, string value)[] labels = {};

        public static LabelSet Empty = new LabelSet();

        public LabelSet()
        {
        }

        public LabelSet(params (string name, string value)[] labels)
        {
            this.labels = labels;
        }

        /// <summary>
        /// Return Array of Tuple&lt;Key,Value&gt;.
        /// </summary>
        public override (string name, string value)[] GetLabels()
        {
            return labels;
        }
    }

    public class LabelSetSplit
    {
        private string[] keys;
        private string[] values;

        public LabelSetSplit(string[] keys, string[] values)
        {
            this.keys = keys;
            this.values = values;
        }

        public LabelSetSplit(params string[] val)
        {
            var len = val.Length / 2;

            keys = new string[len];
            values = new string[len];

            int pos = 0;
            for (int n = 0; n < len; n += 2)
            {
                keys[pos] = val[n];
                values[pos] = val[n+1];
                pos++;
            }
        }

        public virtual string[] GetKeys()
        {
            return keys;
        }

        public virtual string[] GetValues()
        {
            return values;
        }

        public virtual string[] GetKeyValues()
        {
            var len = keys.Length;

            var ret = new string[2*len];

            int pos = 0;
            for (int n = 0; n < len; n++)
            {
                ret[pos++] = keys[n];
                ret[pos++] = values[n];
            }

            return ret;
        }

        public Tuple<string,string>[] GetLabels()
        {
            var ret = new Tuple<string,string>[keys.Length];

            int pos = 0;
            for (int n = 0; n < keys.Length; n++)
            {
                var key = keys[n];
                var val = values[n];
                ret[pos++] = Tuple.Create(key,val);
            }

            return ret;
        }
    }
}