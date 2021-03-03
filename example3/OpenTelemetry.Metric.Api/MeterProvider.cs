using System;
using System.Reflection;

namespace OpenTelemetry.Metric.Api
{
    public interface IMeterProvider
    {
        IMeter GetMeter(string name, string version);
        IMeter GetMeter<T>();
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

        public IMeter GetMeter<T>()
        {
            var clazzType = typeof(T);
            Assembly asm = clazzType.Assembly;

            string name = clazzType.FullName;
            var asmVersion = asm.GetName().Version?.ToString();
            var fileVersion = asm.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
            var productVersion = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            var version = productVersion ?? asmVersion ?? fileVersion ?? "";

            return new DotNetMeter(name, version);
        }
    }
}