using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace Cruddy.Components
{
    public class InputInfo
    {
        public required string DisplayName { get; init; }
        public required string? DisplayFormat { get; init; }
        public object? Value { get; set; }
        public string? FormattedValue { get; set; }
        public required Type Type { get; init; }
        public required bool Disabled { get; init; }
        public required bool Required { get; init; }
        public required bool HideKeyColumn { get; init; }
        public required bool IsKeyColumn { get; init; }
        public DataType? DataType { get; init; }

        private object? _min = 0;
        private object? _max;
        private bool _maxSet;

        public object? Min { get => _min; init => _min = value; }

        public object? Max
        {
            get => _maxSet ? _max : InferMax();
            init { _max = value; _maxSet = true; }
        }

        private object? InferMax()
        {
            return Type.GetTypeCode(Type) switch
            {
                TypeCode.Byte => byte.MaxValue,
                TypeCode.SByte => sbyte.MaxValue,
                TypeCode.Int16 => short.MaxValue,
                TypeCode.UInt16 => ushort.MaxValue,
                TypeCode.Int32 => int.MaxValue,
                TypeCode.UInt32 => uint.MaxValue,
                TypeCode.Int64 => long.MaxValue,
                TypeCode.UInt64 => ulong.MaxValue,
                TypeCode.Single => float.MaxValue,
                TypeCode.Double => double.MaxValue,
                TypeCode.Decimal => decimal.MaxValue,
                _ => null
            };
        }

        public bool Visible 
        { 
            get { 
                if (IsKeyColumn)
                {
                    return !HideKeyColumn;
                }
                return true;
            }
        }

        /// <summary>
        /// Returns the appropriate HTML input type="data" and input type="datetime-local" format (i.e. not in localized format).
        /// </summary>
        /// <returns></returns>
        public string InputDateTimeValue()
        {
            if (Value is DateTime dt)
            {
                if (DataType == System.ComponentModel.DataAnnotations.DataType.Date)
                {
                    return dt.ToString("yyyy-MM-dd");
                }
                else if (DataType == System.ComponentModel.DataAnnotations.DataType.Time)
                {
                    return dt.ToString("HH:mm:ss");
                }
                return dt.ToString("yyyy-MM-ddTHH:mm:ss");
            }
            return string.Empty;
        }
    }
}
