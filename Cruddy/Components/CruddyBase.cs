using Cruddy.Helpers;
using Dapper;
using Microsoft.AspNetCore.Components;
using System.Data.Common;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cruddy.Components
{
    /// <summary>
    /// Base class for simple CRUD operations in a blazor component.
    /// </summary>
    public partial class CruddyBase<TEntity> : ComponentBase 
    {
        public CruddyBase()
        {
            if (string.IsNullOrEmpty(TableName))
            {
                TableName = typeof(TEntity).Name;
                if (PluralizeTableName)
                {
                    TableName = Pluralize(TableName);
                }
            }

            if (string.IsNullOrEmpty(KeyColumn))
            {
                KeyColumn = PropertyHelper.CalcKey(typeof(TEntity));
            }
        }

        /// <summary>
        /// The default schema for the database table. Default is "dbo".
        /// </summary>
        [Parameter]
        public string DefaultSchema { get; set; } = "dbo";

        /// <summary>
        /// Name of database table to 'CRUD'. If not specified, the name of the class <typeparamref name="TEntity"/> will be used.
        /// </summary>
        [Parameter]
        public string TableName { get; set; }

        /// <summary>
        /// Whether to pluralize <seealso cref="TableName"/> (add 's') when doing SQL SELECT. Default is true.
        /// </summary>
        [Parameter]
        public bool PluralizeTableName { get; set; } = true;

        /// <summary>
        /// The columns to retrieve from table <seealso cref="TableName"/>, separated by commas.
        /// Default is "*" (all columns). Must be valid column names in the table.
        /// </summary>
        [Parameter]
        public string TableColumns { get; set; } = "*";

        /// <summary>
        /// The column that is the primary key of table <seealso cref="TableName"/>.
        /// If not specified, the first property of <typeparamref name="TEntity"/> marked with 
        /// the KeyAttribute will be used. If no property is marked with KeyAttribute, it will 
        /// look for a property named "Id" or "{ClassName}Id".
        /// </summary>
        [Parameter]
        public string? KeyColumn { get; set; }

        /// <summary>
        /// Whether to allow editing of the key column when creating rows (not when updating). Default is false.
        /// </summary>
        [Parameter]
        public bool AllowKeyColumnEditOnCreate { get; set; } = false;

        /// <summary>
        /// The maximum number of rows to retrieve from table <seealso cref="TableName"/>.
        /// </summary>
        /// <remarks>Default is 10000.</remarks>
        [Parameter]
        public int Top { get; set; } = 10000;

        /// <summary>
        /// Whether to retrieve only distinct (=unique) rows.
        /// </summary>
        /// <remarks>Default is false.</remarks>
        [Parameter]
        public bool Distinct { get; set; } = false;

        /// <summary>
        /// The column(s) to order the results by, separated by commas. 
        /// Must be a valid column name in table <seealso cref="TableName"/>.
        /// </summary>
        [Parameter]
        public string? OrderBy { get; set; }

        /// <summary>
        /// The WHERE clause to filter the results by (without the "WHERE" keyword).
        /// </summary>
        [Parameter]
        public string? Where { get; set; }

        /// <summary>
        /// Sorting order for the rows when using <seealso cref="OrderBy"/>. 
        /// </summary>
        /// <remarks>Default is <seealso cref="SortOrder.Ascending".</remarks>
        [Parameter]
        public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

        /// <summary>
        /// Here the complete SQL SELECT statement, including joints, can be specified.
        /// If specified, parameters like <seealso cref="Top"/> and <seealso cref="OrderBy"/> are ignored.
        /// </summary>
        [Parameter]
        public string? Select { get; set; }

        /// <summary>
        /// A database connection e.g an SqlConnection or MysqlConnection. 
        /// </summary>
        [Parameter]
        public required DbConnection DbConnection { get; set; }

        /// <summary>
        /// The rows retrieved from the database.
        /// </summary>
        protected List<TEntity>? Rows;

        /// <summary>
        /// During component initialization, this method fills <see cref="ColumnAliasDict"/>, 
        /// adds columns to the grid and retrieves the rows from the database.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            FillColumnAliasDict();
            Rows = GetTableRows<TEntity>(DbConnection, BuildSql());
        }

        /// <summary>
        /// A dictionary that maps the column names specified in <seealso cref="TableColumns"/> 
        /// to their corresponding property names in <typeparamref name="TEntity"/>.
        /// </summary>
        // TODO: Fix the SonarQube warning S3887: 
#pragma warning disable S3887   // SonarQube rule S3887: "Immutable fields should not be mutable". Use an immutable collection or reduce the accessibility of the non-private readonly field 'ColumnAliasDict'.
        public readonly Dictionary<string, string> ColumnAliasDict = new(StringComparer.OrdinalIgnoreCase);
#pragma warning restore S3887

        /// <summary>
        /// Fills the <seealso cref="ColumnAliasDict"/> dictionary with the column names and their 
        /// corresponding property names. Use 'AS' to specify a property in the <typeparamref name="TEntity"/>. 
        /// </summary>
        public void FillColumnAliasDict()
        {
            var columns = TableColumns.Split(',').Select(c => c.Trim());
            foreach (var column in columns)
            {
                var parts = column.Split([" AS "], StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 2 && parts[1].Contains('['))
                {
                    parts[1] = parts[1].Replace("[", "").Replace("]", "").Trim();
                }

                var key = parts[0].Trim();
                if (!ColumnAliasDict.ContainsKey(key))
                {
                    ColumnAliasDict.Add(key, parts.Length == 2 ? parts[1] : key);
                }
            }
        }

        /// <summary>
        /// This method builds the final SQL SELECT statement. It uses parameter 
        /// <seealso cref="TableName"/> or <seealso cref="Select"/> as base depending on which one is specified. 
        /// When <seealso cref="TableName"/> is specified, it will also use <seealso cref="TableColumns"/>, 
        /// <seealso cref="Top"/> and <seealso cref="Distinct"/> to build the SQL SELECT statement.
        /// If <seealso cref="OrderBy"/> is specified it will be appended (including <seealso cref="SortOrder"/>).
        /// </summary>
        /// <returns>The final SQL SELECT statement.</returns>  
        public virtual string BuildSql()
        {
            if (!string.IsNullOrEmpty(Select))
            {
                return Select;
            }

            string sql = 
                $"SELECT {(Distinct ? "DISTINCT " : "")}{(Top > 0 ? $"TOP {Top} " : "")}" +
                $" {TableColumns} FROM {DefaultSchema}.{TableName}";

            if (!string.IsNullOrEmpty(Where))
            {
                sql += $" WHERE {Where}";
            }

            if (!string.IsNullOrEmpty(OrderBy))
            {
                sql += $" ORDER BY {OrderBy}";


                if (!sql.EndsWith(" ASC", StringComparison.InvariantCultureIgnoreCase) && 
                    !sql.EndsWith(" DESC", StringComparison.InvariantCultureIgnoreCase) && SortOrder != SortOrder.None)
                {
                    sql += $" {(SortOrder == SortOrder.Ascending ? "ASC" : "DESC")}";
                }
            }

            return sql;
        }

        /// <summary>
        /// Gets the properties of <typeparamref name="TEntity"/> that correspond
        /// to the columns specified in <seealso cref="TableColumns"/>.
        /// Only public readable and writable properties of supported types are returned. 
        /// NOTE: When operation is <seealso cref="CrudOperation.Create"/>, 
        /// all properties are returned regardless of <seealso cref="TableColumns"/>.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="operation">The CRUD operation type.</param>
        /// <returns>An array of <see cref="PropertyInfo"/> objects that match the specified columns and operation.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public PropertyInfo[] GetPropertiesForTableColumns(TEntity entity, CrudOperation operation)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var cols = TableColumns == "*" || operation == CrudOperation.Create
                ? new List<string>()
                : TableColumns.Split(",", StringSplitOptions.TrimEntries).ToList();

            var props = PropertyHelper.GetColumnProperties(entity.GetType())
                .Where(p => PropertyHelper.IsSupported(p.PropertyType) &&
                            p.CanRead && p.CanWrite &&
                            (cols.Count == 0 || (cols.Count > 0 && cols.Contains(p.Name))))
                .ToArray();

            return props;
        }

        /// <summary>
        /// Determines whether a given property name should be included in the
        /// SQL operation based on the operation type. For example, the key column 
        /// is excluded from UPDATE operations unless AllowKeyColumnEditOnCreate is true.
        /// </summary>
        /// <param name="operation">The CRUD operation type.</param>
        /// <param name="propName">The name of the property.</param>
        /// <returns>True if the property should be included; otherwise, false.</returns>
        public bool IncludeColumn(CrudOperation operation, string propName)
        {
            if (string.Equals(propName, KeyColumn, StringComparison.OrdinalIgnoreCase))
            {
                if (operation == CrudOperation.Create)
                {
                    return AllowKeyColumnEditOnCreate;
                } 
                return false;
            }

            return true;
        }

#pragma warning disable S2077   // SonarQube rule S2077: "SQL queries should not be vulnerable to injection attacks".

        public object? Delete(DbConnection DbConnection, object keyValue)
        {
            var sql = $"DELETE FROM {DefaultSchema}.{TableName} WHERE {KeyColumn} = @keyValue";
            return DbConnection.ExecuteScalar(sql, param: new { keyValue });
        }

        public PropertyInfo[] GetPropertiesForCreateOrUpdate(TEntity entity, CrudOperation operation)
        {
            if (operation != CrudOperation.Create && operation != CrudOperation.Update)
            {
                throw new ArgumentException("Invalid operation. Must be either Create or Update.", nameof(operation));
            }

            var cols = TableColumns == "*" || operation == CrudOperation.Create
                ? new List<string>()
                : TableColumns.Split(",", StringSplitOptions.TrimEntries).ToList();

            var props = PropertyHelper.GetColumnProperties(entity.GetType())
                .Where(p => p.CanRead && p.CanWrite &&
                            (cols.Count == 0 || (cols.Count > 0 && cols.Contains(p.Name))) && IncludeColumn(operation, p.Name))
                .ToArray();

            return props;
        }

        public int Update(DbConnection DbConnection, TEntity entity, object keyValue)
        {
            // Build an UPDATE statement that sets all public writable properties except the key column.
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var props = GetPropertiesForCreateOrUpdate(entity, CrudOperation.Update);
            if (props.Length == 0) return 0;

            var setClauses = props.Select(p => $"{p.Name} = @{p.Name}");
            var sql = $"UPDATE {DefaultSchema}.{TableName} SET {string.Join(", ", setClauses)} WHERE {KeyColumn} = @keyValue";

            var dp = new Dapper.DynamicParameters();
            foreach (var prop in props)
            {
                var val = prop.GetValue(entity);
                dp.Add(prop.Name, val);
            }
            dp.Add("keyValue", keyValue);

            return DbConnection.Execute(sql, dp);
        }

        /// <summary>
        /// Inserts a new entity into the database table. By default the key column is excluded from the INSERT
        /// (useful when the key is an identity column). Returns the database scalar result if available
        /// (for example SCOPE_IDENTITY()), otherwise returns the number of rows affected.
        /// </summary>
        /// <param name="DbConnection">Database connection to use.</param>
        /// <param name="entity">The entity to insert.</param>
        /// <returns>Scalar result from the DB (e.g. new id) or rows affected.</returns>
        public object? Create(DbConnection DbConnection, TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var props = GetPropertiesForCreateOrUpdate(entity, CrudOperation.Create);
            if (props.Length == 0) return 0;

            var colNames = props.Select(p => p.Name).ToArray();
            var paramNames = props.Select(p => "@" + p.Name).ToArray();

            var sql = $"INSERT INTO {DefaultSchema}.{TableName} ({string.Join(", ", colNames)}) VALUES ({string.Join(", ", paramNames)})";

            var dp = new Dapper.DynamicParameters();
            foreach (var prop in props)
            {
                var val = prop.GetValue(entity);
                dp.Add(prop.Name, val);
            }

            // Try to return an identity value for SQL Server. If that fails, fall back to Execute (rows affected).
            try
            {
                var identitySql = sql + "; SELECT SCOPE_IDENTITY();";
                return DbConnection.ExecuteScalar(identitySql, dp);
            }
            catch
            {
                return DbConnection.Execute(sql, dp);
            }
        }
#pragma warning restore S2077

        /// <summary>
        /// Gets rows from the database table in <seealso cref="TableName"/> or <seealso cref="Select"/>
        /// and maps them to a list of <typeparamref name="T1"/>.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="DbConnection">A database connection e.g an SqlConnection (for MS SQL Server)/param>
        /// <param name="sql">The SQL query to execute.</param>
        static public List<T> GetTableRows<T>(DbConnection DbConnection, string sql)
        {
            IEnumerable<dynamic> dynRows = DbConnection.Query(sql);
            return [.. Map<T>(dynRows)];
        }

        /// <summary>
        /// Maps a collection of dynamics to a list of strongly typed objects of type T.
        /// </summary>
        /// <typeparam name="T">The type to map the dynamic items to.</typeparam>
        /// <param name="dynItems">The collection of dynamics.</param>
        /// <returns>A list of strongly typed objects of type T.</returns>
        public static List<T> Map<T>(IEnumerable<dynamic> dynItems)
        {
            if (dynItems == null) return [];

            // Use System.Text.Json to map dynamic objects to strongly-typed objects.
            // Serialize the dynamic collection and deserialize to List<T> with case-insensitive property matching.
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                // Preserve numbers and handle common enum/string conversions
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true) }
            };

            var json = JsonSerializer.Serialize(dynItems, options);
            var deserialized = JsonSerializer.Deserialize<List<T>>(json, options);
            return deserialized ?? [];
        }

        /// <summary>
        /// Pluralizes a given name according to basic English rules. If the name ends with 'y' and is preceded
        /// by a consonant, it replaces 'y' with 'ies'. If the name ends with 's', 'x', 'z', 'ch', or 'sh', 
        /// it adds 'es'. Otherwise, it simply adds 's'.
        /// Casing is preserved, so if the last character of the name is uppercase, the pluralized form 
        /// will also be in uppercase.
        /// </summary>
        /// <param name="name">The name to pluralize.</param>
        /// <returns>The pluralized form of the name.</returns>
        static public string Pluralize(string name)
        {
            string pluralized;
            if (name.EndsWith("y", StringComparison.OrdinalIgnoreCase) && !IsVowel(name[^2]))
            {
                pluralized = name[..^1] + "ies";
            }
            else if (name.EndsWith("s", StringComparison.OrdinalIgnoreCase) ||
                     name.EndsWith("x", StringComparison.OrdinalIgnoreCase) ||
                     name.EndsWith("z", StringComparison.OrdinalIgnoreCase) ||
                     name.EndsWith("ch", StringComparison.OrdinalIgnoreCase) ||
                     name.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
            {
                pluralized = name + "es";
            }
            else
            {
                pluralized = name + "s";
            }

            if (char.IsUpper(name[^1]))
            {
                pluralized = pluralized.ToUpper();
            }

            return pluralized;
        }

        /// <summary>
        /// Determines if a character is a vowel (a, e, i, o, u ) in either uppercase or lowercase.
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <returns>True if the character is a vowel; otherwise, false.</returns>
        public static bool IsVowel(char c)
        {
            return "aeiouAEIOU".Contains(c);
        }

    }

    public enum CrudOperation
    {
        Create = 1,
        Read,
        Update
    }

}
