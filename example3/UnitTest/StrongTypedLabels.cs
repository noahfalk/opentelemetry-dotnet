using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Metric;
using Xunit;

namespace UnitTest
{
    public class StrongTypedLabels
    {
        struct Labels
        {
            public string Dim1 { get; set; }
            public string Dim2 { get; set; }
        }

        [Fact]
        public void SubscribeToMetric()
        {
            
            Gauge<Labels> g = new Gauge<Labels>("StrongTypedLabels.A");
            using TestListener l = new TestListener(g);

            g.Set(19.3, new Labels() { Dim1 = "X", Dim2 = "Y" });

            Assert.Equal(19.3, l.Measurement);
            Assert.Equal(new string[] { "X", "Y" }, l.Labels);
        }
    }

    class TestListener : IDisposable
    {
        MeterListener _listener;
        MeterBase _meter;

        public TestListener(MeterBase meter)
        {
            _meter = meter;
            _listener = new MeterListener()
            {
                MeterPublished = (m, opt) => { if (m == _meter) opt.Subscribe(); },
                MeasurementRecorded = (meter, measurement, labels, cookie) =>
                {
                    Measurement = measurement;
                    Labels = labels;

                    Assert.Equal(_meter, meter);
                    Assert.Null(cookie);
                }
            };
            _listener.Start();
        }

        public string[] Labels { get; private set; }
        public double Measurement { get; private set; }

        public void Dispose()
        {
            _listener.Dispose();
        }
    }
}
