using System;

namespace OpenTelmetry.Api
{
    public abstract class CounterBase
    {
        static protected MetricSource source;

        protected string name;
        protected string ns;
        protected string type;

        public object sdkdata { get; set; } = null;

        protected CounterBase(string ns, string name, string type)
        {
            this.name = name;
            this.ns = ns;
            this.type = type;

            CounterBase.source.OnCreate(this);
        }

        public virtual string GetNameSpace()
        {
            return ns;
        }

        public virtual string GetName()
        {
            return name;
        }

        public virtual string GetCounterType()
        {
            return type;
        }

        static public void RegisterSDK(MetricSource source)
        {
            CounterBase.source = source;
        }
    }

    public class Counter : CounterBase
    {
        public Counter(string ns, string name) : base(ns, name, "Counter")
        {
        }

        public void Add(int num)
        {
            source.Record(this, num);
        }
    }

    public class Recorder : CounterBase
    {
        public Recorder(string ns, string name) : base(ns, name, "Recorder")
        {
        }

        public void Record(int num)
        {
            source.Record(this, num);
        }
    }
}