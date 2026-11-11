using Dapper;
using Lindekilde.Blazor.Cruddy.Components;
using System.Data.Common;

namespace Lindekilde.Blazor.Cruddy.Repositories;

/// <summary>
/// Dapper based repository that does CRUD on the rows of a database table without needing a model.
/// A row is a dictionary of column name/value pairs, and the table columns are described by <see cref="CruddyField"/>.
/// </summary>
public class TableRepository(DbConnection dbConnection, string schema, string tableName, string? keyColumn)
{
    private const string ColumnsSql =
        """
        SELECT c.name AS Name,
               ty.name AS TypeName,
               c.is_nullable AS IsNullable,
               CASE WHEN ic.column_id IS NULL THEN 0 ELSE 1 END AS IsPrimaryKey,
               c.is_computed AS IsComputed
        FROM sys.columns c
        JOIN sys.tables t ON t.object_id = c.object_id
        JOIN sys.schemas s ON s.schema_id = t.schema_id
        JOIN sys.types ty ON ty.user_type_id = c.user_type_id
        LEFT JOIN sys.indexes i ON i.object_id = t.object_id AND i.is_primary_key = 1
        LEFT JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.column_id = c.column_id
        WHERE s.name = @Schema AND t.name = @Table
        ORDER BY c.column_id
        """;

    private sealed record ColumnRow(string Name, string TypeName, bool IsNullable, int IsPrimaryKey, bool IsComputed);

#pragma warning disable S2077   // SonarQube rule S2077: "SQL queries should not be vulnerable to injection attacks".

    /// <summary>
    /// Gets all columns of the table, of a supported type, using sys.columns, sys.tables, sys.types and sys.indexes.
    /// </summary>
    /// <returns>The columns in table order. Empty if the table does not exist.</returns>
    public List<CruddyField> GetColumns()
    {
        var rows = dbConnection.Query<ColumnRow>(ColumnsSql, new { Schema = schema, Table = tableName });

        var fields = new List<CruddyField>();
        foreach (var row in rows)
        {
            var type = MapSqlType(row.TypeName, out var isDate);
            if (type is null)
            {
                continue;
            }

            fields.Add(new CruddyField(row.Name, row.Name, type, row.IsNullable, row.IsPrimaryKey == 1, row.IsComputed, isDate));
        }

        return fields;
    }

    /// <summary>
    /// Maps a SQL Server type name to a supported CLR type.
    /// </summary>
    /// <param name="sqlType">The SQL Server type name e.g. "nvarchar".</param>
    /// <param name="isDate">True if the SQL type is "date".</param>
    /// <returns>The CLR type, or null if the SQL type is not supported.</returns>
    public static Type? MapSqlType(string sqlType, out bool isDate)
    {
        isDate = sqlType.Equals("date", StringComparison.OrdinalIgnoreCase);

        return sqlType.ToLowerInvariant() switch
        {
            "int" => typeof(int),
            "bigint" => typeof(long),
            "smallint" => typeof(short),
            "tinyint" => typeof(byte),
            "bit" => typeof(bool),
            "decimal" or "numeric" or "money" or "smallmoney" => typeof(decimal),
            "float" => typeof(double),
            "real" => typeof(float),
            "date" or "datetime" or "datetime2" or "smalldatetime" => typeof(DateTime),
            "time" => typeof(TimeSpan),
            "uniqueidentifier" => typeof(Guid),
            "char" or "varchar" or "nchar" or "nvarchar" or "text" or "ntext" or "sysname" => typeof(string),
            _ => null,
        };
    }

    /// <summary>
    /// Gets all rows returned by the SQL SELECT statement.
    /// </summary>
    /// <param name="sql">The SQL SELECT statement.</param>
    /// <returns>A list of rows (column name/value pairs).</returns>
    public List<CruddyRow> GetAll(string sql) => dbConnection.Query(sql).Select(ToRow).ToList();

    /// <summary>
    /// Gets a row by the value of the key column.
    /// </summary>
    /// <param name="id">The key value.</param>
    /// <param name="columns">The columns to select.</param>
    /// <returns>The row if found; otherwise, null.</returns>
    public CruddyRow? GetById(object id, string columns = "*")
    {
        var sql = $"SELECT {columns} FROM {schema}.{tableName} WHERE {keyColumn} = @Id";
        return dbConnection.Query(sql, new { Id = id }).Select(ToRow).FirstOrDefault();
    }

    /// <summary>
    /// Inserts a row, and returns the identity value or number of rows affected.
    /// </summary>
    /// <param name="row">The row to insert.</param>
    /// <param name="fields">The fields to include in the insert statement.</param>
    /// <returns>The identity value of the new row or the number of rows affected.</returns>
    public object? Add(CruddyRow row, IReadOnlyList<CruddyField> fields)
    {
        ArgumentNullException.ThrowIfNull(row);
        if (fields.Count == 0) return 0;

        var colNames = fields.Select(f => $"[{f.Column}]");
        var paramNames = fields.Select((_, i) => $"@p{i}");

        var sql = $"INSERT INTO {schema}.{tableName} ({string.Join(", ", colNames)}) VALUES ({string.Join(", ", paramNames)})";
        var dp = CreateParameters(row, fields);

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
    /// Updates a row identified by the value of the key column.
    /// </summary>
    /// <param name="row">The row holding the new values.</param>
    /// <param name="keyValue">The key value of the row to update.</param>
    /// <param name="fields">The fields to include in the update statement.</param>
    /// <returns>The number of rows affected.</returns>
    public int Update(CruddyRow row, object keyValue, IReadOnlyList<CruddyField> fields)
    {
        ArgumentNullException.ThrowIfNull(row);
        if (fields.Count == 0) return 0;

        var setClauses = fields.Select((f, i) => $"[{f.Column}] = @p{i}");
        var sql = $"UPDATE {schema}.{tableName} SET {string.Join(", ", setClauses)} WHERE {keyColumn} = @keyValue";

        var dp = CreateParameters(row, fields);
        dp.Add("keyValue", keyValue);

        return dbConnection.Execute(sql, dp);
    }

    /// <summary>
    /// Deletes the row identified by the value of the key column.
    /// </summary>
    /// <param name="keyValue">The key value of the row to delete.</param>
    /// <returns>The scalar result of the delete statement.</returns>
    public object? Delete(object keyValue)
    {
        var sql = $"DELETE FROM {schema}.{tableName} WHERE {keyColumn} = @keyValue";
        return dbConnection.ExecuteScalar(sql, param: new { keyValue });
    }

    /// <summary>
    /// Gets the dropdown options for a foreign key from the primary table.
    /// </summary>
    /// <param name="info">The foreign key info.</param>
    /// <returns>A list of options ordered by display value.</returns>
    public List<FKeyOption> GetFKeyOptions(FKeyInfo info)
    {
        var sql = $"SELECT {info.PrimaryId} AS Id, {info.PrimaryDisplay} AS Display FROM {info.PrimaryTable} ORDER BY {info.PrimaryDisplay}";
        return dbConnection.Query<FKeyOption>(sql).ToList();
    }

#pragma warning restore S2077

    private static DynamicParameters CreateParameters(CruddyRow row, IReadOnlyList<CruddyField> fields)
    {
        var dp = new DynamicParameters();
        for (var i = 0; i < fields.Count; i++)
        {
            row.TryGetValue(fields[i].Name, out var value);
            dp.Add($"p{i}", value);
        }
        return dp;
    }

    private static CruddyRow ToRow(dynamic dynRow)
    {
        var values = (IDictionary<string, object?>)dynRow;
        return new CruddyRow(values, StringComparer.OrdinalIgnoreCase);
    }
}
