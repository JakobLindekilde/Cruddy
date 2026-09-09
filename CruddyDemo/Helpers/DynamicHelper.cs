using System.Reflection;

namespace CruddyDemo.Helpers
{
    /// <summary>
    /// This class is intended to provide helper methods for the dynamic type.
    /// </summary>
    static public class DynamicHelper
    {
        public static List<T> MapDynRows<T>(IEnumerable<dynamic> dynRows)
        {
            var list = new List<T>();
            foreach (var dynRow in dynRows)
            {
                list.Add(MapDyn<T>(dynRow));
            }
            return list;
        }

        public static T MapDyn<T>(dynamic dyn)
        {
            var target = Activator.CreateInstance<T>()!;
            var targetType = typeof(T);

            var targetProps = targetType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(p => p.CanWrite);

            var sourceDict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            FillDictPropNames(sourceDict, dyn);

            foreach (var prop in targetProps)
            {
                if (sourceDict.TryGetValue(prop.Name, out var value))
                {
                    if (value == DBNull.Value) value = null;

                    if (value != null)
                    {
                        var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        try
                        {
                            var converted = Convert.ChangeType(value, propType);
                            prop.SetValue(target, converted);
                        }
                        catch
                        {
                            // Fallback: if types are assignable, set directly
                            if (prop.PropertyType.IsAssignableFrom(value.GetType()))
                            {
                                prop.SetValue(target, value);
                            }
                        }
                    }
                    else
                    {
                        prop.SetValue(target, null);
                    }
                }
            }

            return target;
        }

        /// <summary>
        /// // Build a dictionary of source property names -> values. Support IDictionary (ExpandoObject/Dapper) and regular objects.
        /// </summary>
        public static void FillDictPropNames(Dictionary<string, object?> sourceDict, dynamic dyn)
        {
            if (dyn is IDictionary<string, object> kv)
            {
                foreach (var pair in kv)
                {
                    sourceDict[pair.Key] = pair.Value;
                }
            }
            else
            {
                var sourceObj = (object)dyn;
                var sourceType = sourceObj.GetType();
                var sourceProps = sourceType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                foreach (var p in sourceProps)
                {
                    sourceDict[p.Name] = p.GetValue(sourceObj);
                }
            }
        }


    }
}
