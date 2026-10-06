using System.Reflection;

namespace Cruddy.Repositories;

/// <summary>
/// Repository abstraction for database operations on entities of type <typeparamref name="TEntity"/>.
/// </summary>
public interface IRepository<TEntity>
{
    /// <summary>
    /// Gets all records from the database using the provided SQL query and maps them to a list of <typeparamref name="TEntity"/>.
    /// </summary>
    /// <param name="sql">The SQL query to execute.</param>
    /// <returns>A list of <typeparamref name="TEntity"/> objects.</returns>
    List<TEntity> GetAll(string sql);

    /// <summary>
    /// Gets a record by its ID from the database and maps it to an instance of <typeparamref name="TEntity"/>.
    /// </summary>
    /// <param name="id">The ID of the record to retrieve.</param>
    /// <param name="columns">The columns to select.</param>
    /// <returns>An instance of <typeparamref name="TEntity"/> if found; otherwise, null.</returns>
    TEntity? GetById(object id, string columns = "*");

    /// <summary>
    /// Adds a new record to the database using the provided entity and properties, 
    /// and returns the identity value or number of rows affected.
    /// </summary>
    /// <param name="entity">The entity to add to the database.</param>
    /// <param name="properties">The properties of the entity to include in the insert statement.</param>
    /// <returns>The identity value of the newly inserted record or the number of rows affected.</returns>
    object? Add(TEntity entity, PropertyInfo[] properties);

    /// <summary>
    /// Updates an existing record in the database using the provided entity, key value, and properties.
    /// </summary>
    /// <param name="entity">The entity to update in the database.</param>
    /// <param name="keyValue">The value of the primary key for the record to update.</param>
    /// <param name="properties">The properties of the entity to include in the update statement.</param>
    /// <returns>The number of rows affected.</returns>
    int Update(TEntity entity, object keyValue, PropertyInfo[] properties);

    /// <summary>
    /// Deletes a record from the database using the provided key value.
    /// </summary>
    /// <param name="keyValue">The value of the primary key for the record to delete.</param>
    /// <returns>The identity value of the deleted record or the number of rows affected.</returns>
    object? Delete(object keyValue);
}
