using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf;
using OpenTelemetry.Metric.Sdk;
using Opentelemetry.Proto.Metrics.V1;
using Opentelemetry.Proto.Common.V1;
using Opentelemetry.Proto.Collector.Metrics.V1;

namespace Microsoft.OpenTelemetry.Export
{
    /* TODO
     * 
    public class ProtoBufClient
    {
        public ProtoBufClient()
        {
        }

        public (string name, string value)[] GetResource()
        {
            var resources = new (string name, string value)[] {
                ( "Host", "LocalHost"),
                ( "Location", "Portland" )
            };

            return resources;
        }

        public byte[] Send(ExportItem[] items)
        {
            var groups = items.GroupBy(
                k => (k.MeterName, k.MeterVersion),
                item => item,
                (k,items) => (k, items));

            var instMetrics = new List<InstrumentationLibraryMetrics>();
            foreach (var group in groups)
            {
                var instMetric = new InstrumentationLibraryMetrics();
                instMetric.InstrumentationLibrary = new InstrumentationLibrary();
                var lib = instMetric.InstrumentationLibrary;
                lib.Name = group.k.MeterName;
                lib.Version = group.k.MeterVersion;

                // Add all the ExportItems...
                foreach (var item in group.items)
                {
                    var metrics = BuildMetric(item);
                    if (metrics is not null)
                    {
                        instMetric.Metrics.AddRange(metrics);
                    }
                }

                instMetrics.Add(instMetric);
            }

            var resmetric = new ResourceMetrics();
            resmetric.InstrumentationLibraryMetrics.AddRange(instMetrics);
            resmetric.Resource = new Opentelemetry.Proto.Resource.V1.Resource();
            resmetric.Resource.DroppedAttributesCount = 0;
            var attribs = resmetric.Resource.Attributes;
            foreach (var resource in this.GetResource())
            {
                var kv = new KeyValue();
                kv.Key = resource.name;
                kv.Value = new AnyValue();
                kv.Value.StringValue = resource.value;
                attribs.Add(kv);
            }

            var request = new ExportMetricsServiceRequest();
            request.ResourceMetrics.Add(resmetric);

            return request.ToByteArray();
        }

        public Metric[] BuildMetric(ExportItem item)
        {
            var metrics = new List<Metric>();

            if (item.AggregationConfig == "SumCountMinMax")
            {
                foreach (var d in item.AggData)
                {
                    Metric metric = new Metric();
                    metric.Name = $"{item.InstrumentName} _{d.name} [{item.InstrumentType}]";
                    var sum = new DoubleSum();
                    metric.DoubleSum = sum;
                    sum.IsMonotonic = true;
                    var datapoints = sum.DataPoints;

                    var datapoint = new DoubleDataPoint();
                    datapoint.StartTimeUnixNano = (ulong) item.dt.ToUnixTimeMilliseconds() * 100000;
                    datapoint.TimeUnixNano = (ulong) item.dt.ToUnixTimeMilliseconds() * 100000;

                    foreach (var l in item.Labels.GetLabels())
                    {
                        var kv = new StringKeyValue();
                        kv.Key = l.name;
                        kv.Value = l.value;
                        datapoint.Labels.Add(kv);
                    }

                    datapoint.Value = double.Parse(d.value);
                    datapoints.Add(datapoint);

                    metrics.Add(metric);
                }
            }

            return metrics.ToArray();
        }

        public void Receive(byte[] bytes)
        {
            if (bytes.Length > 0)
            {
                Console.WriteLine($"Received {bytes.Length} bytes");

                var parser = new Google.Protobuf.MessageParser<ExportMetricsServiceRequest>(() => new ExportMetricsServiceRequest());
                var request = parser.ParseFrom(bytes);

                var records = new List<(
                    string resource,
                    string meterName,
                    string meterVersion,
                    string name,
                    string label,
                    ulong timestamp,
                    string value)>();

                foreach (var resMetric in request.ResourceMetrics)
                {
                    var attrs = resMetric.Resource.Attributes
                        .Select(k => $"{k.Key}={k.Value.StringValue}")
                        .ToList();
                    attrs.Sort();
                    var resource = String.Join("|", attrs);

                    foreach (var instMetric in resMetric.InstrumentationLibraryMetrics)
                    {
                        string meterName = instMetric.InstrumentationLibrary.Name;
                        string meterVersion = instMetric.InstrumentationLibrary.Version;

                        foreach (var metric in instMetric.Metrics)
                        {
                            if (metric.DoubleSum is not null)
                            {
                                foreach (var dp in metric.DoubleSum.DataPoints)
                                {
                                    var labels = dp.Labels.Select(k => $"{k.Key}={k.Value}").ToList();
                                    labels.Sort();

                                    records.Add((
                                        resource,
                                        meterName,
                                        meterVersion,
                                        metric.Name,
                                        String.Join("|", labels),
                                        dp.TimeUnixNano,
                                        $"{dp.Value}"
                                    ));
                                }
                            }
                        }
                    }
                }

                // Sort and Display

                var sortedList = new List<string>();
                foreach (var rec in records)
                {
                    var fields = new string[] {
                        //rec.resource,
                        rec.meterName,
                        rec.meterVersion,
                        rec.label,
                        rec.name,
                        rec.timestamp.ToString(),
                        rec.value
                    };

                    sortedList.Add(String.Join(" | ", fields));
                }
                sortedList.Sort();

                foreach (var rec in sortedList)
                {
                    Console.WriteLine(rec);
                }
            }
        }
    
    }
    */
}
