using Dapper;
using Microsoft.AspNetCore.Components;
using System.Data.Common;
using System.Text.Json;
using System.Text.Json.Serialization;
using CruddyDemo.Helpers;

namespace CruddyDemo.Components
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

#pragma warning disable S2077   // SonarQube rule S2077: "SQL queries should not be vulnerable to injection attacks".

        public object? Delete(DbConnection DbConnection, object keyValue)
        {
            var sql = $"DELETE FROM {DefaultSchema}.{TableName} WHERE {KeyColumn} = @keyValue";
            return DbConnection.ExecuteScalar(sql, param: new { keyValue });
        }

        public int Update(DbConnection DbConnection, TEntity entity, object keyValue)
        {
            // Build an UPDATE statement that sets all public writable properties except the key column.
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var props = entity.GetType()
                .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite && !string.Equals(p.Name, KeyColumn, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (props.Length == 0)
            {
                return 0;
            }

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
}
