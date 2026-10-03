namespace Cruddy.Helpers
{
    /// <summary>
    /// Provides helper methods for Type operations.
    /// </summary>
    static public class TypeHelper
    {
        /// <summary>
        /// Returns true if the specified type is supported by Cruddy, which includes string, 
        /// number types, decimal types, bool, enum, DateTime, TimeSpan, TimeOnly and Guid.
        /// </summary>
        /// <param name="type">The type to check for support.</param>
        /// <returns>True if the type is supported; otherwise, false.</returns>
        public static bool IsSupported(Type type)
        {
            var t = GetUnderlyingType(type);

            if (t == typeof(string) ||
                t.IsEnum ||
                IsNumber(t) ||
                IsDecimal(t) ||
                t == typeof(bool) ||
                t == typeof(DateTime) ||
                //t == typeof(DateTimeOffset) ||  //TODO: Support for DateTimeOffset and DateOnly missing
                //t == typeof(DateOnly) ||  
                t == typeof(TimeSpan) ||
                t == typeof(TimeOnly) ||
                t == typeof(Guid))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the type name is a number type like 
        /// Int32, Int64, Int16, UInt32, UInt64, UInt16, Byte, or SByte.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is a number type; otherwise, false.</returns>
        public static bool IsNumber(Type type)
        {
            return
                type == typeof(int) ||
                type == typeof(long) ||
                type == typeof(short) ||
                type == typeof(uint) ||
                type == typeof(ulong) ||
                type == typeof(ushort) ||
                type == typeof(byte) ||
                type == typeof(sbyte);
        }

        /// <summary>
        /// Returns true if the type is a decimal type (Decimal, Double, or Single).
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is a decimal type; otherwise, false.</returns>
        public static bool IsDecimal(Type type)
        {
            return
                type == typeof(decimal) ||
                type == typeof(double) ||
                type == typeof(float);
        }

        /// <summary>
        /// Gets the underlying type of a nullable type, or the type itself if it is not nullable.
        /// </summary>
        /// <param name="type">The type for which to get the underlying type.</param>
        /// <returns>The underlying type if the type is nullable; otherwise, the type itself.</returns>
        public static Type GetUnderlyingType(Type type) => Nullable.GetUnderlyingType(type) ?? type;


        /// <summary>
        /// Tries to parse the value to the target type. If parsing fails, returns null.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="targetType">The target type to parse the value to.</param>
        /// <returns>The parsed value, or null if parsing fails.</returns>
        public static object? TryParseValue(object? value, Type targetType)
        {
            object? converted = null;
            if (value == null)
            {
                return converted;
            }

            if (targetType == typeof(string)) converted = value.ToString();
            else if (targetType.IsEnum) converted = Enum.Parse(targetType, value.ToString()!);
            else if (targetType == typeof(int)) converted = int.TryParse(value.ToString(), out var i) ? i : (int?)null;
            else if (targetType == typeof(long)) converted = long.TryParse(value.ToString(), out var l) ? l : (long?)null;
            else if (targetType == typeof(short)) converted = short.TryParse(value.ToString(), out var s) ? s : (short?)null;
            else if (targetType == typeof(uint)) converted = uint.TryParse(value.ToString(), out var ui) ? ui : (uint?)null;
            else if (targetType == typeof(ulong)) converted = ulong.TryParse(value.ToString(), out var ul) ? ul : (ulong?)null;
            else if (targetType == typeof(ushort)) converted = ushort.TryParse(value.ToString(), out var us) ? us : (ushort?)null;
            else if (targetType == typeof(byte)) converted = byte.TryParse(value.ToString(), out var by) ? by : (byte?)null;
            else if (targetType == typeof(sbyte)) converted = sbyte.TryParse(value.ToString(), out var sby) ? sby : (sbyte?)null;
            else if (targetType == typeof(decimal)) converted = decimal.TryParse(value.ToString(), out var d) ? d : (decimal?)null;
            else if (targetType == typeof(double)) converted = double.TryParse(value.ToString(), out var dd) ? dd : (double?)null;
            else if (targetType == typeof(float)) converted = float.TryParse(value.ToString(), out var f) ? f : (float?)null;
            else if (targetType == typeof(bool)) converted = bool.TryParse(value.ToString(), out var b) ? b : (bool?)null;
            else if (targetType == typeof(DateTime)) converted = DateTime.TryParse(value.ToString(), out var dt) ? dt : (DateTime?)null;
            else if (targetType == typeof(DateTimeOffset)) converted = DateTimeOffset.TryParse(value.ToString(), out var dto) ? dto : (DateTimeOffset?)null;
            else if (targetType == typeof(DateOnly)) converted = DateOnly.TryParse(value.ToString(), out var dO) ? dO : (DateOnly?)null;
            else if (targetType == typeof(TimeSpan)) converted = TimeSpan.TryParse(value.ToString(), out var ts) ? ts : (TimeSpan?)null;
            else if (targetType == typeof(TimeOnly)) converted = TimeOnly.TryParse(value.ToString(), out var to) ? to : (TimeOnly?)null;
            else if (targetType == typeof(Guid)) converted = Guid.TryParse(value.ToString(), out var g) ? g : (Guid?)null;
            else converted = value;

            return converted;
        }

    }
}
