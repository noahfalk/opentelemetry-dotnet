using System.Collections.Generic;

namespace Microsoft.OpenTelemetry.Api
{
    public interface IMetricProvider
    {
        IMeter GetMeter(string name, string version);
    }

    public interface IMeter
    {
        ICounter NewCounter(string name);

        IBatchBuilder RecordBatch(ILabelSet labelSet);
    }

    public interface IBatchBuilder
    {
        IBatchBuilder Add(ICounter counter, int value);
        IBatchBuilder Add(ICounter counter, double value);

        void Record();
    }

    public interface ICounter
    {
        void Add(int increment, ILabelSet labels);
        void Add(double increment, ILabelSet labels);

        IBoundCounter Bind(ILabelSet labels);
    }

    public interface IBoundCounter
    {
        void Add(int increment);
        void Add(double increment);

        void Unbind();
    }

    public interface ILabelSet
    {
        IDictionary<string,string> GetLabels();
    }
}
