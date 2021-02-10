using System;

namespace OpenTelmetry.Api
{
    public class LabelSet
    {
        static public readonly string[] empty_keyvalues = {};
        static public LabelSet Empty { get; } = new LabelSet();
        
        private string[] keyvalues;

        public LabelSet()
        {
            keyvalues = empty_keyvalues;
        }

        /// <summary>
        /// i.e. new LabelSet(key1, value1, key2, value2, ...)
        /// </summary>
        public LabelSet(params string[] values)
        {
            keyvalues = values;
        }

        /// <summary>
        /// Returns KeyValue pairs in a flatten string array. 
        /// (i.e. [key1, value1, key2, value2, ... ])
        /// </summary>
        public virtual string[] GetKeyValues()
        {
            return keyvalues;
        }

        /// <summary>
        /// Return Array of Tuple&lt;Key,Value&gt;.
        /// </summary>
        public Tuple<string,string>[] GetLabels()
        {
            var ret = new Tuple<string,string>[keyvalues.Length/2];

            int pos = 0;
            for (int n = 0; n < keyvalues.Length; n += 2)
            {
                var key = keyvalues[n];
                var val = keyvalues[n+1];

                ret[pos++] = Tuple.Create(key,val);
            }

            return ret;
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