using System.Data.Common;

namespace Lindekilde.Blazor.Cruddy.Repositories;

/// <summary>
/// Creates SQL Server table repositories.
/// </summary>
public sealed class TableRepositorySqlServerFactory : ITableRepositoryFactory
{
    /// <inheritdoc />
    public ITableRepository Create(DbConnection dbConnection, string schema, string tableName, string? keyColumn) =>
        new TableRepositorySqlServer(dbConnection, schema, tableName, keyColumn);
}
