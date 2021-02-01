using System.Collections.Generic;

namespace Microsoft.OpenTelemetry.Api
{
    public class DictionaryLabelSet : ILabelSet
    {
        IDictionary<string,string> labels;

        public DictionaryLabelSet()
        {
            this.labels = new Dictionary<string,string>();
        }

        public DictionaryLabelSet(IDictionary<string,string> labels)
        {
            this.labels = labels;
        }

        public IDictionary<string, string> GetLabels()
        {
            return labels;
        }
    }
}