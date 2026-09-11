using Dapper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using QuickGrid.Toolkit;
using QuickGrid.Toolkit.Columns;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using CruddyDemo.Helpers;

namespace CruddyDemo.Components
{
    /// <summary>
    /// Base class for simple CRUD operations in a blazor component.
    /// </summary>
    public partial class Cruddy<TEntity>   // Tip: public partial class CruddyNy<T> : ComponentBase
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
        /// <remarks>Default is false.</remarks>
        [Parameter]
        public bool? Distinct { get; set; } = false;

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
        /// The default date format to use when displaying date values 
        /// when not using <seealso cref="DisplayFormatAttribute"/>.
        /// </summary>
        [Parameter]
        public string? DefaultDateFormat { get; set; } = "dd-MM-yyyy";

        /// <summary>
        /// The default decimal, double and float format to use when displaying decimal values 
        /// when not using <seealso cref="DisplayFormatAttribute"/>.
        /// </summary>
        [Parameter]
        public string? DefaultDecimalFormat { get; set; } = "0.00";

        /// <summary>
        /// The default format to use when displaying numbers 
        /// when not using <seealso cref="DisplayFormatAttribute"/>.
        /// </summary>
        [Parameter]
        public string? DefaultNumberFormat { get; set; } = "0";

        /// <summary>
        /// The QuickGrid component that displays the rows retrieved from the database.
        /// </summary>
        protected QuickGrid<TEntity>? MyGrid;

        /// <summary>
        /// The ColumnManager that manages the columns of the QuickGrid component.
        /// </summary>
        protected readonly ColumnManager<TEntity> ColumnManager = new();

        /// <summary>
        /// The rows retrieved from the database.
        /// </summary>
        protected List<TEntity>? Rows;


        /// <summary>
        /// A dictionary that maps the column names specified in <seealso cref="TableColumns"/> 
        /// to their corresponding property names in <typeparamref name="TEntity"/>.
        /// </summary>
        readonly Dictionary<string, string> ColumnAliasDict = new(StringComparer.OrdinalIgnoreCase);

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
            // TODO: Handle '[' and ']' in column names (e.g. [Customer Name] AS CustomerName). I'm not sure if it works
            // with Dapper or the code in BuildSql()

            string sql;

            if (!string.IsNullOrEmpty(Select))
            {
                sql = Select;
            }
            else
            {
                sql = $"SELECT {(Distinct.GetValueOrDefault() ? "DISTINCT " : "")}{(Top.HasValue ? $"TOP {Top.Value} " : "")}" +
                    $" {TableColumns} FROM {TableName}";

            }

            if (!sql.Contains(" WHERE ", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(Where))
            {
                sql += $" WHERE {Where}";
            }

            if (!sql.Contains("ORDER BY", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(OrderBy))
            {
                sql += $" ORDER BY {OrderBy}";


                if (!sql.EndsWith(" ASC", StringComparison.InvariantCultureIgnoreCase) && 
                    !sql.EndsWith(" DESC", StringComparison.InvariantCultureIgnoreCase) && SortOrder != SortOrder.None)
                {
                    sql += $" {(SortOrder == SortOrder.Ascending ? "ASC" : "DESC")}";
                }
            }

            // TODO: Should we check if the SQL statement is valid? Maybe there is a NuGet package that can do this?
            // Or we can just try to execute the SQL statement and catch any exception.

            return sql;
        }

        /// <summary>
        /// During component initialization, this method fills <see cref="ColumnAliasDict"/>, 
        /// adds columns to the grid and retrieves the rows from the database.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            FillColumnAliasDict();
            AddColumnsToGrid();
            Rows = GetTableRows<TEntity>(DbConnection, BuildSql());
        }

        /// <summary>
        /// Add a simple column, using AddSimple(), for each public readable property on TEntity.
        /// </summary>
        protected virtual void AddColumnsToGrid()
        {
            Type entityType = typeof(TEntity);
            var props = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead);

            if (TableColumns == "*")
            {
                foreach (var prop in props)
                {
                    AddColumn(prop);
                }
            }
            else
            {
                foreach (var col in ColumnAliasDict)
                {
                    var prop = props.FirstOrDefault(p => p.Name.Equals(col.Value, StringComparison.OrdinalIgnoreCase));
                    if (prop != null)
                    {
                        AddColumn(prop);
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the type name is a number type (Decimal, Int32, Double, Single, Int64, UInt32, UInt64).
        /// </summary>
        public static bool IsNumber(string typeName)
        {
            return typeName == "Decimal" || typeName == "Int32" || typeName == "Double" || typeName == "Single" || typeName == "Int64" || typeName == "UInt32" || typeName == "UInt64";
        }

        /// <summary>
        /// Adds a column to the QuicGrid for the specified property
        /// </summary>
        protected virtual void AddColumn(PropertyInfo prop)
        {
            if (IsNumber(prop.PropertyType.Name))
            {
                AddNumberColumn(prop);
            }
            else if (prop.PropertyType.Name == "DateTime")
            {
                AddSimpleDateColumn(prop);
            }
            else
            {
                AddSimpleColumn(prop);
            }
        }

        //public DynamicColumn<TGridItem> AddNumber(Expression<Func<TGridItem, decimal?>>        expression, string? title = null, string? fullTitle = null, string format = "N0", string? @class = null, Align align = Align.Right, bool visible = true, string? propertyName = null, bool? calculateTotal = null)
        //public DynamicColumn<TGridItem> AddNumber(Expression<Func<TGridItem, double?>>         expression, string? title = null, string? fullTitle = null, string format = "N0", string? @class = null, Align align = Align.Right, bool visible = true, string? propertyName = null, bool? calculateTotal = null)
        //public DynamicColumn<TGridItem> AddSimpleDate<TValue>(Expression<Func<TGridItem, TValue?>> expression, string? title = null, string? fullTitle = null, string? format = "dd/MM/yyyy", string? @class = null, Align align = Align.Center, CellStyleMap<TValue>? cellStyle = null, bool visible = true)
        //public DynamicColumn<TGridItem> AddSimple    <TValue>(Expression<Func<TGridItem, TValue?>> expression, ColumnInfo columnInfo, string? format = null, Align align = Align.Left, CellStyleMap<TValue>? cellStyle = null, GridSort<TGridItem>? sortBy = null,  bool visible = true, string? propertyName = null)

        /// <summary>
        /// Adds a Toolkit.AddSimple column to the QuickGrid for the specified property.
        /// </summary>
        /// <param name="prop">The property to add a column for.</param>
        private void AddSimpleColumn(PropertyInfo prop)
        {
            // TODO: Make sure to get the correct overload of AddSimple(). Count the parameters!
            var method = typeof(ColumnManager<TEntity>).GetMethods()
                .FirstOrDefault(m => m.Name == "AddSimple" && m.GetParameters()[0].ParameterType.Name.StartsWith("Expression"));

            if (method != null)
            {
                // AddSimple is a generic method, so we need to make it generic with the correct type argument. In this case,
                // we can use object as the type argument, since we don't know the actual type of the property at compile time.
                var genericMethod = method.MakeGenericMethod(typeof(object));

                try
                {
                    Type entityType = typeof(TEntity);
                    var param = Expression.Parameter(entityType, "p");
                    var access = Expression.PropertyOrField(param, prop.Name);

                    var delegateType = typeof(Func<,>).MakeGenericType(entityType, typeof(object));
                    var returnType = delegateType.GetMethod("Invoke").ReturnType;

                    // convert access to the expected return type if needed
                    Expression body = access;
                    if (access.Type != returnType)
                    {
                        body = Expression.Convert(access, returnType);
                    }

                    // create a strongly-typed lambda matching the overload
                    var lambda = Expression.Lambda(delegateType, body, param);

                    var displayName = PropertyHelper.GetDisplayName(prop);
                    var format = GetDisplayFormat(prop);
                    var columnInfo = new ColumnInfo(displayName, displayName, null);
                    genericMethod.Invoke(ColumnManager, [lambda, columnInfo, format, Align.Left, null, null, true, null]);
                    // Would be nice if the code below worked, but it doesn't because of the generic type parameter. So we have to use reflection to invoke the method.
                    //ColumnManager.AddSimple(           lambda, columnInfo, format, Align.Left, null, null, true, null);
                }
                catch
                {
                    // ignore any failures adding a specific column
                    // TODO: What should we do here? Log the error? Show a message in the UI?
                }
            }
        }

        /// <summary>
        /// Adds a Toolkit.AddNumber column to the QuickGrid for the specified property.
        /// </summary>
        /// <param name="prop">The property to add a column for.</param>
        private void AddNumberColumn(PropertyInfo prop)
        {
            var method = typeof(ColumnManager<TEntity>).GetMethods()
                .FirstOrDefault(m => m.Name == "AddNumber" && m.GetParameters()[0].ParameterType.Name.StartsWith("Expression"));

            if (method != null)
            {
                try
                {
                    Type entityType = typeof(TEntity);
                    var param = Expression.Parameter(entityType, "p");
                    var access = Expression.PropertyOrField(param, prop.Name);

                    var firstParamType = method.GetParameters()[0].ParameterType; // Expression<TDelegate>
                    var delegateType = firstParamType.GetGenericArguments()[0];          // TDelegate (e.g. Func<Customer, Nullable<decimal>>)
                    var returnType = delegateType.GetMethod("Invoke").ReturnType;        // Nullable<decimal> (or decimal/other)

                    // convert access to the expected return type if needed
                    Expression body = access;
                    if (access.Type != returnType)
                    {
                        body = Expression.Convert(access, returnType);
                    }

                    // create a strongly-typed lambda matching the overload
                    var lambda = Expression.Lambda(delegateType, body, param);

                    var displayName = PropertyHelper.GetDisplayName(prop);
                    var format = GetDisplayFormat(prop);
                    method.Invoke(ColumnManager, [lambda, displayName, displayName, format, null, Align.Left, true, null, null]);
                    // Would be nice if the code below worked, but it doesn't because of the generic type parameter. So we have to use reflection to invoke the method.
                    //ColumnManager.AddNumber(    lambda, displayName, displayName, format, null, Align.Left, true, null, null);
                }
                catch
                {
                    // ignore any failures adding a specific column
                    // TODO: What should we do here? Log the error? Show a message in the UI?
                }
            }
        }

        /// <summary>
        /// Adds a Toolkit.AddSimpleDate column to the QuickGrid for the specified property.
        /// </summary>
        /// <param name="prop">The property to add a column for.</param>
        private void AddSimpleDateColumn(PropertyInfo prop)
        {
            var method = typeof(ColumnManager<TEntity>).GetMethods()
                .FirstOrDefault(m => m.Name == "AddSimpleDate" && m.GetParameters()[0].ParameterType.Name.StartsWith("Expression"));

            if (method != null)
            {
                // AddSimpleDate is a generic method, so we need to make it generic with the correct type argument. In this case,
                // we can use object as the type argument, since we don't know the actual type of the property at compile time.
                var genericMethod = method.MakeGenericMethod(typeof(object));

                try
                {
                    Type entityType = typeof(TEntity);
                    var param = Expression.Parameter(entityType, "p");
                    var access = Expression.PropertyOrField(param, prop.Name);

                    var delegateType = typeof(Func<,>).MakeGenericType(entityType, typeof(object));
                    var returnType = delegateType.GetMethod("Invoke").ReturnType;

                    // convert access to the expected return type if needed
                    Expression body = access;
                    if (access.Type != returnType)
                    {
                        body = Expression.Convert(access, returnType);
                    }

                    // create a strongly-typed lambda matching the overload
                    var lambda = Expression.Lambda(delegateType, body, param);

                    var displayName = PropertyHelper.GetDisplayName(prop);
                    var format = GetDisplayFormat(prop);
                    genericMethod.Invoke(ColumnManager, [lambda, displayName, displayName, format, null, Align.Left, null, true]);
                    // Would be nice if the code below worked, but it doesn't because of the generic type parameter. So we have to use reflection to invoke the method.
                    //ColumnManager.AddSimpleDate(       lambda, displayName, displayName, format, null, Align.Left, null, true); 
                }
                catch
                {
                    // ignore any failures adding a specific column
                    // TODO: What should we do here? Log the error? Show a message in the UI?
                }
            }
        }

        /// <summary>
        /// Gets the display format for the specified property either 
        /// from the DisplayFormatAttribute, or from the default formats.
        /// </summary>
        /// <param name="prop">The property to get the display format for.</param>
        /// <returns>The display format string, or null if none is specified.</returns>
        protected string? GetDisplayFormat(PropertyInfo prop)
        {
            var displayFormat = PropertyHelper.GetDisplayFormat(prop);
            if (!string.IsNullOrEmpty(displayFormat))
            {
                return displayFormat;
            }

            var propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            if (propertyType == typeof(decimal) || propertyType == typeof(double) || propertyType == typeof(float))
            {
                return string.IsNullOrEmpty(DefaultDecimalFormat) ? null : DefaultDecimalFormat;
            }

            if (propertyType == typeof(int) ||
                propertyType == typeof(long) ||
                propertyType == typeof(short) ||
                propertyType == typeof(byte) ||
                propertyType == typeof(uint) ||
                propertyType == typeof(ulong) ||
                propertyType == typeof(ushort) ||
                propertyType == typeof(sbyte))
            {
                return string.IsNullOrEmpty(DefaultNumberFormat) ? null : DefaultNumberFormat;
            }

            // TODO: Should we handle DateTimeOffset like DateTime?
            if (propertyType == typeof(DateTime))
            {
                return string.IsNullOrEmpty(DefaultDateFormat) ? null : DefaultDateFormat;
            }

            return null;
        }

        /// <summary>
        /// Gets rows from the database table in <seealso cref="TableName"/> or <seealso cref="Select"/>
        /// and maps them to a list of <typeparamref name="T1"/>.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="DbConnection">A database connection e.g an SqlConnection (for MS SQL Server)/param>
        /// <param name="tableName">The rows retrieved from this database table.</param>
        /// <param name="cols">The columns to retrieve.</param>
        static public List<T> GetTableRows<T>(DbConnection DbConnection, string sql)
        {
            IEnumerable<dynamic> dynRows = DbConnection.Query(sql);
            return Helpers.DynamicMapper.MapCollection<T>(dynRows).ToList();
        }

    }
}
