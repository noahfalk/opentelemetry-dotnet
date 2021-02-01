namespace Microsoft.OpenTelemetry.Sdk
{
    public interface IValueItem
    {
    }

    static public class ValueItemExtensions
    {
        public static IValueItem Assign<T>(T val)
        {
            if (val is int iVal)
            {
                return new IntValue(iVal);
            }
            else if (val is double dVal)
            {
                return new DoubleValue(dVal);
            }
            else
            {
                return new StringValue($"{val}");
            }
        }
    }

    public struct StringValue : IValueItem
    {
        public string value;

        public StringValue(string value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value;
        }
    }

    public struct IntValue : IValueItem
    {
        public int value;

        public IntValue(int value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return $"{value}";
        }
    }

    public struct DoubleValue : IValueItem
    {
        public double value;

        public DoubleValue(double value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return $"{value}";
        }
    }
}