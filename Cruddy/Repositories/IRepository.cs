using System.Reflection;

namespace Cruddy.Repositories;

/// <summary>
/// Repository abstraction for database operations on entities of type <typeparamref name="TEntity"/>.
/// </summary>
public interface IRepository<TEntity>
{
    /// <summary>Gets all rows returned by the specified SQL SELECT statement.</summary>
    List<TEntity> GetAll(string sql);

    /// <summary>Gets a single entity by key, or null if not found.</summary>
    TEntity? GetById(object id, string columns = "*");

    /// <summary>Inserts an entity using the given properties. Returns the new identity or rows affected.</summary>
    object? Add(TEntity entity, PropertyInfo[] properties);

    /// <summary>Updates an entity using the given properties. Returns rows affected.</summary>
    int Update(TEntity entity, object keyValue, PropertyInfo[] properties);

    /// <summary>Deletes the entity with the specified key.</summary>
    object? Delete(object keyValue);
}
