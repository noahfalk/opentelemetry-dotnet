using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Microsoft.Diagnostics.Metric
{
    public sealed class MetricSource
    {
        // singleton Default Source
        private static MetricSource default_source = new MetricSource("DefaultSource");

        // Global list of all sources
        private static ConcurrentDictionary<string,MetricSource> source_registry = new();

        // Name of this source
        public string Name { get; }

        // Allow users to associate their context
        public object UserContext { get; set; }

        // All listeners attached to this source
        // Note: Value portion of Dictionary is user supplied description
        private ConcurrentDictionary<MetricListener, string> listeners = new();

        private MetricSource(string name)
        {
            this.Name = name;
        }

        public static MetricSource DefaultSource
        {
            get => default_source;
        }

        public static ICollection<string> Sources
        {
            get => source_registry.Keys;
        }

        public static MetricSource GetSource(string name)
        {
            return source_registry.GetOrAdd(name, (k) => new MetricSource(k));
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

        internal bool ReportCreate(MetricBase meter, MetricLabel labels)
        {
            foreach (var listener in listeners)
            {
                listener.Key.OnCreate(this, meter, labels);
            }

            return true;
        }

        public bool ReportValue<T>(MetricBase meter, T value, MetricLabel labels)
        {
            foreach (var listener in listeners)
            {
                listener.Key.OnRecord(this, meter, value, labels);
            }

            return true;
        }

        public bool ReportValue<T>(IList<Tuple<MetricBase, T>> records, MetricLabel labels)
        {
            foreach (var listener in listeners)
            {
                listener.Key.OnRecord(this, records, labels);
            }

            return true;
        }
    }
}