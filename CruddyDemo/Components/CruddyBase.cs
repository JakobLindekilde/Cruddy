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
                    if (TableName.EndsWith("y", StringComparison.OrdinalIgnoreCase) && !IsVowel(TableName[TableName.Length - 2]))
                    {
                        TableName = TableName.Substring(0, TableName.Length - 1) + "ies";
                    }
                    else if (TableName.EndsWith("s", StringComparison.OrdinalIgnoreCase) || 
                             TableName.EndsWith("x", StringComparison.OrdinalIgnoreCase) || 
                             TableName.EndsWith("z", StringComparison.OrdinalIgnoreCase) || 
                             TableName.EndsWith("ch", StringComparison.OrdinalIgnoreCase) || 
                             TableName.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
                    {
                        TableName += "es";
                    }
                    else
                    {
                        TableName += "s";
                    }
                }
            }

            if (string.IsNullOrEmpty(KeyColumn))
            {
                KeyColumn = PropertyHelper.GetKey(typeof(TEntity));
                if (string.IsNullOrEmpty(KeyColumn) && PropertyHelper.PropExists(typeof(TEntity), "Id"))
                {
                    KeyColumn = "Id";
                } else if (string.IsNullOrEmpty(KeyColumn) && PropertyHelper.PropExists(typeof(TEntity), "ID"))
                {
                    KeyColumn = "ID";
                } else if (string.IsNullOrEmpty(KeyColumn) && PropertyHelper.PropExists(typeof(TEntity), $"{typeof(TEntity).Name}Id"))
                {
                    KeyColumn = $"{typeof(TEntity).Name}Id";
                } else if (string.IsNullOrEmpty(KeyColumn) && PropertyHelper.PropExists(typeof(TEntity), $"{typeof(TEntity).Name}ID"))
                {
                    KeyColumn = $"{typeof(TEntity).Name}ID";
                }

                if (string.IsNullOrEmpty(KeyColumn))
                {
                    KeyColumn = "Id";
                }
            }
        }

        private static bool IsVowel(char c)
        {
            return "aeiouAEIOU".IndexOf(c) >= 0;
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
        /// NOTE: If you use ASC/DESC in this parameter, then set <seealso cref="SortOrder"/> to <seealso cref="SortOrder.None"/>.
        /// </summary>
        [Parameter]
        public string? OrderBy { get; set; }

        /// <summary>
        /// The WHERE clause to filter the results by. Must be a valid 
        /// SQL WHERE clause (without the "WHERE" keyword).
        /// </summary>
        [Parameter]
        public string? Where { get; set; }

        /// <summary>
        /// Sorting order for the rows when using <seealso cref="OrderBy"/>. Default is <seealso cref="SortOrder.Ascending"/>.
        /// NOTE: If you use ASC/DESC in <seealso cref="OrderBy"/> parameter, then set this to <seealso cref="SortOrder.None"/>.
        /// </summary>
        [Parameter]
        public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

        /// <summary>
        /// Here the complete SQL SELECT statement, including joints, can be specified.
        /// If specified, parameters like <seealso cref="TableName"/> and <seealso cref="TableColumns"/> are ignored.
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
        protected readonly Dictionary<string, string> ColumnAliasDict = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Fills the <seealso cref="ColumnAliasDict"/> dictionary with the column names and their 
        /// corresponding property names. Use 'AS' to specify a property in the <typeparamref name="TEntity"/>. 
        /// </summary>
        protected void FillColumnAliasDict()
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
        protected virtual string BuildSql()
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

        public object? Delete(DbConnection DbConnection, object keyValue)
        {
            var sql = $"DELETE FROM {DefaultSchema}.{TableName} WHERE {KeyColumn} = @keyValue";
            return DbConnection.ExecuteScalar(sql, param: new { keyValue });
        }


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
            return Map<T>(dynRows).ToList();
        }

        /// <summary>
        /// Maps a collection of dynamics to a list of strongly typed objects of type T.
        /// </summary>
        /// <typeparam name="T">The type to map the dynamic items to.</typeparam>
        /// <param name="dynItems">The collection of dynamics.</param>
        /// <returns>A list of strongly typed objects of type T.</returns>
        public static List<T> Map<T>(IEnumerable<dynamic> dynItems)
        {
            if (dynItems == null) return new List<T>();

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
            return deserialized ?? new List<T>();
        }

    }
}
