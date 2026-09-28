using System.Reflection;

namespace Cruddy.Helpers
{
    /// <summary>
    /// Provides helper methods for Property operations.
    /// </summary>
    static public class PropertyHelper
    {
        /// <summary>
        /// Returns true if the type name is a number type like 
        /// Int32, Int64, Int16, UInt32, UInt64, UInt16, Byte, or SByte.
        /// </summary>
        /// <param name="typeName">The name of the type to check.</param>
        /// <returns>True if the type name is a number type; otherwise, false.</returns>
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
        /// <param name="typeName">The name of the type to check.</param>
        /// <returns>True if the type is a decimal type; otherwise, false.</returns>
        public static bool IsDecimal(Type type)
        {
            return
                type == typeof(decimal) ||
                type == typeof(double) ||
                type == typeof(float);
        }

        /// <summary>
        /// Returns true if the specified type is supported by Cruddy, which includes string, 
        /// number types, decimal types, bool, enum, DateTime, DateTimeOffset, TimeSpan, and Guid.
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
                t == typeof(DateTimeOffset) ||
                t == typeof(TimeSpan) ||
                t == typeof(TimeOnly) ||
                t == typeof(Guid))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the specified property is nullable. This handles both
        /// nullable value types (Nullable<T>) and C# 8+ nullable reference types
        /// by inspecting NullableAttribute/NullableContextAttribute on the property,
        /// declaring type or assembly.
        /// </summary>
        /// <param name="property">The PropertyInfo to check for nullability.</param>
        /// <returns>True if the property is nullable; otherwise, false.</returns>
        public static bool IsNullable(PropertyInfo property)
        {
            if (property == null) return false;

            // Value type Nullable<T>
            if (Nullable.GetUnderlyingType(property.PropertyType) != null)
            {
                return true;
            }

            // Reference types: inspect NullableAttribute on the property
            if (!property.PropertyType.IsValueType)
            {
                var nullableAttr = property.CustomAttributes
                    .FirstOrDefault(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");

                if (nullableAttr != null && nullableAttr.ConstructorArguments.Count == 1)
                {
                    var arg = nullableAttr.ConstructorArguments[0];
                    // Constructor may be a byte or a byte[]
                    if (arg.ArgumentType == typeof(byte[]))
                    {
                        var args = (IReadOnlyCollection<CustomAttributeTypedArgument>?)arg.Value;
                        if (args != null && args.Count > 0 && args.First().Value is byte b)
                        {
                            return b == 2;
                        }
                    }
                    else if (arg.Value is byte b)
                    {
                        return b == 2;
                    }
                }

                // If no NullableAttribute on the property, check NullableContext on declaring type
                var contextAttr = property.DeclaringType?.CustomAttributes
                    .FirstOrDefault(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute");

                if (contextAttr != null && contextAttr.ConstructorArguments.Count == 1 && contextAttr.ConstructorArguments[0].Value is byte cb)
                {
                    return cb == 2;
                }

                // Finally, check assembly-level NullableContextAttribute
                var asmContext = property.DeclaringType?.Assembly.CustomAttributes
                    .FirstOrDefault(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute");

                if (asmContext != null && asmContext.ConstructorArguments.Count == 1 && asmContext.ConstructorArguments[0].Value is byte cb2)
                {
                    return cb2 == 2;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the underlying type of a nullable type, or the type itself if it is not nullable.
        /// </summary>
        /// <param name="type">The type for which to get the underlying type.</param>
        /// <returns>The underlying type if the type is nullable; otherwise, the type itself.</returns>
        public static Type GetUnderlyingType(Type type) => Nullable.GetUnderlyingType(type) ?? type;

        /// <summary>
        /// Gets all public instance properties of the specified type that are considered "table column" properties.
        /// </summary>
        /// <param name="type">The type for which to get the column properties.</param>
        /// <returns>An array of PropertyInfo objects representing the column properties.</returns>
        public static PropertyInfo[] GetColumnProperties(Type type) =>
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !(p.PropertyType.IsClass && p.PropertyType != typeof(string)))
                .ToArray();

        /// <summary>
        /// Gets all public instance properties of the specified type that can be read (i.e., have a getter).
        /// </summary>
        /// <param name="type">The type for which to get the readable properties.</param>
        /// <returns>An array of PropertyInfo objects representing the readable properties.</returns>
        public static PropertyInfo[] GetReadProperties(Type type)
        {
            return [.. type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead)];
        }                   

        /// <summary>
        /// Gets the name of the first property marked with the KeyAttribute in the given object type.
        /// </summary>
        /// <param name="type">The type from which to get the property with KeyAttribute.</param>
        /// <returns>The name of the property marked with the KeyAttribute, or null if none is found.</returns>
        public static string? GetKey(Type type)
        {
            if (type == null) return null;

            if (type.GetProperties().Any(p => Attribute.IsDefined(p, typeof(System.ComponentModel.DataAnnotations.KeyAttribute))))
            {
                return type.GetProperties().First(p => Attribute.IsDefined(p, typeof(System.ComponentModel.DataAnnotations.KeyAttribute))).Name;
            }

            return null;
        }

        /// <summary>
        /// Calculates the key property name for the given type. It first checks for a property marked with the KeyAttribute. 
        /// If none is found, it looks for common key property names like "Id", "ID", "{TypeName}Id", or "{TypeName}ID". 
        /// If none of these are found, it defaults to "Id".
        /// </summary>
        /// <param name="type">The type for which to calculate the key property name.</param>
        /// <returns>The name of the key property.</returns>
        public static string CalcKey(Type type)
        {
            var key = GetKey(type);

            if (string.IsNullOrEmpty(key) && PropertyHelper.PropExists(type, "Id"))
            {
                key = "Id";
            }
            else if (string.IsNullOrEmpty(key) && PropertyHelper.PropExists(type, "ID"))
            {
                key = "ID";
            }
            else if (string.IsNullOrEmpty(key) && PropertyHelper.PropExists(type, $"{type.Name}Id"))
            {
                key = $"{type.Name}Id";
            }
            else if (string.IsNullOrEmpty(key) && PropertyHelper.PropExists(type, $"{type.Name}ID"))
            {
                key = $"{type.Name}ID";
            }

            if (string.IsNullOrEmpty(key))
            {
                key = "Id";
            }

            return key;
        }

        /// <summary>
        /// Checks if a property with the given name exists in the specified type.
        /// </summary>
        /// <param name="type">The type in which to check for the property.</param>
        /// <param name="propName">The name of the property to check for.</param>
        /// <returns>True if the property exists; otherwise, false.</returns>
        public static bool PropExists(Type type, string propName)
        {
            if (!string.IsNullOrEmpty(propName))
            {
                var prop = type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the value of a property from an object using reflection.   
        /// </summary>
        /// <param name="item">The object from which to get the property value.</param>
        /// <param name="propName">The name of the property.</param>
        /// <returns>The value of the property as a string.</returns>
        public static string GetValue(object item, string propName)
        {
            string value = string.Empty;
            if (!string.IsNullOrEmpty(propName))
            {
                var prop = item.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop != null)
                {
                    var val = prop.GetValue(item);
                    value = val?.ToString() ?? string.Empty;
                }
            }
            return value;
        }

        /// <summary>
        /// Gets the display name for a property, using the DisplayNameAttribute or 
        /// DisplayAttribute if present, otherwise returns the property name.
        /// </summary>
        /// <param name="prop">The property for which to get the display name.</param>
        /// <returns>The display name of the property.</returns>
        public static string GetDisplayName(PropertyInfo prop)
        {
            var displayNameAttr = prop.GetCustomAttribute<System.ComponentModel.DisplayNameAttribute>();
            if (displayNameAttr != null)
            {
                return displayNameAttr.DisplayName;
            }

            var displayAttr = prop.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
            if (displayAttr != null)
            {
                return displayAttr.Name ?? prop.Name;
            }

            return prop.Name;
        }

        /// <summary>
        /// Gets the display format for a property, using the DisplayFormatAttribute if present, otherwise returns null.
        /// </summary>
        /// <param name="prop">The property for which to get the display format.</param>
        /// <returns>The display format of the property.</returns>
        public static string? GetDisplayFormat(PropertyInfo prop)
        {
            var displayFormatAttr = prop.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayFormatAttribute>();
            if (displayFormatAttr != null)
            {
                return displayFormatAttr.DataFormatString;
            }

            return null;
        }

        static public string GetFormattedValue(PropertyInfo prop, object? item)
        {
            string formattedValue;
            var format = Cruddy.Helpers.PropertyHelper.GetDisplayFormat(prop);
            var value = prop.GetValue(item);

            if (value == null)
            {
                formattedValue = string.Empty;
            }
            else if (!string.IsNullOrEmpty(format) && value is System.IFormattable f)
            {
                formattedValue = f.ToString(format, System.Globalization.CultureInfo.CurrentCulture);
            }
            else
            {
                formattedValue = value?.ToString() ?? string.Empty;
            }

            return formattedValue;
        }


    }
}
