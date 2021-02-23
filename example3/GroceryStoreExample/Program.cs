﻿using System;
using OpenTelemetry.Metric.Api;
using OpenTelemetry.Metric.Sdk;

namespace GroceryStoreExample
{
    class Program
    {
        public static void Main(string[] args)
        {
            // Create Metric Pipeline
            var pipeline = new MetricProvider()
                .Name("OrderPipeline1")
                .AttachSource("StoreMetrics")
                .AggregateByLabels(new SumCountMinMax(), 
                    new LabelSet(("Customer", "*")),
                    new LabelSet(("Item", "lemon,tomato")),
                    new LabelSet(("Customer", "CustomerA,CustomerC"), ("Item", "*")),
                    new LabelSet(("Store", "*"), ("Item", "*"))
                    )
                .AggregateByLabels(new LabelHistogram())
                .AddExporter(new ConsoleExporter("export1", 6000))
                .Build();

            var pipeline2 = new MetricProvider()
                .Name("OrderPipeline2")
                .AttachSource("StoreMetrics")
                .SetCollectionPeriod(6000)
                .AggregateByLabels(new LabelHistogram())
                .AddExporter(new ConsoleExporter("export2", 1000))
                .AddExporter(new ConsoleExporter("export3", 6000))
                .Build();


            var store = new GroceryStore("Portland");
            store.process_order("CustomerA", ("potato", 2), ("tomato", 3));
            store.process_order("CustomerB", ("tomato", 10));
            store.process_order("CustomerC", ("potato", 2));
            store.process_order("CustomerA", ("tomato", 1));


            // Shutdown Metric Pipeline
            pipeline.Stop();
            pipeline2.Stop();
        }
    }
}


/*

*** Collect...
Counter/StoreMetrics/cash_counter/CountSumMinMax/_Total
    CountSumMinMax: n=4, sum=46.400000000000006, min=2.2, max=30

Counter/StoreMetrics/cash_counter/CountSumMinMax/Customer=CustomerA
    CountSumMinMax: n=4, sum=28.4, min=3, max=11.2

Counter/StoreMetrics/cash_counter/CountSumMinMax/Customer=CustomerB
    CountSumMinMax: n=1, sum=30, min=30, max=30

Counter/StoreMetrics/cash_counter/CountSumMinMax/Customer=CustomerC
    CountSumMinMax: n=2, sum=4.4, min=2.2, max=2.2

Counter/StoreMetrics/cash_counter/CountSumMinMax/Store=Portland
    CountSumMinMax: n=4, sum=46.400000000000006, min=2.2, max=30

Counter/StoreMetrics/cash_counter/LabelHistogram/_Total
    LabelHistogram: _total=4, Store:Portland=4, Customer:CustomerA=2, Customer:CustomerB=1, Customer:CustomerC=1

Counter/StoreMetrics/item_counter/CountSumMinMax/_Total
    CountSumMinMax: n=5, sum=18, min=1, max=10

Counter/StoreMetrics/item_counter/CountSumMinMax/Customer=CustomerA
    CountSumMinMax: n=3, sum=6, min=1, max=3

Counter/StoreMetrics/item_counter/CountSumMinMax/Customer=CustomerA/Item=potato
    CountSumMinMax: n=1, sum=2, min=2, max=2

Counter/StoreMetrics/item_counter/CountSumMinMax/Customer=CustomerA/Item=tomato
    CountSumMinMax: n=2, sum=4, min=1, max=3

Counter/StoreMetrics/item_counter/CountSumMinMax/Customer=CustomerB
    CountSumMinMax: n=1, sum=10, min=10, max=10

Counter/StoreMetrics/item_counter/CountSumMinMax/Customer=CustomerC
    CountSumMinMax: n=1, sum=2, min=2, max=2

Counter/StoreMetrics/item_counter/CountSumMinMax/Customer=CustomerC/Item=potato
    CountSumMinMax: n=1, sum=2, min=2, max=2

Counter/StoreMetrics/item_counter/CountSumMinMax/Item=potato/Store=Portland
    CountSumMinMax: n=2, sum=4, min=2, max=2

Counter/StoreMetrics/item_counter/CountSumMinMax/Item=tomato
    CountSumMinMax: n=4, sum=24, min=1, max=10

Counter/StoreMetrics/item_counter/CountSumMinMax/Item=tomato/Store=Portland
    CountSumMinMax: n=3, sum=14, min=1, max=10

Counter/StoreMetrics/item_counter/LabelHistogram/_Total
    LabelHistogram: _total=5, Store:Portland=5, Item:potato=2, Customer:CustomerA=3, Item:tomato=3, Customer:CustomerB=1, Customer:CustomerC=1

*/

