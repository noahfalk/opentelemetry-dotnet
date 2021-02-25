using System;
using System.Collections.Generic;
using Microsoft.Diagnostics.Metric;
using OpenTelemetry.Metric.Api;
using OpenTelemetry.Metric.Sdk;

namespace GroceryStoreExample
{
    public class GroceryStore
    {
        private static Dictionary<string,double> price_list = new()
        {
            { "potato", 1.10 },
            { "tomato", 3.00 },
        };

        private string store_name;

        private Counter item_counter;
        private Counter cash_counter;

        public GroceryStore(string store_name)
        {
            this.store_name = store_name;

            // Setup Metrics

            var source = MetricSource.GetSource("StoreMetrics");

            var store_labelset = new MetricLabelSet(("Store", store_name));

            item_counter = source.CreateCounter("item_counter", store_labelset, 
                new MetricLabelSet(
                    ("Description", "Number of items sold"),
                    ("Unit", "Count"),
                    ("DefaultAggregator", "Sum")
                    ));

            cash_counter = source.CreateCounter("cash_counter", store_labelset,
                new MetricLabelSet(
                    ("Description", "Total available cash"),
                    ("Unit", "USD"),
                    ("DefaultAggregator", "Sum")
                    ));
        }

        public void process_order(string customer, params (string name, int qty)[] items)
        {
            double total_price = 0;

            foreach (var item in items)
            {
                total_price += item.qty * price_list[item.name];

                // Record Metric
                item_counter.Add(item.qty, new MetricLabelSet(("Item", item.name), ("Customer", customer)));
            }

            // Record Metric
            cash_counter.Add(total_price, new MetricLabelSet(("Customer", customer)));
        }
    }
}
