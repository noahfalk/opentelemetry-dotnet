using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Metric;

namespace SimpleExamples
{
    class Gauge_StronglyTypedLabels_Example
    {
        struct HatsSoldLabels
        {
            public string Color { get; set; }
            public int Size { get; set; }
        }
        Gauge<HatsSoldLabels> _hatsSoldCounter = new("HatCo.HatsSold");

        public void Run()
        {
            _hatsSoldCounter.Set(19, new HatsSoldLabels() { Color="Red", Size=12 });
            _hatsSoldCounter.Set(7, new HatsSoldLabels() { Color = "Blue" });
        }
    }
}
