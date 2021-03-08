using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Diagnostics.Metric
{

    public class MeterListener
    {
        // this dictionary is synchronized by the MetricCollection.s_lock
        Dictionary<MeterBase, object> _subscribedObservableMeters = new Dictionary<MeterBase, object>();

        public Action<MeterBase, MeterSubscribeOptions> MeterPublished;
        public Action<MeterBase, double, string[], object> MeasurementRecorded;
        public Action<MeterBase, object> MeterUnpublished;

        public void Start()
        {
            MeterCollection.Instance.AddListener(this);
        }

        public void RecordObservableMeters()
        {
            // This ensures that meters can't be published/unpublished while we are trying to traverse the
            // list. The Observe callback could still be concurrent with Dispose().
            lock (MeterCollection.Lock)
            {
                Dictionary<MeterBase, object> subscriptionCopy = new(_subscribedObservableMeters);
            }
            MeasurementObserver observer = new MeasurementObserver(this);
            foreach (KeyValuePair<MeterBase, object> kv in _subscribedObservableMeters)
            {
                observer.CurrentMeter = kv.Key;
                observer.CurrentCookie = kv.Value;
                observer.CurrentMeter.Observe(observer);
            }
        }

        public void Dispose()
        {
            MeterCollection.Instance.RemoveListener(this);
        }

        internal protected virtual void RecordMeasurement<LabelsType>(MeterBase<LabelsType> meter, double measurement, LabelsType labelValues, object listenerCookie)
        {
            this.MeasurementRecorded?.Invoke(meter, measurement, LabelTypeConverter<LabelsType>.GetLabelValues(labelValues), listenerCookie);
        }

        internal void SubscribeObservableMeter(MeterBase meter, object listenerCookie)
        {
            _subscribedObservableMeters[meter] = listenerCookie;
        }

        internal object UnsubscribeObservableMeter(MeterBase meter)
        {
            _subscribedObservableMeters.Remove(meter, out object cookie);
            return cookie;
        }
    }

    public class MeasurementObserver
    {
        internal MeasurementObserver(MeterListener listener)
        {
            Listener = listener;
        }
        internal MeterListener Listener { get; private set; }
        internal MeterBase CurrentMeter { get; set; }
        internal object CurrentCookie { get; set; }

        public void Observe(double value)
        {
            Listener.MeasurementRecorded(CurrentMeter, value, Array.Empty<string>(), CurrentCookie);
        }

        public void Observe(double value, params string[] labelValues)
        {
            Listener.MeasurementRecorded(CurrentMeter, value, labelValues, CurrentCookie);
        }
    }

}
