using System;
using System.Collections.Generic;
using Microsoft.Diagnostics.Metric;

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
            var staticLabels = new Dictionary<string, string>
            {
                { "Store", store_name }
            };

            item_counter = new Counter("GroceryStoreExample", "1.0.0", "GroceryStore.item_counter", staticLabels,
                new string[] { "Item", "Customer" });
            cash_counter = new Counter("GroceryStoreExample", "1.0.0", "GroceryStore.cash_counter", staticLabels,
                new string[] { "Customer" });
        }

        public void process_order(string customer, params (string name, int qty)[] items)
        {
            double total_price = 0;

            foreach (var item in items)
            {
                total_price += item.qty * price_list[item.name];

                // Record Metric
                item_counter.Add(item.qty, item.name, customer);
            }

            // Record Metric
            cash_counter.Add(total_price, customer);
        }
    }
}
