using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace Microsoft.OpenTelemetry.Api
{
    static public class MetricProvider
    {
        static IMetricProvider provider = new NoOpMetricProvider();

        static public IMetricProvider GetProvider()
        {
            return provider;
        }

        static public IMetricProvider SetProvider(IMetricProvider provider)
        {
            return Interlocked.Exchange(ref MetricProvider.provider, provider);
        }
    }
}
