using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Microsoft.Diagnostics.Metric
{
    public sealed class MetricSource
    {
        // singleton Default Source
        private static MetricSource default_source = new MetricSource("DefaultSource", "1.0.0");

        // Global list of all sources
        private static ConcurrentDictionary<(string name, string version), MetricSource> source_registry = new();

        // Name of this source
        public string Name { get; }

        public string Version { get; }

        // Allow users to associate their context
        public object UserContext { get; set; }

        // All listeners attached to this source
        // Note: Value portion of Dictionary is user supplied description
        private ConcurrentDictionary<MetricListener, string> listeners = new();

        private MetricSource(string name, string version)
        {
            this.Name = name;
            this.Version = version;
        }

        public static MetricSource DefaultSource
        {
            get => default_source;
        }

        public static ICollection<(string name, string version)> Sources
        {
            get => source_registry.Keys;
        }

        public static MetricSource GetSource(string name)
        {
            foreach (var reg in source_registry)
            {
                if (reg.Key.name == name)
                {
                    return reg.Value;
                }
            }

            return source_registry.GetOrAdd((name, "*"), (k) => new MetricSource(k.name, k.version));
        }

        public static MetricSource GetSource(string name, string version)
        {
            return source_registry.GetOrAdd((name, version), (k) => new MetricSource(k.name, k.version));
        }

        public ICollection<MetricListener> Listeners
        {
            get => listeners.Keys;
        }

        public bool AttachListener(MetricListener listener, string desc)
        {
            return listeners.TryAdd(listener, desc);
        }

        // param desc must match same as AttachListener call
        public bool DettachListener(MetricListener listener, string desc)
        {
            return listeners.TryRemove(KeyValuePair.Create(listener, desc));
        }

        internal bool ReportCreate(MetricBase meter)
        {
            foreach (var listener in listeners)
            {
                listener.Key.OnCreate(this, meter);
            }

            return true;
        }

        public bool ReportValue<T>(MetricBase meter, T value, MetricLabelSet labels)
        {
            foreach (var listener in listeners)
            {
                listener.Key.OnRecord(this, meter, value, labels);
            }

            return true;
        }

        public bool ReportValue<T>(IList<Tuple<MetricBase, T>> records, MetricLabelSet labels)
        {
            foreach (var listener in listeners)
            {
                listener.Key.OnRecord(this, records, labels);
            }

            return true;
        }
    }
}