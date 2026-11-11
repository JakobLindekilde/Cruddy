using Lindekilde.Blazor.Cruddy.Components;

namespace Lindekilde.Blazor.Cruddy.Repositories;

internal interface ITableRepository
{
    /// <summary>
    /// Gets all columns of the table, of a supported type, using sys.columns, sys.tables, sys.types and sys.indexes.
    /// </summary>
    /// <returns>The columns in table order. Empty if the table does not exist.</returns>
    List<CruddyField> GetColumns();

    /// <summary>
    /// Gets all rows returned by the SQL SELECT statement.
    /// </summary>
    /// <param name="sql">The SQL SELECT statement.</param>
    /// <returns>A list of rows (column name/value pairs).</returns>
    List<CruddyRow> GetAll(string sql);

    /// <summary>
    /// Gets a row by the value of the key column.
    /// </summary>
    /// <param name="id">The key value.</param>
    /// <param name="columns">The columns to select.</param>
    /// <returns>The row if found; otherwise, null.</returns>
    CruddyRow? GetById(object id, string columns = "*");

    /// <summary>
    /// Inserts a row, and returns the identity value or number of rows affected.
    /// </summary>
    /// <param name="row">The row to insert.</param>
    /// <param name="fields">The fields to include in the insert statement.</param>
    /// <returns>The identity value of the new row or the number of rows affected.</returns>
    object? Add(CruddyRow row, IReadOnlyList<CruddyField> fields);

    /// <summary>
    /// Updates a row identified by the value of the key column.
    /// </summary>
    /// <param name="row">The row holding the new values.</param>
    /// <param name="keyValue">The key value of the row to update.</param>
    /// <param name="fields">The fields to include in the update statement.</param>
    /// <returns>The number of rows affected.</returns>
    int Update(CruddyRow row, object keyValue, IReadOnlyList<CruddyField> fields);

    /// <summary>
    /// Deletes the row identified by the value of the key column.
    /// </summary>
    /// <param name="keyValue">The key value of the row to delete.</param>
    /// <returns>The scalar result of the delete statement.</returns>
    object? Delete(object keyValue);

    /// <summary>
    /// Gets the dropdown options for a foreign key from the primary table.
    /// </summary>
    /// <param name="info">The foreign key info.</param>
    /// <returns>A list of options ordered by display value.</returns>
    List<FKeyOption> GetFKeyOptions(FKeyInfo info);


}
