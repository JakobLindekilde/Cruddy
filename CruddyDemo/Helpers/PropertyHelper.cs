using System.Reflection;


namespace CruddyDemo.Helpers
{
    static public class PropertyHelper
    {
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
