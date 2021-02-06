using System;
using System.Runtime.InteropServices;

namespace OpenTelmetry.Api
{
    /// <summary>
    /// MetricValue as a C# value type with Boxing allocation
    /// </summary>
    public struct MetricValue
    {
        public MetricValueType valueType;
        public object value;

        public MetricValue(int v)
        {
            this.valueType = MetricValueType.intType;
            this.value = (object) v;
        }

        public MetricValue(double v)
        {
            this.valueType = MetricValueType.doubleType;
            this.value = (object) v;
        }

        public int ToInt32()
        {
            if (this.value is int v)
            {
                return v;
            }

            return 0;
        }

        public double ToDouble()
        {
            if (this.value is double v)
            {
                return v;
            }
            
            return 0;
        }
    }

    /// <summary>
    /// MetricValue as a C# value type with minimum heap allocation
    /// </summary>
    public struct MetricValueSpan
    {
        public MetricValueType valueType;

        private byte b0;
        private byte b1;
        private byte b2;
        private byte b3;
        private byte b4;
        private byte b5;
        private byte b6;
        private byte b7;

        public MetricValueSpan(int v)
        {
            this.valueType = MetricValueType.intType;

            // TODO: Looking forward to an allocation free (Span<>) verison of GetBytes()
            var buffer = BitConverter.GetBytes(v);
            b0 = buffer[0];
            b1 = buffer[1];
            b2 = buffer[2];
            b3 = buffer[3];
            b4 = 0;
            b5 = 0;
            b6 = 0;
            b7 = 0;
        }

        public MetricValueSpan(double v)
        {
            this.valueType = MetricValueType.doubleType;
            
            var buffer = BitConverter.GetBytes(v);
            b0 = buffer[0];
            b1 = buffer[1];
            b2 = buffer[2];
            b3 = buffer[3];
            b4 = buffer[4];
            b5 = buffer[5];
            b6 = buffer[6];
            b7 = buffer[7];
        }

        public int ToInt32()
        {
            Span<byte> buffer = stackalloc byte[4];
            buffer[0] = b0;
            buffer[1] = b1;
            buffer[2] = b2;
            buffer[3] = b3;
            //buffer[4] = b4;
            //buffer[5] = b5;
            //buffer[6] = b6;
            //buffer[7] = b7;

            var v = BitConverter.ToInt32(buffer);

            return v;
        }

        public double ToDouble()
        {
            Span<byte> buffer = stackalloc byte[8];
            buffer[0] = b0;
            buffer[1] = b1;
            buffer[2] = b2;
            buffer[3] = b3;
            buffer[4] = b4;
            buffer[5] = b5;
            buffer[6] = b6;
            buffer[7] = b7;

            var v = BitConverter.ToDouble(buffer);
            return v;
        }
    }

    public enum MetricValueType
    {
        intType,
        doubleType,
    }
}