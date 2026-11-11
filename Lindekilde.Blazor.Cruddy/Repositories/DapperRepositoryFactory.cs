using System.Data.Common;

namespace Lindekilde.Blazor.Cruddy.Repositories;

/// <summary>
/// Creates SQL Server table repositories.
/// </summary>
public sealed class DapperRepositoryFactory : IRepositoryFactory
{
    /// <inheritdoc />
    public IRepository Create(DbConnection dbConnection, string schema, string tableName, string? keyColumn) =>
        new DapperRepository(dbConnection, schema, tableName, keyColumn);
}
