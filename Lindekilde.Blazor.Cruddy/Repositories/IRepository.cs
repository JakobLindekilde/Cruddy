using Lindekilde.Blazor.Cruddy.Components;

namespace Lindekilde.Blazor.Cruddy.Repositories;

/// <summary>
/// Provides CRUD operations for a database table.
/// </summary>
public interface IRepository
{
    /// <summary>
    /// Gets all columns of the table, of a supported type, using sys.columns, sys.tables, sys.types and sys.indexes.
    /// </summary>
    /// <param name="columnAliasDict">Column aliases keyed by column name.</param>
    /// <param name="cancellationToken">A token to cancel the database operation.</param>
    /// <returns>The columns in table order. Empty if the table does not exist.</returns>
    Task<List<CruddyField>> GetColumnsAsync(Dictionary<string, string> columnAliasDict, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all rows returned by the SQL SELECT statement.
    /// </summary>
    /// <param name="sql">The SQL SELECT statement.</param>
    /// <param name="cancellationToken">A token to cancel the database operation.</param>
    /// <returns>A list of rows (column name/value pairs).</returns>
    Task<List<CruddyRow>> GetAllAsync(string sql, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a row by the value of the key column.
    /// </summary>
    /// <param name="id">The key value.</param>
    /// <param name="columns">The columns to select.</param>
    /// <param name="cancellationToken">A token to cancel the database operation.</param>
    /// <returns>The row if found; otherwise, null.</returns>
    Task<CruddyRow?> GetByIdAsync(object id, string columns = "*", CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a row, and returns the identity value or number of rows affected.
    /// </summary>
    /// <param name="row">The row to insert.</param>
    /// <param name="fields">The fields to include in the insert statement.</param>
    /// <param name="cancellationToken">A token to cancel the database operation.</param>
    /// <returns>The identity value of the new row or the number of rows affected.</returns>
    Task<object?> AddAsync(CruddyRow row, IReadOnlyList<CruddyField> fields, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a row identified by the value of the key column.
    /// </summary>
    /// <param name="row">The row holding the new values.</param>
    /// <param name="keyValue">The key value of the row to update.</param>
    /// <param name="fields">The fields to include in the update statement.</param>
    /// <param name="cancellationToken">A token to cancel the database operation.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> UpdateAsync(CruddyRow row, object keyValue, IReadOnlyList<CruddyField> fields, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the row identified by the value of the key column.
    /// </summary>
    /// <param name="keyValue">The key value of the row to delete.</param>
    /// <param name="cancellationToken">A token to cancel the database operation.</param>
    /// <returns>The scalar result of the delete statement.</returns>
    Task<object?> DeleteAsync(object keyValue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the dropdown options for a foreign key from the primary table.
    /// </summary>
    /// <param name="info">The foreign key info.</param>
    /// <param name="cancellationToken">A token to cancel the database operation.</param>
    /// <returns>A list of options ordered by display value.</returns>
    Task<List<FKeyOption>> GetFKeyOptionsAsync(FKeyInfo info, CancellationToken cancellationToken = default);


}
