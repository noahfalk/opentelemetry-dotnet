using System.Collections.Generic;
using System.Collections.Concurrent;

namespace OpenTelmetry.Api
{
    public class MetricProvider
    {
        // singleton Default Provider
        private static MetricProvider default_provider = new MetricProvider("DefaultProvider");

        // Global list of all providers
        private static ConcurrentDictionary<string,MetricProvider> provider_registry = new();

        // All listeners on this provider
        private ConcurrentDictionary<MetricListener, bool> listeners = new();

        private string name;

        private MetricProvider(string name)
        {
            this.name = name;
        }

        public string GetName()
        {
            return name;
        }
        
        public static MetricProvider GetProvider(string name)
        {
            return provider_registry.GetOrAdd(name, (k) => new MetricProvider(k));
        }

        public static MetricProvider DefaultProvider
        {
            get => default_provider;
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