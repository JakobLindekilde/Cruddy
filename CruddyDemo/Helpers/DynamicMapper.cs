namespace CruddyDemo.Helpers
{
    // TODO: Consider findig a NuGet package that can handle the mapping. 
    
    // TODO: From CoPilot: Consider using System.Text.Json for mapping dynamic objects to strongly typed objects,
    // as it can be more efficient and flexible. However, the current implementation is straightforward and works well for most scenarios.

    /// <summary>
    /// This class is intended to provide helper methods for mapping dynamic types 
    /// into strongly typed objects. It is particularly useful when working with data 
    /// retrieved from sources like Dapper, which often returns dynamic objects.
    /// </summary>
    static public class DynamicMapper
    {
        /// <summary>
        /// Maps a collection of dynamics to a list of strongly typed objects of type T.
        /// </summary>
        /// <typeparam name="T">The type to map the dynamic items to.</typeparam>
        /// <param name="dynItems">The collection of dynamics.</param>
        /// <returns>A list of strongly typed objects of type T.</returns>
        public static List<T> MapCollection<T>(IEnumerable<dynamic> dynItems)
        {
            var list = new List<T>();
            foreach (var dynItem in dynItems)
            {
                list.Add(Map<T>(dynItem));
            }
            return list;
        }

        /// <summary>
        /// Maps a dynamic object to a strongly typed object of type T. 
        /// </summary>
        /// <typeparam name="T">The type to map the dynamic object to.</typeparam>
        /// <param name="dynItem">The dynamic object to map.</param>
        /// <returns>A strongly typed object of type T.</returns>
        public static T Map<T>(dynamic dynItem)
        {
            var target = Activator.CreateInstance<T>()!;
            var targetType = typeof(T);

            var targetProps = targetType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(p => p.CanWrite);

            var sourceDict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            FillDictPropNames(sourceDict, dynItem);

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
        /// Build a dictionary of source property names -> values. 
        /// Support IDictionary (ExpandoObject/Dapper) and regular objects.
        /// </summary>
        public static void FillDictPropNames(Dictionary<string, object?> sourceDict, dynamic dyn)
        {
            if (dyn is IDictionary<string, object> dict)
            {
                foreach (var pair in dict)
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
