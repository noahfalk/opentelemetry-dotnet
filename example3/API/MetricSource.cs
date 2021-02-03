using System;

namespace OpenTelmetry.Api
{
    public abstract class MetricSource
    {
        protected MetricSource oldSource = null;

        public MetricSource()
        {
        }

        public void RegisterSDK()
        {
            MetricBase.RegisterSDK(this);
        }

        /// <summary>
        /// Let SDK know when new counters are being created
        /// </summary>
        public abstract bool OnCreate(MetricBase counter, LabelSet labels);


        /// <summary>
        /// Let SDK know when new measures are recorded
        /// </summary>
        public abstract bool OnRecord(MetricBase counter, DateTimeOffset dt, int num, LabelSet labels);

        // TODO: Represent int/double as a generic class so we don't need two OnRecord() function
        // TODO: Need discussion of carrying native number or BOX up the number into generic class
        public abstract bool OnRecord(MetricBase counter, DateTimeOffset dt, double num, LabelSet labels);
    }
}