using System.Collections.Generic;
using System.Collections.Concurrent;

namespace OpenTelmetry.Api
{
    public class MetricSource
    {
        // singleton Default Source
        private static MetricSource default_source = new MetricSource("DefaultSource");

        // Global list of all sources
        private static ConcurrentDictionary<string,MetricSource> source_registry = new();

        // All listeners on this source
        private ConcurrentDictionary<MetricListener, bool> listeners = new();

        private string name;

        private MetricSource(string name)
        {
            this.name = name;
        }

        public string GetName()
        {
            return name;
        }
        
        public static MetricSource GetSource(string name)
        {
            return source_registry.GetOrAdd(name, (k) => new MetricSource(k));
        }

        public static MetricSource DefaultSource
        {
            get => default_source;
        }

        public bool AttachListener(MetricListener listener)
        {
            return listeners.TryAdd(listener, true);
        }

        public bool DettachListener(MetricListener listener)
        {
            return listeners.TryRemove(KeyValuePair.Create(listener, true));
        }

        public ICollection<MetricListener> GetListeners()
        {
            return listeners.Keys;
        }

    }
}