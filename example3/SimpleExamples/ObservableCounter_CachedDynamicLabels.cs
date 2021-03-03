using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Metric;

namespace SimpleExamples
{
    class ObservableCounter_CachedDynamicLabels_Example
    {
        ObservableCounter _hatsSoldCounter = new ObservableCounter("SimpleExamples", "1.0.0", "HatCo.ColoredHatsSold.Cached", new string[] { "Color" });

        LabeledObservableCounter _yellowHatsSoldCounter;
        LabeledObservableCounter _redHatsSoldCounter;

        public ObservableCounter_CachedDynamicLabels_Example()
        {
            _yellowHatsSoldCounter = _hatsSoldCounter.WithLabels(() => ColoredHatStoreData.GetTotalHatsSold("Yellow"), "Yellow");
            _redHatsSoldCounter = _hatsSoldCounter.WithLabels(() => ColoredHatStoreData.GetTotalHatsSold("Red"), "Red");
        }
    }
}
