namespace OpenTelmetry.Api
{
    public struct MetricValue
    {
        public MetricValueType valueType;
        public object value;

        public MetricValue(int v)
        {
            this.valueType = MetricValueType.intType;

            // TODO: Does this BOX the number causing an allocation on heap?
            this.value = (object) v;
        }

        public MetricValue(double v)
        {
            this.valueType = MetricValueType.doubleType;
            this.value = (object) v;
        }

        public enum MetricValueType
        {
            intType,
            doubleType,
        }
    }
}