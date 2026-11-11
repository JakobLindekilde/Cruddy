using System.Data.Common;

namespace Lindekilde.Blazor.Cruddy.Repositories;

/// <summary>
/// Creates a repository for a component's database table.
/// </summary>
public interface ITableRepositoryFactory
{
    /// <summary>
    /// Creates a repository using the specified connection and table settings.
    /// </summary>
    /// <param name="dbConnection">The component's database connection.</param>
    /// <param name="schema">The table schema.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="keyColumn">The table's key column, if known.</param>
    /// <returns>A repository for the specified table.</returns>
    ITableRepository Create(DbConnection dbConnection, string schema, string tableName, string? keyColumn);
}
