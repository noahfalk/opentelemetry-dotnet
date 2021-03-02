using System.Collections.Generic;
using Microsoft.Diagnostics.Metric;

namespace OpenTelemetry.Metric.Api
{
    public interface IMeter
    {
        Counter CreateCounter(string name);
        Counter CreateCounter(string name, Dictionary<string,string> labels);
        Counter CreateCounter(string name, params string[] dimNames);
        Counter CreateCounter(string name, Dictionary<string,string> labels, params string[] dimNames);

        Gauge CreateGauge(string name);
        Gauge CreateGauge(string name, Dictionary<string,string> labels);
        Gauge CreateGauge(string name, params string[] dimNames);
        Gauge CreateGauge(string name, Dictionary<string,string> labels, params string[] dimNames);
    }

    public class DotNetMeter : IMeter
    {
        private string libname;
        private string libver;

        public DotNetMeter(string libname, string libver)
        {
            this.libname = libname;
            this.libver = libver;
        }

        public Counter CreateCounter(string name)
        {
            return new Counter(libname, libver, name);
        }

        public Counter CreateCounter(string name, Dictionary<string,string> labels)
        {
            return new Counter(libname, libver, name, labels);
        }

        public Counter CreateCounter(string name, params string[] dimNames)
        {
            return new Counter(libname, libver, name, dimNames);
        }

        public Counter CreateCounter(string name, Dictionary<string,string> labels, params string[] dimNames)
        {
            return new Counter(libname, libver, name, labels, dimNames);
        }

        public Gauge CreateGauge(string name)
        {
            return new Gauge(libname, libver, name);
        }

        public Gauge CreateGauge(string name, Dictionary<string,string> labels)
        {
            return new Gauge(libname, libver, name, labels);
        }

        public Gauge CreateGauge(string name, params string[] dimNames)
        {
            return new Gauge(libname, libver, name, dimNames);
        }

        public Gauge CreateGauge(string name, Dictionary<string,string> labels, params string[] dimNames)
        {
            return new Gauge(libname, libver, name, labels, dimNames);
        }
    }
}