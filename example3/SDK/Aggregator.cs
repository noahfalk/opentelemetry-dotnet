using System;
using System.Collections.Generic;
using OpenTelmetry.Api;
using OpenTelmetry.Sdk;

namespace OpenTelmetry.Sdk
{
    public abstract class Aggregator
    {
        public abstract void Update(MeterBase meter, MetricValue num);
    }

    public class CountSumMinMax : Aggregator
    {
        public int count = 0;
        public double sum = 0;
        public double max = 0;
        public double min = 0;

        public override void Update(MeterBase meter, MetricValue value)
        {
            double num = 0;
            if (value.value is int i)
            {
                num = i;
            }
            if (value.value is double d)
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

        public override void Update(MeterBase meter, MetricValue value)
        {
            var labels = meter.Labels.GetKeyValues();

            var keys = new List<string>() { "_total" };

            for (var n = 0; n < labels.Length; n+= 2)
            {
                keys.Add($"{labels[n]}:{labels[n+1]}");
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
