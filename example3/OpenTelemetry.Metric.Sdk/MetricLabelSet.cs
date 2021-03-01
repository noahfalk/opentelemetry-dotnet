using System;
using System.Linq;

namespace OpenTelemetry.Metric.Sdk
{
    public class MetricLabelSet : IEquatable<MetricLabelSet>
    {
        static private (string name, string value)[] emptyLabel = {};
        
        static private MetricLabelSet defaultLabel = new MetricLabelSet();

        private (string name, string value)[] labels = {};

        static public MetricLabelSet DefaultLabelSet
        {
            get => defaultLabel;
        }

        public MetricLabelSet()
        {
            labels = emptyLabel;
        }

        public MetricLabelSet(string[] labelNames, string[] labelValues)
        {
            this.labels = new (string, string)[labelNames.Length];
            for(int i = 0; i < labelNames.Length; i++)
            {
                if(i < labelValues.Length)
                {
                    this.labels[i] = (labelNames[i], labelValues[i]);
                }
                else
                {
                    this.labels[i] = (labelNames[i], "");
                }
            }
        }

        public MetricLabelSet(params (string name, string value)[] labels)
        {
            this.labels = labels;
        }

        /// <summary>
        /// Return Array of Tuple&lt;Key,Value&gt;.
        /// </summary>
        public virtual (string name, string value)[] GetLabels()
        {
            return this.labels;
        }

        public bool Equals(MetricLabelSet other)
        {
            if (this.labels.Length != other.labels.Length)
            {
                return false;
            }

            var len = this.labels.Length;
            for (var i = 0; i < len; i++)
            {
                if (this.labels[i].name != other.labels[i].name)
                {
                    return false;
                }

                if (this.labels[i].value != other.labels[i].value)
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(Object obj)
        {
            if (obj is MetricLabelSet other)
            {
                return this.Equals(other);
            }
            
            return false;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();

            foreach (var l in labels)
            {
                hash.Add(l.name);
                hash.Add(l.value);
            }
            
            return hash.ToHashCode();
        }

        public override string ToString()
        {
            var items = labels.Select(k => $"{k.name}={k.value}");
            return String.Join(";", items);
        }
    }
}
