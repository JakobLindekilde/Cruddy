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

    /// <summary>
    /// Gets all records from the database using the provided SQL query and maps them to a list of <typeparamref name="TEntity"/>.
    /// </summary>
    /// <param name="sql">The SQL query to execute.</param>
    /// <returns>A list of <typeparamref name="TEntity"/> objects.</returns>
    public List<TEntity> GetAll(string sql)
    {
        IEnumerable<dynamic> dynRows = dbConnection.Query(sql);
        return Map<TEntity>(dynRows);
    }

    /// <summary>
    /// Gets a record by its ID from the database and maps it to an instance of <typeparamref name="TEntity"/>.
    /// </summary>
    /// <param name="id">The ID of the record to retrieve.</param>
    /// <param name="columns">The columns to select.</param>
    /// <returns>An instance of <typeparamref name="TEntity"/> if found; otherwise, null.</returns>
    public TEntity? GetById(object id, string columns = "*")
    {
        var sql = $"SELECT {columns} FROM {schema}.{tableName} WHERE {keyColumn} = @Id";

        IEnumerable<dynamic> dynRows = dbConnection.Query(sql, new { Id = id });
        return Map<TEntity>(dynRows).FirstOrDefault();
    }

    /// <summary>
    /// Adds a new record to the database using the provided entity and properties, 
    /// and returns the identity value or number of rows affected.
    /// </summary>
    /// <param name="entity">The entity to add to the database.</param>
    /// <param name="properties">The properties of the entity to include in the insert statement.</param>
    /// <returns>The identity value of the newly inserted record or the number of rows affected.</returns>
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

    /// <summary>
    /// Updates an existing record in the database using the provided entity, key value, and properties.
    /// </summary>
    /// <param name="entity">The entity to update in the database.</param>
    /// <param name="keyValue">The value of the primary key for the record to update.</param>
    /// <param name="properties">The properties of the entity to include in the update statement.</param>
    /// <returns>The number of rows affected.</returns>
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

    /// <summary>
    /// Deletes a record from the database using the provided key value.
    /// </summary>
    /// <param name="keyValue">The value of the primary key for the record to delete.</param>
    /// <returns>The identity value of the deleted record or the number of rows affected.</returns>
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
