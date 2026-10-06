using Cruddy.Helpers;
using Cruddy.Repositories;
using Microsoft.AspNetCore.Components;
using System.Data.Common;
using System.Reflection;

namespace Cruddy.Components;

/// <summary>
/// Base class for simple CRUD operations in a blazor component.
/// </summary>
public partial class CruddyBase<TEntity> : ComponentBase where TEntity : class
{
    /// <summary>
    /// The parameterless constructor initializes the <seealso cref="TableName"/> and <seealso cref="KeyColumn"/> 
    /// properties based on the type of <typeparamref name="TEntity"/>.
    /// </summary>
    public CruddyBase()
    {
        if (string.IsNullOrEmpty(TableName))
        {
            TableName = typeof(TEntity).Name;
            if (PluralizeTableName)
            {
                TableName = PluralizeHelper.Pluralize(TableName);
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
    /// <remarks>Default is <seealso cref="SortOrder.Ascending"/></remarks>
    [Parameter]
    public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

    /// <summary>
    /// Here the complete SQL SELECT statement, including joins, can be specified.
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
    protected List<TEntity> Rows = new();

    /// <summary>
    /// During component initialization, this method fills <see cref="ColumnAliasDict"/>, 
    /// adds columns to the grid and retrieves the rows from the database.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        FillColumnAliasDict();
        Rows = Repository.GetAll(BuildSql());
    }

    /// <summary>
    /// The repository used for all database operations.
    /// </summary>
    protected IRepository<TEntity> Repository =>
        new DapperRepository<TEntity>(DbConnection, DefaultSchema, TableName, KeyColumn);

    /// <summary>
    /// A dictionary that maps the column names specified in <seealso cref="TableColumns"/> 
    /// to their corresponding property names in <typeparamref name="TEntity"/>.
    /// </summary>
    public readonly Dictionary<string, string> ColumnAliasDict = new(StringComparer.OrdinalIgnoreCase);

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
    /// <param name="columnsAlt">An optional alternative set of columns to use instead of <seealso cref="TableColumns"/>.</param>
    /// <returns>An array of <see cref="PropertyInfo"/> objects that match the specified columns and operation.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public PropertyInfo[] GetPropertiesForTableColumns(TEntity entity, CrudOperation operation, string columnsAlt = "")
    {
        ArgumentNullException.ThrowIfNull(entity);

        var colsToUse = string.IsNullOrEmpty(columnsAlt) ? TableColumns : columnsAlt;
        var cols = colsToUse == "*" || operation == CrudOperation.Create
            ? [] : colsToUse.Split(",", StringSplitOptions.TrimEntries).ToList();

        var props = PropertyHelper.GetColumnProperties(entity.GetType())
            .Where(p => TypeHelper.IsSupported(p.PropertyType) &&
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

    /// <summary>
    /// Deletes a row from the database table based on the specified key value.
    /// </summary>
    /// <param name="keyValue">The value of the key column for the row to delete.</param>
    /// <returns>The result of the delete operation.</returns>
    public object? Delete(object keyValue) => Repository.Delete(keyValue);

    /// <summary>
    /// Gets the properties of <typeparamref name="TEntity"/> that correspond to 
    /// the columns specified in <seealso cref="TableColumns"/>
    /// </summary>
    /// <param name="entity">The entity instance from which to get the properties.</param>
    /// <param name="operation">The CRUD operation being performed.</param>
    /// <returns>An array of <see cref="PropertyInfo"/> objects representing the properties to include in the operation.</returns>
    /// <exception cref="ArgumentException">Thrown if the operation is not Create or Update.</exception>
    public PropertyInfo[] GetPropertiesForCreateOrUpdate(TEntity entity, CrudOperation operation)
    {
        if (operation != CrudOperation.Create && operation != CrudOperation.Update)
        {
            throw new ArgumentException("Invalid operation. Must be either Create or Update.", nameof(operation));
        }

        var cols = TableColumns == "*" || operation == CrudOperation.Create
            ? [] : TableColumns.Split(",", StringSplitOptions.TrimEntries).ToList();

        var props = PropertyHelper.GetColumnProperties(entity.GetType())
            .Where(p => p.CanRead && p.CanWrite &&
                        (cols.Count == 0 || (cols.Count > 0 && cols.Contains(p.Name))) && IncludeColumn(operation, p.Name))
            .ToArray();

        return props;
    }

    /// <summary>
    /// Updates an existing entity in the database table based on the specified key value.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="keyValue">The value of the key column for the row to update.</param>
    /// <returns>The number of rows affected.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> is null.</exception>    
    public int Update(TEntity entity, object keyValue)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var props = GetPropertiesForCreateOrUpdate(entity, CrudOperation.Update);
        return Repository.Update(entity, keyValue, props);
    }

    /// <summary>
    /// Inserts a new entity into the database table. By default the key column is excluded from the INSERT
    /// (useful when the key is an identity column). Returns the database scalar result if available
    /// (for example SCOPE_IDENTITY()), otherwise returns the number of rows affected.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    /// <returns>Scalar result from the DB (e.g. new id) or rows affected.</returns>
    public object? Create(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var props = GetPropertiesForCreateOrUpdate(entity, CrudOperation.Create);
        return Repository.Add(entity, props);
    }

    /// <summary>
    /// Gets a single row from the database table in <seealso cref="TableName"/> based on the specified key value
    /// </summary>
    /// <param name="id">The value of the key column for the row to retrieve.</param>    /// <param name="columns">The columns to include in the result.</param>
    /// <returns>The entity corresponding to the specified key value, or null if not found.</returns>
    public TEntity? GetTableRow(object id, string columns = "*") => Repository.GetById(id, columns);

    /// <summary>
    /// Maps a collection of dynamics to a list of strongly typed objects of type T.
    /// </summary>
    /// <typeparam name="T">The type to map the dynamic items to.</typeparam>
    /// <param name="dynItems">The collection of dynamics.</param>
    /// <returns>A list of strongly typed objects of type T.</returns>
    public static List<T> Map<T>(IEnumerable<dynamic> dynItems)
    {
        return DapperRepository<TEntity>.Map<T>(dynItems);
    }
}
