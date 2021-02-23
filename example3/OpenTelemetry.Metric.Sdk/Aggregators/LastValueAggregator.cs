using System;
using System.Collections.Generic;
using Microsoft.Diagnostics.Metric;
using OpenTelemetry.Metric.Api;
using OpenTelemetry.Metric.Sdk;

namespace OpenTelemetry.Metric.Sdk
{
    public class LastValueAggregator : Aggregator
    {
        public override AggregatorState CreateState()
        {
            return new LastValueState();
        }
    }

    public class LastValueState : AggregatorState
    {
        private static readonly object lockValues = new();

        private GaugeValueType status = GaugeValueType.Empty;
        private long lvalue = 0;
        private double dvalue = 0.0;

        public override void Update<T>(MetricBase meter, T value, MetricLabelSet labels)
        {
            if (value is int ival)
            {
                lock (lockValues)
                {
                    this.lvalue = ival;
                    this.status = GaugeValueType.LongValue;
                }
            }
            else if (value is long lval)
            {
                lock (lockValues)
                {
                    this.lvalue = lval;
                    this.status = GaugeValueType.LongValue;
                }
            }
            else if (value is double dval)
            {
                lock (lockValues)
                {
                    this.dvalue = dval;
                    this.status = GaugeValueType.DoubleValue;
                }
            }
        }

        public override (string key, string value)[] Serialize()
        {
            GaugeValueType status;
            long lvalue;
            double dvalue;

            lock (lockValues)
            {
                status = this.status;
                lvalue = this.lvalue;
                dvalue = this.dvalue;
                this.status = GaugeValueType.Empty;
            }

            string val = "";

            switch (status)
            {
                case GaugeValueType.LongValue:
                    val = $"{lvalue}";
                    break;

                case GaugeValueType.DoubleValue:
                    val = $"{dvalue}";
                    break;

                default:
                    break;
            }

            return new (string key, string value)[]
            {
                ( "last", $"{val}" ),
            };
        }

        protected enum GaugeValueType
        {
            Empty,
            LongValue,
            DoubleValue
        }
    }
}
