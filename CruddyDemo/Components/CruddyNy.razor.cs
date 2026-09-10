using Dapper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using QuickGrid.Toolkit;
using QuickGrid.Toolkit.Columns;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;

namespace CruddyDemo.Components
{
    /// <summary>
    /// Base class for simple CRUD operations in a blazor component.
    /// </summary>
    public partial class CruddyNy<TEntity>
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
        /// <remarks>Default is 10000.</remarks>
        [Parameter]
        public int? Top { get; set; } = 10000;

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
        public bool? ShowInfoMessage { get; set; }

        protected string InfoMessage { get; set; } = string.Empty;

        protected QuickGrid<TEntity>? MyGrid;

        protected readonly ColumnManager<TEntity> ColumnManager = new();


        /// <summary>
        /// The rows retrieved from the database table in <seealso cref="TableName"/>.
        /// </summary>
        protected IQueryable<TEntity>? Rows;

        public bool IsInitialized { get; set; }


        readonly Dictionary<string, string> ColumnAliasDict = new(StringComparer.OrdinalIgnoreCase);

        protected void FillColumnAliasDict()
        {
            var columns = TableColumns.Split(',').Select(c => c.Trim());
            foreach (var column in columns)
            {
                var parts = column.Split([" AS "], StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 2 && parts[1].Contains('['))
                {
                    parts[1] = parts[1].Replace("[", "").Replace("]", "");
                }

                ColumnAliasDict.Add(parts[0].Trim(), parts.Length == 2 ? parts[1].Trim() : parts[0].Trim());
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
        /// Calls first base.OnInitializedAsync() then FillRowsAsync() and AddColumnsToGrid() last.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            // TODO: Why is OnInitializedAsync called twice? Should be called only once, but it is called twice. This is a bug in Blazor?
            // Should we NOT call base.OnInitializedAsync()?
            await base.OnInitializedAsync();

            if (!IsInitialized)
            {
                IsInitialized = true;
                FillColumnAliasDict();
                await FillRowsAsync();
                AddColumnsToGrid();
            }
        }

        /// <summary>
        /// This method uses <seealso cref="GetTableRowsAsync{T1}"/> to retrieve the rows 
        /// from the database and maps them to a list of <typeparamref name="T"/>.
        /// </summary>
        protected virtual async Task FillRowsAsync()
        {
            Rows = await GetTableRowsAsync<TEntity>(DbConnection, BuildSql());
        }

        /// <summary>
        /// Add a simple column, using AddSimple(), for each public readable property on TEntity.
        /// </summary>
        protected virtual void AddColumnsToGrid()
        {
            var entityType = typeof(TEntity);
            var props = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead);

            foreach (var prop in props)
            {
                if (TableColumns == "*" || ColumnAliasDict.ContainsValue(prop.Name))
                {
                    AddColumn(prop, entityType);
                }
            }
        }

        protected virtual void AddColumn(PropertyInfo prop, Type entityType)
        {
            var param = Expression.Parameter(entityType, "p");
            var access = Expression.PropertyOrField(param, prop.Name);

            Expression body = access;
            // If value type, box to object
            if (prop.PropertyType.IsValueType)
            {
                body = Expression.Convert(access, typeof(object));
            }

            var lambdaType = typeof(Func<,>).MakeGenericType(entityType, typeof(object));
            var lambda = Expression.Lambda(lambdaType, body, param);

            //if (prop.PropertyType.Name == "Decimal")
            //{ 
            //    AddNumberColumn(prop, lambda);
            //}
            //else
            //{ 
               AddSimpleColumn(prop, lambda);
            //}
        }

        private void AddSimpleColumn(PropertyInfo prop, LambdaExpression lambda)
        {
            // ColumnManager has generic AddSimple expecting Expression<Func<TEntity, TValue?>>; use object as TValue
            var addSimpleMethod = typeof(ColumnManager<TEntity>).GetMethods()
                .FirstOrDefault(m => m.Name == "AddSimple" && m.GetParameters().Length >= 1 && m.GetParameters()[0].ParameterType.Name.StartsWith("Expression"));

            if (addSimpleMethod != null)
            {
                var genericMethod = addSimpleMethod.MakeGenericMethod(typeof(object));
                try
                {
                    // TODO: Add in the order of the columns in TableColumns, not in the order of the properties in TEntity.

                    // TODO: Set ColumnInfo title, fullName and class
                    var columnInfo = new ColumnInfo(prop.Name, prop.Name, null);
                    genericMethod.Invoke(ColumnManager, [lambda, columnInfo, null, Align.Left, null, null, true, null]);
                }
                catch
                {
                    // ignore any failures adding a specific column
                    // TODO: What should we do here? Log the error? Show a message in the UI?
                }
            }
        }


        //public DynamicColumn<TGridItem> AddSimple<TValue>(Expression<Func<TGridItem, TValue?>> expression, ColumnInfo columnInfo,                          string? format = null,                       Align align = Align.Left, CellStyleMap<TValue>? cellStyle = null, GridSort<TGridItem>? sortBy = null, bool visible = true, string? propertyName = null)
        //public DynamicColumn<TGridItem> AddNumber(Expression<Func<TGridItem, decimal?>>        expression, string? title = null, string? fullTitle = null, string format = "N0", string? @class = null, Align align = Align.Right, bool visible = true, string? propertyName = null, bool? calculateTotal = null)
        //public DynamicColumn<TGridItem> AddNumber(Expression<Func<TGridItem, double?>>         expression, string? title = null, string? fullTitle = null, string format = "N0", string? @class = null, Align align = Align.Right, bool visible = true, string? propertyName = null, bool? calculateTotal = null)

        private void AddNumberColumn(PropertyInfo prop, LambdaExpression lambda)
        {
            var addSimpleMethod = typeof(ColumnManager<TEntity>).GetMethods()
                .FirstOrDefault(m => m.Name == "AddNumber" && m.GetParameters().Length >= 1 && m.GetParameters()[0].ParameterType.Name.StartsWith("Expression"));

            if (addSimpleMethod != null)
            {
                var genericMethod = addSimpleMethod.MakeGenericMethod(typeof(object));
                try
                {
                    genericMethod.Invoke(ColumnManager, [lambda, prop.Name, prop.Name, "N0", null, Align.Right, true, null, null]);
                }
                catch
                {
                    // ignore any failures adding a specific column
                    // TODO: What should we do here? Log the error? Show a message in the UI?
                }
            }
        }

        /// <summary>
        /// Gets rows from the database table in <seealso cref="TableName"/> and maps them to a list of <typeparamref name="T1"/>.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="DbConnection">A database connection e.g an SqlConnection (for MS SQL Server)/param>
        /// <param name="tableName">The rows retrieved from this database table.</param>
        /// <param name="cols">The columns to retrieve.</param>
        static public async Task<IQueryable<T>> GetTableRowsAsync<T>(DbConnection DbConnection, string sql)
        {
            IEnumerable<dynamic> dynRows = await DbConnection.QueryAsync(sql);
            return Helpers.DynamicHelper.MapDynRows<T>(dynRows).AsQueryable();
        }

    }
}
