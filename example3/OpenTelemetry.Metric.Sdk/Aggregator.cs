using System;
using System.Collections.Generic;
using OpenTelemetry.Metric.Api;
using OpenTelemetry.Metric.Sdk;

namespace OpenTelemetry.Metric.Sdk
{
    public abstract class Aggregator
    {
        public abstract void Update<T>(MeterBase meter, T num, LabelSet labels);
    }

    public class CountSumMinMax : Aggregator
    {
        public int count = 0;
        public double sum = 0;
        public double max = 0;
        public double min = 0;

        public override void Update<T>(MeterBase meter, T value, LabelSet labels)
        {
            double num = 0;

            if (value is int i)
            {
                num = i;
            }
            else if (value is double d)
            {
                num = d;
            }
            
            count++;
            sum += num;
            if (count == 1)
            {
                min = num;
                max = num;
            }
            else
            {
                min = Math.Min(min, num);
                max = Math.Max(max, num);
            }
        }
    }

    public class LabelHistogram : Aggregator
    {
        public Dictionary<string,int> bins = new();

        public override void Update<T>(MeterBase meter, T value, LabelSet labels)
        {
            var effectiveLabels = new Dictionary<string,string>();

            var boundLabels = meter.Labels.GetLabels();
            foreach (var label in boundLabels)
            {
                effectiveLabels[label.Item1] = label.Item2;
            }

            var adhocLabels = labels.GetLabels();
            foreach (var label in adhocLabels)
            {
                effectiveLabels[label.Item1] = label.Item2;
            }

            var keys = new List<string>() { "_total" };

            foreach (var l in effectiveLabels)
            {
                keys.Add($"{l.Key}:{l.Value}");
            }

            foreach (var key in keys)
            {
                int count;
                if (!bins.TryGetValue(key, out count))
                {
                    count = 0;
                }

                bins[key] = count + 1;
            }
        }
    }

    public class ExtraSDKState : MeterState
    {
        // TODO: SDK can store additional state data for each meter
    }
}