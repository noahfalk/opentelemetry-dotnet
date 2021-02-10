using System;
using System.Collections.Generic;
using OpenTelmetry.Api;
using OpenTelmetry.Sdk;

namespace Example
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

            var store_labelset = new LabelSet("Store", store_name);

            item_counter = source.CreateCounter("item_counter", store_labelset, 
                new LabelSet(
                    "Description", "Number of items sold",
                    "Unit", "Count",
                    "DefaultAggregator", "Sum"
                    ));

            cash_counter = source.CreateCounter("cash_counter", store_labelset,
                new LabelSet(
                    "Description", "Total available cash",
                    "Unit", "USD",
                    "DefaultAggregator", "Sum"
                    ));
        }

        public void process_order(string customer, params (string name, int qty)[] items)
        {
            double total_price = 0;

            foreach (var item in items)
            {
                total_price += item.qty * price_list[item.name];

                // Record Metric
                item_counter.Add(item.qty, new LabelSet("Item", item.name, "Customer", customer));
            }

            // Record Metric
            cash_counter.Add(total_price, new LabelSet("Customer", customer));
        }
    }

    public class Application
    {
        public static void Main(string[] args)
        {
            // Create Metric Pipeline
            var pipeline = new SampleSdk()
                .Name("OrderPipeline")
                .AttachSource("StoreMetrics")
                .AggregateByLabels(typeof(CountSumMinMax), 
                    new LabelSet("Customer", "*"),
                    new LabelSet("Item", "lemon,tomato"),
                    new LabelSet("Customer", "CustomerA,CustomerC", "Item", "*"),
                    new LabelSet("Store", "*", "Item", "*")
                    )
                .AggregateByLabels(typeof(LabelHistogram))
                .Build();


            var store = new GroceryStore("Portland");
            store.process_order("CustomerA", ("potato", 2), ("tomato", 3));
            store.process_order("CustomerB", ("tomato", 10));
            store.process_order("CustomerC", ("potato", 2));
            store.process_order("CustomerA", ("tomato", 1));


            // Shutdown Metric Pipeline
            pipeline.Stop();
        }

        /*
*** Collect...
Counter/StoreMetrics/item_counter/_Total
  CountSumMinMax: n=5, sum=18, min=1, max=10
Counter/StoreMetrics/item_counter/Store=Portland
  CountSumMinMax: n=5, sum=18, min=1, max=10
Counter/StoreMetrics/item_counter/Item=potato
  CountSumMinMax: n=2, sum=4, min=2, max=2
Counter/StoreMetrics/item_counter/Customer=customerA
  CountSumMinMax: n=3, sum=6, min=1, max=3
Counter/StoreMetrics/item_counter/Item=tomato
  CountSumMinMax: n=3, sum=14, min=1, max=10
Counter/StoreMetrics/cash_counter/_Total
  CountSumMinMax: n=4, sum=46.400000000000006, min=2.2, max=30
Counter/StoreMetrics/cash_counter/Store=Portland
  CountSumMinMax: n=4, sum=46.400000000000006, min=2.2, max=30
Counter/StoreMetrics/cash_counter/Customer=customerA
  CountSumMinMax: n=2, sum=14.2, min=3, max=11.2
Counter/StoreMetrics/item_counter/Customer=customerB
  CountSumMinMax: n=1, sum=10, min=10, max=10
Counter/StoreMetrics/cash_counter/Customer=customerB
  CountSumMinMax: n=1, sum=30, min=30, max=30
Counter/StoreMetrics/item_counter/Customer=customerC
  CountSumMinMax: n=1, sum=2, min=2, max=2
Counter/StoreMetrics/cash_counter/Customer=customerC
  CountSumMinMax: n=1, sum=2.2, min=2.2, max=2.2
        */
    }
}
