using Dapper;
using System.Data.Common;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cruddy.Repositories;

/// <summary>
/// Dapper based implementation of <see cref="IRepository{TEntity}"/>.
/// </summary>
public class DapperRepository<TEntity>(DbConnection dbConnection, string schema, string tableName, string? keyColumn)
    : IRepository<TEntity>
{
#pragma warning disable S2077   // SonarQube rule S2077: "SQL queries should not be vulnerable to injection attacks".

    public List<TEntity> GetAll(string sql)
    {
        IEnumerable<dynamic> dynRows = dbConnection.Query(sql);
        return Map<TEntity>(dynRows);
    }

    public TEntity? GetById(object id, string columns = "*")
    {
        var sql = $"SELECT {columns} FROM {schema}.{tableName} WHERE {keyColumn} = @Id";

        IEnumerable<dynamic> dynRows = dbConnection.Query(sql, new { Id = id });
        return Map<TEntity>(dynRows).FirstOrDefault();
    }

    public object? Add(TEntity entity, PropertyInfo[] properties)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (properties.Length == 0) return 0;

        var colNames = properties.Select(p => p.Name);
        var paramNames = properties.Select(p => "@" + p.Name);

        var sql = $"INSERT INTO {schema}.{tableName} ({string.Join(", ", colNames)}) VALUES ({string.Join(", ", paramNames)})";
        var dp = CreateParameters(entity, properties);

        // Try to return an identity value for SQL Server. If that fails, fall back to Execute (rows affected).
        try
        {
            return dbConnection.ExecuteScalar(sql + "; SELECT SCOPE_IDENTITY();", dp);
        }
        catch
        {
            return dbConnection.Execute(sql, dp);
        }
    }

    public int Update(TEntity entity, object keyValue, PropertyInfo[] properties)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (properties.Length == 0) return 0;

        var setClauses = properties.Select(p => $"{p.Name} = @{p.Name}");
        var sql = $"UPDATE {schema}.{tableName} SET {string.Join(", ", setClauses)} WHERE {keyColumn} = @keyValue";

        var dp = CreateParameters(entity, properties);
        dp.Add("keyValue", keyValue);

        return dbConnection.Execute(sql, dp);
    }

    public object? Delete(object keyValue)
    {
        var sql = $"DELETE FROM {schema}.{tableName} WHERE {keyColumn} = @keyValue";
        return dbConnection.ExecuteScalar(sql, param: new { keyValue });
    }

#pragma warning restore S2077

    private static DynamicParameters CreateParameters(TEntity entity, PropertyInfo[] properties)
    {
        var dp = new DynamicParameters();
        foreach (var prop in properties)
        {
            dp.Add(prop.Name, prop.GetValue(entity));
        }
        return dp;
    }

    /// <summary>
    /// Maps a collection of dynamics to a list of strongly typed objects of type T.
    /// </summary>
    public static List<T> Map<T>(IEnumerable<dynamic> dynItems)
    {
        if (dynItems == null) return [];

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true) }
        };

        var json = JsonSerializer.Serialize(dynItems, options);
        var deserialized = JsonSerializer.Deserialize<List<T>>(json, options);
        return deserialized ?? [];
    }
}
