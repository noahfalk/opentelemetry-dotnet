using System;
using System.Collections.Generic;
using System.Threading;

namespace OpenTelmetry.Api
{
    public abstract class MetricBase
    {
        static protected List<MetricSource> registered = new();

        public MetricSource source { get; }

        public string MetricName { get; }

        public string MetricNamespace { get; }

        public string MetricType { get; }

        public virtual bool Enabled { get; set; }

        public MetricState state { get; set; }

        protected MetricBase(string ns, string name, string type)
        {
            MetricName = name;
            MetricNamespace = ns;
            MetricType = type;

            var sources = registered;
            if (sources.Count > 0)
            {
                source = sources[sources.Count-1];
                Enabled = source.OnCreate(this);
            }
        }

        protected void RecordMetricData(int num)
        {
            if (Enabled)
            {
                source?.OnRecord(this, num);
            }
        }

        protected void RecordMetricData(double num)
        {
            if (Enabled)
            {
                source?.OnRecord(this, num);
            }
        }

        static public void RegisterSDK(MetricSource source)
        {
            while (true)
            {
                var oldSources = registered;

                var newSources = new List<MetricSource>();
                newSources.AddRange(oldSources);
                newSources.Add(source);

                var orgSources = Interlocked.CompareExchange(ref MetricBase.registered, newSources, oldSources);
                if (orgSources == oldSources)
                {
                    break;
                }
            }
        }
    }

    public class MetricState
    {
    }
}