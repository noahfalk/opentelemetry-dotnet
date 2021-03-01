using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;

namespace Microsoft.Diagnostics.Metric
{
    public abstract class Meter : MeterBase
    {
        public static Dictionary<string, string> EmptyStaticLabels { get; } = new Dictionary<string, string>();

        public override string LibName { get; }
        public override string LibVersion { get; }
        public override string Name { get; }

        public override string[] LabelNames { get; }

        public override Dictionary<string, string> StaticLabels { get; }

        protected Meter(string libname, string libver, string name, string[] labelNames) 
            : this(libname, libver, name, EmptyStaticLabels, labelNames)
        {
        }

        protected Meter(string libname, string libver, string name, Dictionary<string,string> staticLabels, string[] labelNames)
        {
            LibName = libname;
            LibVersion = libver;
            Name = name;
            StaticLabels = staticLabels;
            LabelNames = labelNames;
            MeterCollection.Instance.AddMetric(this);
        }

        
    }
}
