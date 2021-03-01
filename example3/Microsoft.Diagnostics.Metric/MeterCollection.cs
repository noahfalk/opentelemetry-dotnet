using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Metric
{
    internal class MeterCollection
    {
        public static MeterCollection Instance = new MeterCollection();

        // Even if we had multiple exposed instances of this collection in the future
        // this lock also synchronizes access to per-metric subscription lists so it
        // needs to remain global (or metric subscription lists need to be changed)
        static internal object Lock = new object();

        List<MeterBase> _meters = new List<MeterBase>();
        List<MeterListener> _listeners = new List<MeterListener>();
        MeterSubscribeOptions _subscribeOptions = new MeterSubscribeOptions();

        public void AddMetric(MeterBase meter)
        {
            lock(Lock)
            {
                _meters.Add(meter);
                foreach(MeterListener listener in _listeners)
                {
                    NotifyListenerMetricAdd(listener, meter);
                }
            }
        }

        public void RemoveMetric(MeterBase meter)
        {
            lock (Lock)
            {
                _meters.Remove(meter);
                foreach (MeterListener listener in _listeners)
                {
                    NotifyListenerMetricRemove(listener, meter);
                }
            }
        }

        public void AddListener(MeterListener listener)
        {
            lock(Lock)
            {
                _listeners.Add(listener);
                foreach(MeterBase meter in _meters)
                {
                    NotifyListenerMetricAdd(listener, meter);
                }
            }
        }

        public void RemoveListener(MeterListener listener)
        {
            lock (Lock)
            {
                _listeners.Remove(listener);
                foreach (MeterBase meter in _meters)
                {
                    NotifyListenerMetricRemove(listener, meter);
                }
            }
        }

        void NotifyListenerMetricAdd(MeterListener listener, MeterBase meter)
        {
            _subscribeOptions.Reset();
            listener.MeterPublished?.Invoke(meter, _subscribeOptions);
            if (_subscribeOptions.IsSubscribed)
            {
                if (!meter.IsObservable)
                {
                    meter.AddSubscription(listener, _subscribeOptions.Cookie);
                }
                else
                {
                    listener.SubscribeObservableMeter(meter, _subscribeOptions.Cookie);
                }
            }
        }

        void NotifyListenerMetricRemove(MeterListener listener, MeterBase meter)
        {
            object cookie = null;
            if (!meter.IsObservable)
            {
                 cookie = meter.RemoveSubscription(listener);
            }
            else
            {
                cookie = listener.UnsubscribeObservableMeter(meter);
            }
            listener.MeterUnpublished?.Invoke(meter, cookie);
        }
    }

    public class MeterSubscribeOptions
    {
        internal bool IsSubscribed { get; private set; }
        internal object Cookie { get; private set; }
        internal void Reset()
        {
            IsSubscribed = false;
            Cookie = null;
        }
        public void Subscribe()
        {
            IsSubscribed = true;
        }
        public void Subscribe(object cookie)
        {
            Cookie = cookie;
            IsSubscribed = true;
        }
    }
}
