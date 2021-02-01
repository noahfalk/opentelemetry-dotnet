using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.OpenTelemetry.Api;

namespace Microsoft.OpenTelemetry.Sdk
{
    public class LabelSet : ILabelSet
    {
        private ConcurrentDictionary<string,string> labels = new();

        public LabelSet()
        {
        }

        public LabelSet(ILabelSet labels)
        {
            foreach (var kv in labels.GetLabels())
            {
                this.labels[kv.Key] = kv.Value;
            }
        }

        public LabelSet(string data)
        {
            var pairs = data.Split(";");
            foreach (var kv in pairs)
            {
                var p = kv.Split("=");
                if (p.Length == 2)
                {
                    labels[p[0]] = p[1];
                }
            }
        }

        public IDictionary<string,string> GetLabels()
        {
            return labels;
        }

        public void Update(LabelSet old)
        {
            foreach (var kv in old.labels)
            {
                labels[kv.Key] = kv.Value;
            }
        }

        public string Export()
        {
            return String.Join(";", labels.Select(k=>k.Key + "=" + k.Value));
        }

        public string ExportKeys()
        {
            return String.Join(";", labels.Select(k=>k.Key));
        }
    }
}
