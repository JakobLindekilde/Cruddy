using System.Reflection;

namespace CruddyDemo.Helpers
{
    /// <summary>
    /// Provides helper methods for Property operations.
    /// </summary>
    static public class PropertyHelper
    {
        /// <summary>
        /// Returns true if the type is a decimal type (decimal, double, or float).
        /// </summary>
        /// <param name="typeName">The name of the type to check.</param>
        /// <returns>True if the type is a decimal type; otherwise, false.</returns>
        public static bool IsDecimal(string typeName)
        {
            return
                typeName == "Decimal" ||
                typeName == "Double" ||
                typeName == "Single";
        }

        /// <summary>
        /// Returns true if the type name is a number type.
        /// </summary>
        /// <param name="typeName">The name of the type to check.</param>
        /// <returns>True if the type name is a number type; otherwise, false.</returns>
        public static bool IsNumber(string typeName)
        {
            return
                typeName == "Int32" ||
                typeName == "Int64" ||
                typeName == "Int16" ||
                typeName == "UInt32" ||
                typeName == "UInt64" ||
                typeName == "UInt16" ||
                typeName == "Byte" ||
                typeName == "SByte";
        }

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

    }
}
