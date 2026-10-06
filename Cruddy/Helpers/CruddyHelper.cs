using System.Reflection;

namespace Cruddy.Helpers;

/// <summary>
/// Misc helper methods for Cruddy.
/// </summary>
static public class CruddyHelper
{
    /// <summary>
    /// Creates a deep copy of the specified item using JSON serialization and deserialization.
    /// </summary>
    /// <param name="item">The item to deep copy.</param>
    /// <returns>A deep copy of the specified item.</returns>
    public static TEntity DeepCopy<TEntity>(TEntity item)
    {
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var json = System.Text.Json.JsonSerializer.Serialize(item, options);
        return System.Text.Json.JsonSerializer.Deserialize<TEntity>(json, options)!;
    }

    /// <summary>
    /// Creates a new instance of the specified type TEntity. If the default constructor is not available, 
    /// it attempts to create an instance by deserializing an empty JSON object.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to create.</typeparam>
    /// <returns>A new instance of the specified type TEntity.</returns>
    public static TEntity NewInstance<TEntity>()
    {
        try
        {
            return Activator.CreateInstance<TEntity>();
        }
        catch
        {
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return System.Text.Json.JsonSerializer.Deserialize<TEntity>("{}", options)!;
        }
    }

}
