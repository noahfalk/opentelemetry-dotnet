namespace OpenTelmetry.Api
{
    public class MetricProvider
    {
        private static MetricProvider default_provider = new MetricProvider("Default");

        private string ns;

        public MetricProvider(string ns)
        {
            this.ns = ns;
        }

        public void AttachListener(MetricListener listener)
        {
            // TODO: How to attach?
        }

        public static MetricProvider Default
        {
            get => default_provider;
        }

        public string GetNamespace()
        {
            return ns;
        }
    }
}