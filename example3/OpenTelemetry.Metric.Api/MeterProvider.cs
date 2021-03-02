namespace OpenTelemetry.Metric.Api
{
    public interface IMeterProvider
    {
        IMeter GetMeter(string name, string version);
    }

    public class MeterProvider : IMeterProvider
    {
        public static IMeterProvider Global { get; private set; } = new MeterProvider();

        public static IMeterProvider SetMeterProvider(IMeterProvider provider)
        {
            IMeterProvider oldProvider = Global;
            Global = provider;
            return oldProvider;
        }

        public MeterProvider()
        {
        }

        public IMeter GetMeter(string name, string version)
        {
            return new DotNetMeter(name, version);
        }
    }
}