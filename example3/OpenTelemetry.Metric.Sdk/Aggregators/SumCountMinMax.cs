using System;
using System.Collections.Generic;
using Microsoft.Diagnostics.Metric;
using OpenTelemetry.Metric.Api;
using OpenTelemetry.Metric.Sdk;

namespace OpenTelemetry.Metric.Sdk
{
    public class SumCountMinMax : Aggregator
    {
        public override AggregatorState CreateState()
        {
            return new SumCountMinMaxState();
        }
    }

    public class SumCountMinMaxState : AggregatorState
    {
        public int count = 0;
        public double sum = 0;
        public double max = 0;
        public double min = 0;

        public override void Update<T>(MeterBase meter, T value, MetricLabel labels)
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

        public override (string key, string value)[] Serialize()
        {
            return new (string key, string value)[]
            {
                ( "count", $"{count}" ),
                ( "sum", $"{sum}" ),
                ( "min", $"{min}" ),
                ( "max", $"{max}" )
            };
        }
    }
}
