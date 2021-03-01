using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Metric
{
    public class ObservableCounter : Meter
    {
        // This is either a Func<double> or an Action<MeasurementObserver>
        object _observeValueFunc;

        public ObservableCounter(string name, Func<double> observeValue = null) :
            this(name, Array.Empty<string>(), observeValue)
        {
        }

        public ObservableCounter(string name, Action<MeasurementObserver> observeValues) :
            this(name, Array.Empty<string>(), observeValues)
        {
        }

        public ObservableCounter(string name, Dictionary<string, string> staticLabels, Func<double> observeValue = null) :
            this(name, staticLabels, Array.Empty<string>(), observeValue)
        {
        }

        public ObservableCounter(string name, Dictionary<string, string> staticLabels, Action<MeasurementObserver> observeValues) :
            this(name, staticLabels, Array.Empty<string>(), observeValues)
        {
        }

        public ObservableCounter(string name, string[] labelNames, Func<double> observeValue = null) :
            this(name, EmptyStaticLabels, labelNames, observeValue)
        {
        }

        public ObservableCounter(string name, string[] labelNames, Action<MeasurementObserver> observeValues) :
            this(name, EmptyStaticLabels, labelNames, observeValues)
        {
        }

        public ObservableCounter(string name, Dictionary<string, string> staticLabels, string[] labelNames, Func<double> observeValue = null) :
            base(name, staticLabels, labelNames)
        {
            _observeValueFunc = observeValue;
        }

        public ObservableCounter(string name, Dictionary<string, string> staticLabels, string[] labelNames, Action<MeasurementObserver> observeValues) :
            base(name, staticLabels, labelNames)
        {
            _observeValueFunc = observeValues;
        }

        public override AggregationConfiguration DefaultAggregation => AggregationConfigurations.Sum;

        protected internal override bool IsObservable => true;

        protected internal override void Observe(MeasurementObserver observer)
        {
            if (_observeValueFunc is Func<double>)
            {
                observer.Observe(((Func<double>)_observeValueFunc)());
            }
            else if((_observeValueFunc is Action<MeasurementObserver>))
            {
                ((Action<MeasurementObserver>)_observeValueFunc)(observer);
            }
        }

        public LabeledObservableCounter WithLabels(Func<double> observeValue, params string[] labelValues)
        {
            //TODO: we should probably memoize this
            return new LabeledObservableCounter(this, labelValues, observeValue);
        }
    }

    public class LabeledObservableCounter : LabeledMeter<ObservableCounter>
    {
        Func<double> _observeValueFunc;

        internal LabeledObservableCounter(ObservableCounter parent, string[] labelValues, Func<double> observeValue) : base(parent, labelValues)
        {
            _observeValueFunc = observeValue;
        }

        public override AggregationConfiguration DefaultAggregation => AggregationConfigurations.Sum;

        protected internal override bool IsObservable => true;

        protected internal override void Observe(MeasurementObserver observer)
        {
            observer.Observe(_observeValueFunc(), LabelValues);
        }
    }
}
