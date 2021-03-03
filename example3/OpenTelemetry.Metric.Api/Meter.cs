using System.Collections.Generic;
using Microsoft.Diagnostics.Metric;

namespace OpenTelemetry.Metric.Api
{
    public interface IMeter
    {
        Counter CreateCounter(string name);
        Counter CreateCounter(string name, Dictionary<string,string> labels);
        Counter CreateCounter(string name, Dictionary<string,string> labels, params string[] dimNames);

        Counter1D CreateCounter(string name, string dn1);
        Counter1D CreateCounter(string name, Dictionary<string,string> labels, string dn1);

        Counter2D<T> CreateCounter<T>(string name, string dn1, string dn2);
        Counter2D<T> CreateCounter<T>(string name, Dictionary<string,string> labels, string dn1, string dn2);

        Counter3D CreateCounter(string name, string dn1, string dn2, string dn3);
        Counter3D CreateCounter(string name, Dictionary<string,string> labels, string dn1, string dn2, string dn3);

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

        public Counter CreateCounter(string name, Dictionary<string,string> labels, string[] dimNames)
        {
            return new Counter(libname, libver, name, labels, dimNames);
        }

        public Counter1D CreateCounter(string name, string d1)
        {
            return new Counter1D(libname, libver, name, d1);
        }

        public Counter1D CreateCounter(string name, Dictionary<string,string> labels, string d1)
        {
            return new Counter1D(libname, libver, name, labels, d1);
        }

        public Counter2D<T> CreateCounter<T>(string name, string d1, string d2)
        {
            return new Counter2D<T>(libname, libver, name, d1, d2);
        }

        public Counter2D<T> CreateCounter<T>(string name, Dictionary<string,string> labels, string d1, string d2)
        {
            return new Counter2D<T>(libname, libver, name, labels, d1, d2);
        }

        public Counter3D CreateCounter(string name, string d1, string d2, string d3)
        {
            return new Counter3D(libname, libver, name, d1, d2, d3);
        }

        public Counter3D CreateCounter(string name, Dictionary<string,string> labels, string d1, string d2, string d3)
        {
            return new Counter3D(libname, libver, name, labels, d1, d2, d3);
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

    public class Counter1D
    {
        Counter counter;

        public Counter1D(string libname, string libver, string name, string d1)
        {
            counter = new Counter(libname, libver, name, new string[] { d1 });
        }

        public Counter1D(string libname, string libver, string name, Dictionary<string,string> labels, string d1)
        {
            counter = new Counter(libname, libver, name, labels, new string[] { d1 });
        }

        public void Add(double d, string dv1)
        {
            counter.Add(d, new string[] { dv1 });
        }
    }

    public class Counter2D<T>
    {
        Counter counter;

        public Counter2D(string libname, string libver, string name, string d1, string d2)
        {
            counter = new Counter(libname, libver, name, new string[] { d1, d2 });
        }

        public Counter2D(string libname, string libver, string name, Dictionary<string,string> labels, string d1, string d2)
        {
            counter = new Counter(libname, libver, name, labels, new string[] { d1, d2 });
        }

        public void Add(T v, string dv1, string dv2)
        {
            double val = 0;
            if (v is int iv)
            {
                val = iv;
            }
            else if (v is double dv)
            {
                val = dv;
            }

            counter.Add(val, new string[] { dv1, dv2 });
        }
    }

    public class Counter3D
    {
        Counter counter;

        public Counter3D(string libname, string libver, string name, string d1, string d2, string d3)
        {
            counter = new Counter(libname, libver, name, new string[] { d1, d2, d3 });
        }

        public Counter3D(string libname, string libver, string name, Dictionary<string,string> labels, string d1, string d2, string d3)
        {
            counter = new Counter(libname, libver, name, labels, new string[] { d1, d2, d3 });
        }

        public void Add(double d, string dv1, string dv2, string dv3)
        {
            counter.Add(d, new string[] { dv1, dv2, dv3 });
        }
    }
}