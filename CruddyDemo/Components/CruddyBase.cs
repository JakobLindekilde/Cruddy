using Dapper;
using Microsoft.AspNetCore.Components;
using System.Data.Common;

namespace CruddyDemo.Components
{
    /// <summary>
    /// Base class for simple CRUD operations in a blazor component.
    /// </summary>
    public partial class CruddyBase<T> : ComponentBase
    {
        /// <summary>
        /// Name of database table to 'CRUD'.
        /// </summary>
        [Parameter]
        public string? TableName { get; set; }

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
        [Parameter]
        public int? Top { get; set; }

        /// <summary>
        /// Whether to retrieve only distinct (=unique) rows.
        /// </summary>
        [Parameter]
        public bool? Distinct { get; set; }

        /// <summary>
        /// The column(s) to order the results by, separated by commas. 
        /// Must be a valid column name in table <seealso cref="TableName"/>.
        /// NOTE: If you use ASC/DESC in this parameter, then set <seealso cref="SortOrder"/> to <seealso cref="SortOrder.None"/>.
        /// </summary>
        [Parameter]
        public string? OrderBy { get; set; }

        /// <summary>
        /// Sorting order for the rows when using <seealso cref="OrderBy"/>. Default is <seealso cref="SortOrder.Ascending"/>.
        /// NOTE: If you use ASC/DESC in <seealso cref="OrderBy"/> parameter, then set this to <seealso cref="SortOrder.None"/>.
        /// </summary>
        [Parameter]
        public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

        /// <summary>
        /// Here the complete SQL SELECT statement, including joints etc., can be specified.
        /// Use this when not using the other parameters <seealso cref="TableName"/>, <seealso cref="TableColumns"/> and <seealso cref="Top"/>, <seealso cref="Distinct"/>.
        /// If <seealso cref="OrderBy"/> is specified, it will be appended to the SQL statement (including <seealso cref="SortOrder"/>).
        /// </summary>
        [Parameter]
        public string? Select { get; set; }

        /// <summary>
        /// A database connection e.g an SqlConnection or MysqlConnection. 
        /// </summary>
        [Parameter]
        public required DbConnection DbConnection { get; set; }

        /// <summary>
        /// Whether to show technical information.
        /// </summary>
        [Parameter]
        public bool? ShowTechInfo { get; set; }


        /// <summary>
        /// The rows retrieved from the database table in <seealso cref="TableName"/>.
        /// </summary>
        protected IQueryable<T>? Rows;

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
            string sql;

            if (!string.IsNullOrEmpty(Select))
            {
                sql = Select;
            }
            else
            {
                sql = $"SELECT {(Distinct.GetValueOrDefault() ? "DISTINCT " : "")}{(Top.HasValue ? $"TOP {Top.Value} " : "")}" +
                    $" {TableColumns} FROM dbo.{TableName}";
            }

            if (!sql.Contains("ORDER BY", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(OrderBy))
            {
                sql += $" ORDER BY {OrderBy}";

                if (SortOrder != SortOrder.None)
                {
                    sql += $" {(SortOrder == SortOrder.Ascending ? "ASC" : "DESC")}";
                }
            }

            return sql;
        }

        /// <summary>
        /// Calls first base.OnInitializedAsync() then FillRowsAsync().
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await FillRowsAsync();
        }

        /// <summary>
        /// This method uses <seealso cref="GetTableRowsAsync{T1}"/> to retrieve the rows 
        /// from the database and maps them to a list of <typeparamref name="T"/>.
        /// </summary>
        protected virtual async Task FillRowsAsync()
        {
            var sql = BuildSql();
            Rows = await GetTableRowsAsync<T>(DbConnection, sql);
        }

        /// <summary>
        /// Gets rows from the database table in <seealso cref="TableName"/> and maps them to a list of <typeparamref name="T1"/>.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="DbConnection">A database connection e.g an SqlConnection (for MS SQL Server)/param>
        /// <param name="tableName">The rows retrieved from this database table.</param>
        /// <param name="cols">The columns to retrieve.</param>
        static public async Task<IQueryable<T1>> GetTableRowsAsync<T1>(DbConnection DbConnection, string sql)
        {
            IEnumerable<dynamic> dynRows = await DbConnection.QueryAsync(sql);
            return Helpers.DynamicHelper.MapDynRows<T1>(dynRows).AsQueryable();
        }

    }

    /// <summary>
    /// Specifies the direction in which to sort the results of a SQL SELECT statement.
    /// </summary>
    public enum SortOrder
    {
        /// <summary>
        /// Do not sort the rows. Use this if ASC/DESC is used in <seealso cref="CruddyBase{T}.OrderBy"/> parameter.
        /// </summary>
        None = 0,
        /// <summary>
        /// Sort the rows in ascending order.
        /// </summary>
        Ascending = 1,
        /// <summary>
        /// Sort the rows in descending order.
        /// </summary>
        Descending = 2
    }
}
