using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Metric;

namespace SimpleExamples
{
    class ObservableCounter_DynamicLabels_Example
    {
        ObservableCounter _hatsSoldCounter = new ObservableCounter(
            libname: "SimpleExamples",
            libver: "1.0.0",
            name: "HatCo.ColoredHatsSold",
            labelNames: new string[] { "Color" },
            observeValues: o =>
            {
                o.Observe(ColoredHatStoreData.GetTotalHatsSold("Yellow"), "Yellow");
                o.Observe(ColoredHatStoreData.GetTotalHatsSold("Red"), "Red");
            });
    }

    
    static class ColoredHatStoreData
    {
        static Dictionary<string, long> s_hatsSold = new Dictionary<string, long>();

        static ColoredHatStoreData()
        {
            s_hatsSold["Red"] = 0;
            s_hatsSold["Yellow"] = 0;
        }

        public static long GetTotalHatsSold(string color)
        {
            // Pretend this update was occuring asynchronously
            s_hatsSold[color] += new Random().Next(10_000);
            return s_hatsSold[color];
        }
    }
}
