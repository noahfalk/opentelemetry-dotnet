using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace OpenTelemetry.Metric.Sdk
{
    public abstract class Exporter
    {
        public abstract void Export(ExportItem[] exports);

        public abstract void Start(CancellationToken token);

        public abstract void Stop();
    }

    public abstract class ExportItem
    {
    }
}