using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Metric
{
    static class LabelTypeConverter<LabelsType>
    {
        delegate object GetStructPropertyObj(ref LabelsType labels);

        static string[] _labelNames;
        static Func<LabelsType, string[]> _getValues;

        public static void Init()
        {
            if (_labelNames == null)
            {
                PropertyInfo[] dims = typeof(LabelsType).GetProperties();
                string[] labelNames = new string[dims.Length];
                for (int i = 0; i < dims.Length; i++)
                {
                    labelNames[i] = dims[i].Name;
                }
                Interlocked.CompareExchange(ref _labelNames, labelNames, null);
            }
            if(_getValues == null)
            {
                Func<LabelsType, string[]> getValues = CreateValuesGetter();
                Interlocked.CompareExchange(ref _getValues, getValues, null);
            }
        }


        static Func<LabelsType, string[]> CreateValuesGetter()
        {
            Func<LabelsType,string[]> func = CreateValuesGetterViaLCG();
            if (func == null)
            {
                func = CreateValuesGetterViaReflection();
            }
            return func;
        }

        static Func<LabelsType, string[]> CreateValuesGetterViaLCG()
        {
            PropertyInfo[] dims = typeof(LabelsType).GetProperties();
            if(dims.Length > byte.MaxValue)
            {
                return null;
            }
            DynamicMethod dm = new DynamicMethod("GetValues", typeof(string[]), new Type[] { typeof(LabelsType) },
                typeof(LabelsType).Assembly.ManifestModule);
            ILGenerator il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldc_I4_S, (byte)dims.Length);
            il.Emit(OpCodes.Newarr, typeof(string));
            for (int i = 0; i < dims.Length; i++)
            {
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4_S, (byte)i);
                il.Emit(OpCodes.Ldarg_0);
                il.EmitCall(OpCodes.Call, dims[i].GetGetMethod(), null);
                il.Emit(OpCodes.Stelem_Ref);
            }
            il.Emit(OpCodes.Ret);
            return dm.CreateDelegate<Func<LabelsType, string[]>>();
        }

        static Func<LabelsType,string[]> CreateValuesGetterViaReflection()
        {
            PropertyInfo[] dims = typeof(LabelsType).GetProperties();
            GetStructPropertyObj[] getters = new GetStructPropertyObj[dims.Length];
            for (int i = 0; i < dims.Length; i++)
            {
                getters[i] = dims[i].GetGetMethod().CreateDelegate<GetStructPropertyObj>();
            }

            return (LabelsType l) =>
            {
                string[] labelValues = new string[getters.Length];
                for (int i = 0; i < getters.Length; i++)
                {
                    labelValues[i] = getters[i](ref l).ToString();
                }
                return labelValues;
            };
        }

        public static string[] GetLabelNames()
        {
            return _labelNames;
        }

        public static string[] GetLabelValues(LabelsType labels)
        {
            return _getValues(labels);
        }
    }
}
