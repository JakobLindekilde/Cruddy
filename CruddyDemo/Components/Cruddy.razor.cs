using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using QuickGrid.Toolkit;
using QuickGrid.Toolkit.Columns;
using System.Linq.Expressions;
using System.Reflection;
using CruddyDemo.Helpers;

namespace CruddyDemo.Components
{
    /// <summary>
    /// Lists the rows of a database table in a QuickGrid component, 
    /// with columns automatically generated from the public properties of TEntity.
    /// </summary>
    public partial class Cruddy<TEntity> : CruddyBase<TEntity> //where TEntity : class, new()
    {
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
        /// Whether to enable the delete functionality for each row.
        /// </summary>
        [Parameter]
        public bool EnableDelete { get; set; } = false;

        /// <summary>
        /// The primary name of the column/field, used for display purposes in the UI. 
        /// Used in messages like "Are you sure you want to delete this {PrimaryName}?".
        /// </summary>
        [Parameter]
        public string? PrimaryName { get; set; }

        /// <summary>
        /// The QuickGrid component that displays the rows retrieved from the database.
        /// </summary>
        protected QuickGrid<TEntity>? MyGrid;

        /// <summary>
        /// The ColumnManager that manages the columns of the QuickGrid component.
        /// </summary>
        protected readonly ColumnManager<TEntity> MyColumnManager = new();

        /// <summary>
        /// During component initialization, this method fills <see cref="ColumnAliasDict"/>, 
        /// adds columns to the grid and retrieves the rows from the database.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            AddColumnsToGrid();
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
                    genericMethod.Invoke(MyColumnManager, [lambda, columnInfo, format, Align.Left, null, null, true, null]);
                    // Would be nice if the code below worked, but it doesn't because of the generic type parameter. So we have to use reflection to invoke the method.
                    //MyColumnManager.AddSimple(           lambda, columnInfo, format, Align.Left, null, null, true, null);
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
                    method.Invoke(MyColumnManager, [lambda, displayName, displayName, format, null, Align.Left, true, null, null]);
                    // Would be nice if the code below worked, but it doesn't because of the generic type parameter. So we have to use reflection to invoke the method.
                    //MyColumnManager.AddNumber(    lambda, displayName, displayName, format, null, Align.Left, true, null, null);
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
                    genericMethod.Invoke(MyColumnManager, [lambda, displayName, displayName, format, null, Align.Left, null, true]);
                    // Would be nice if the code below worked, but it doesn't because of the generic type parameter. So we have to use reflection to invoke the method.
                    //MyColumnManager.AddSimpleDate(       lambda, displayName, displayName, format, null, Align.Left, null, true); 
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
    }
}
