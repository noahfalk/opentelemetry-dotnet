using System.Collections.Generic;

namespace Microsoft.OpenTelemetry.Sdk
{
    public abstract class Instrument
    {
        protected string name;

        public string GetName()
        {
            return name;
        }

        public abstract List<DataItem> Collect(PipeBuilder pipe);
    }
}