using Lindekilde.Blazor.Cruddy.Helpers;
using Lindekilde.Blazor.Cruddy.Repositories;
using Microsoft.AspNetCore.Components;
using System.Data.Common;

namespace Lindekilde.Blazor.Cruddy.Components;

/// <summary>
/// Base class for simple CRUD operations in a blazor component. A row in table <see cref="TableName"/> is the
/// CRUD entity, and <see cref="TableColumns"/> specifies the CRUD fields. information about
/// the fields is read from the database catalog (sys.columns, sys.tables etc.).
/// </summary>
public partial class CruddyBase : ComponentBase
{
    /// <summary>
    /// The default schema for the database table. Default is "dbo".
    /// </summary>
    [Parameter]
    public string DefaultSchema { get; set; } = "dbo";

    /// <summary>
    /// Name of database table to 'CRUD'. A row in this table is the CRUD entity. Required.
    /// </summary>
    [Parameter]
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Whether to pluralize guessed foreign key table names (add 's'). Default is true.
    /// </summary>
    [Parameter]
    public bool PluralizeTableName { get; set; } = true;

    /// <summary>
    /// The columns of table <see cref="TableName"/> that are the CRUD fields, separated by commas.
    /// Default is "*" (all columns). Must be valid column names in the table. Use 'AS' to specify an alias.
    /// </summary>
    [Parameter]
    public string TableColumns { get; set; } = "*";

    /// <summary>
    /// The column that is the primary key of table <see cref="TableName"/>.
    /// If not specified, the primary key of the table is used. If the table has no primary key, 
    /// it will look for a column named "Id".
    /// </summary>
    [Parameter]
    public string? KeyColumn { get; set; }

    /// <summary>
    /// Whether to allow editing of the key column when creating rows (not when updating). Default is false.
    /// </summary>
    [Parameter]
    public bool AllowKeyColumnEditOnCreate { get; set; } = false;

    /// <summary>
    /// The maximum number of rows to retrieve from table <see cref="TableName"/>.
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
    /// Must be a valid column name in table <see cref="TableName"/>.
    /// </summary>
    [Parameter]
    public string? OrderBy { get; set; }

    /// <summary>
    /// The WHERE clause to filter the results by (without the "WHERE" keyword).
    /// </summary>
    [Parameter]
    public string? Where { get; set; }

    /// <summary>
    /// Sorting order for the rows when using <see cref="OrderBy"/>. 
    /// </summary>
    /// <remarks>Default is <see cref="SortOrder.Ascending"/></remarks>
    [Parameter]
    public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

    /// <summary>
    /// Here the complete SQL SELECT statement, including joins, can be specified.
    /// If specified, parameters like <see cref="Top"/> and <see cref="OrderBy"/> are ignored.
    /// The names of the selected columns must match the CRUD fields (<see cref="TableColumns"/>).
    /// </summary>
    [Parameter]
    public string? Select { get; set; }

    /// <summary>
    /// A database connection e.g an SqlConnection. 
    /// </summary>
    [Parameter]
    public required DbConnection DbConnection { get; set; }


    /// <summary>
    /// Foreign key info in format 
    /// "FKey1: PrimaryTable1, PrimaryId1, PrimaryDisplay1; FKey2: PrimaryTable2, PrimaryId2, PrimaryDisplay2; ..."
    /// </summary>
    [Parameter]
    public string? FKeys { get; set; }

    /// <summary>
    /// Foreign key info object that is initialized from the <seealso cref="FKeys"/> parameter.
    /// </summary>
    protected List<FKeyInfo> FKeyInfoList = [];

    /// <summary>
    /// Dropdown options (id and display text) per foreign key column, loaded from FKeyInfoList.
    /// </summary>
    protected Dictionary<string, List<FKeyOption>> FKeyOptions { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Loads the foreign key options for each foreign key column defined in FKeyInfoList.
    /// </summary>
    protected void LoadFKeyOptions()
    {
        if (FKeyOptions.Count > 0)
        {
            // Already loaded, no need to load again    
            return;
        }

        foreach (var info in FKeyInfoList)
        {
            FKeyOptions[info.FKey] = Repository.GetFKeyOptions(info);
        }
    }

    /// <summary>
    /// Gets the dropdown options for a field if it is a foreign key, otherwise null.
    /// </summary>
    protected List<FKeyOption>? GetFKeyOptions(CruddyField field) =>
        FKeyOptions.TryGetValue(field.Name, out var options) ? options : null;

    /// <summary>
    /// All (supported) columns of table <see cref="TableName"/>, read from the database catalog.
    /// </summary>
    public List<CruddyField> AllColumns { get; protected set; } = [];

    /// <summary>
    /// The CRUD fields, i.e. the columns specified in <see cref="TableColumns"/>.
    /// </summary>
    public List<CruddyField> Fields { get; protected set; } = [];

    /// <summary>
    /// The rows retrieved from the database.
    /// </summary>
    protected List<CruddyRow> Rows = [];

    /// <summary>
    /// During component initialization, this method reads the table column info from the database, fills 
    /// <see cref="FKeyInfoList"/>, <see cref="ColumnAliasDict"/> and <see cref="Fields"/>, 
    /// and retrieves the rows from the database.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrWhiteSpace(TableName))
        {
            throw new InvalidOperationException("Parameter TableName is required.");
        }

        FKeyInfoList = FillFKeyInfoList(FKeys, DefaultSchema, PluralizeTableName);

        AllColumns = Repository.GetColumns();
        if (AllColumns.Count == 0)
        {
            throw new InvalidOperationException($"Table {DefaultSchema}.{TableName} was not found or has no supported columns.");
        }

        if (string.IsNullOrEmpty(KeyColumn))
        {
            KeyColumn = (AllColumns.FirstOrDefault(c => c.IsPrimaryKey) ??
                AllColumns.FirstOrDefault(c => c.Column.Equals("Id", StringComparison.OrdinalIgnoreCase)))?.Column;
        }

        FillColumnAliasDict();
        Fields = GetFields(TableColumns);
        Rows = Repository.GetAll(BuildSql());
    }

    /// <summary>
    /// Fills the <seealso cref="FKeyInfoList"/> object from the <seealso cref="FKeys"/> parameter.
    /// </summary>
    public static List<FKeyInfo> FillFKeyInfoList(string? fKeys, string defaultSchema, bool pluralizeTableName)
    {
        var result = new List<FKeyInfo>();
        if (string.IsNullOrEmpty(fKeys))
        {
            return result;
        }

        var fKeyArray = fKeys.Split(';');
        foreach (var fKey in fKeyArray)
        {
            var parts = fKey.Split(':', 2);

            if (parts.Length == 1)
            {
                var guessed = GuessFKeyInfo(fKey, defaultSchema, pluralizeTableName);
                if (guessed != null)
                {
                    result.Add(guessed);
                }
                continue;
            }

            var values = parts[1].Split(',').Select(v => v.Trim()).ToArray();
            if (values.Length != 3)
            {
                continue;
            }

            var primaryTable = values[0];
            if (!primaryTable.Contains('.'))
            {
                primaryTable = defaultSchema + "." + primaryTable;
            }

            result.Add(new FKeyInfo(parts[0].Trim(), primaryTable, values[1], values[2]));
        }

        return result;
    }

    /// <summary>
    /// Guesses the foreign key info based on the foreign key column name.
    /// We expect that the foreign key column name is in the format "{Tablename}{Id}".
    /// </summary>
    /// <param name="fKey">The foreign key column name.</param>
    /// <param name="defaultSchema">The default schema to use if the primary table does not specify one.</param>
    /// <param name="pluralizeTableName">Indicates whether to pluralize the table name if it is not already pluralized.</param>
    /// <returns>The guessed <see cref="FKeyInfo"/> object, or null if it cannot be guessed.</returns>
    public static FKeyInfo? GuessFKeyInfo(string fKey, string defaultSchema, bool pluralizeTableName)
    {
        var primaryId = fKey.EndsWith("Id", StringComparison.OrdinalIgnoreCase) ? "Id" : string.Empty;
        if (string.IsNullOrEmpty(primaryId))
        {
            return null;
        }

        var primaryTable = fKey[..^2];
        if (pluralizeTableName && !PluralizeHelper.IsPluralized(primaryTable))
        {
            primaryTable = PluralizeHelper.Pluralize(primaryTable);
        }
        if (!primaryTable.Contains('.'))
        {
            primaryTable = defaultSchema + "." + primaryTable;
        }

        return new FKeyInfo(fKey, primaryTable, primaryId, "Name");
    }

    /// <summary>
    /// The repository used for all database operations.
    /// </summary>
    protected TableRepositorySqlServer Repository =>
        new(DbConnection, DefaultSchema, TableName, KeyColumn);

    /// <summary>
    /// A dictionary that maps the column names specified in <seealso cref="TableColumns"/> 
    /// to their corresponding field names (the alias, if 'AS' is used, otherwise the column name).
    /// </summary>
    public readonly Dictionary<string, string> ColumnAliasDict = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Parses a comma separated list of columns, where each column can have an alias specified with 'AS'.
    /// </summary>
    /// <param name="columns">The comma separated list of columns.</param>
    /// <returns>The distinct (column, alias) pairs. The alias is the column name if no alias is specified.</returns>
    public static List<(string Column, string Alias)> ParseColumns(string columns)
    {
        var result = new List<(string Column, string Alias)>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var column in columns.Split(',').Select(c => c.Trim()))
        {
            var parts = column.Split([" AS "], StringSplitOptions.RemoveEmptyEntries);

            var key = parts[0].Replace("[", "").Replace("]", "").Trim();
            if (key.Length == 0 || key == "*" || !seen.Add(key))
            {
                continue;
            }

            var alias = parts.Length == 2 ? parts[1].Replace("[", "").Replace("]", "").Trim() : key;
            result.Add((key, alias));
        }

        return result;
    }

    /// <summary>
    /// Fills the <seealso cref="ColumnAliasDict"/> dictionary with the column names and their 
    /// corresponding field names. Use 'AS' to specify an alias. 
    /// </summary>
    public void FillColumnAliasDict()
    {
        foreach (var (column, alias) in ParseColumns(TableColumns))
        {
            ColumnAliasDict.TryAdd(column, alias);
        }
    }

    /// <summary>
    /// Gets the fields that correspond to the specified columns. "*" means all columns of the table.
    /// Columns not found in the table (or of an unsupported type) are ignored.
    /// </summary>
    /// <param name="columns">Comma separated list of columns, or "*".</param>
    /// <returns>The matching fields, in the order they are specified.</returns>
    public List<CruddyField> GetFields(string columns)
    {
        if (columns.Trim() == "*")
        {
            return [.. AllColumns];
        }

        var fields = new List<CruddyField>();
        foreach (var (column, alias) in ParseColumns(columns))
        {
            var field = AllColumns.FirstOrDefault(c => c.Column.Equals(column, StringComparison.OrdinalIgnoreCase));
            if (field is not null)
            {
                fields.Add(field with { Name = alias });
            }
        }

        return fields;
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
    /// Gets the CRUD fields that can be written for the operation (the key column and read-only 
    /// columns are excluded as described in <see cref="IncludeColumn"/>).
    /// </summary>
    /// <param name="operation">The CRUD operation type. Must be Create or Update.</param>
    /// <returns>The fields to include in the SQL operation.</returns>
    /// <exception cref="ArgumentException">Thrown if the operation is not Create or Update.</exception>
    public List<CruddyField> GetFieldsForCreateOrUpdate(CrudOperation operation)
    {
        if (operation != CrudOperation.Create && operation != CrudOperation.Update)
        {
            throw new ArgumentException("Invalid operation. Must be either Create or Update.", nameof(operation));
        }

        return Fields.Where(f => !f.IsReadOnly && IncludeColumn(operation, f.Column)).ToList();
    }

    /// <summary>
    /// Determines whether a given column should be included in the
    /// SQL operation based on the operation type. For example, the key column 
    /// is excluded from UPDATE operations, and from CREATE operations unless AllowKeyColumnEditOnCreate is true.
    /// </summary>
    /// <param name="operation">The CRUD operation type.</param>
    /// <param name="columnName">The name of the column.</param>
    /// <returns>True if the column should be included; otherwise, false.</returns>
    public bool IncludeColumn(CrudOperation operation, string columnName)
    {
        if (string.Equals(columnName, KeyColumn, StringComparison.OrdinalIgnoreCase))
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
    /// Gets the value of a field in a row, or null if the field is missing or the value is DBNull.
    /// </summary>
    public static object? GetCell(CruddyRow row, string name) =>
        row.TryGetValue(name, out var value) && value is not DBNull ? value : null;

    /// <summary>
    /// Gets the key value of a row.
    /// </summary>
    /// <param name="row">The row.</param>
    /// <returns>The value of the key column, or null if the row does not contain it.</returns>
    public object? GetKeyValue(CruddyRow row)
    {
        var name = Fields.FirstOrDefault(f => string.Equals(f.Column, KeyColumn, StringComparison.OrdinalIgnoreCase))?.Name
            ?? KeyColumn;
        return name is null ? null : GetCell(row, name);
    }

    /// <summary>
    /// Deletes a row from the database table based on the specified key value.
    /// </summary>
    /// <param name="keyValue">The value of the key column for the row to delete.</param>
    /// <returns>The result of the delete operation.</returns>
    public object? Delete(object keyValue) => Repository.Delete(keyValue);

    /// <summary>
    /// Updates an existing row in the database table based on the specified key value.
    /// </summary>
    /// <param name="row">The row to update.</param>
    /// <param name="keyValue">The value of the key column for the row to update.</param>
    /// <returns>The number of rows affected.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="row"/> is null.</exception>    
    public int Update(CruddyRow row, object keyValue)
    {
        ArgumentNullException.ThrowIfNull(row);

        var fields = GetFieldsForCreateOrUpdate(CrudOperation.Update);
        return Repository.Update(row, keyValue, fields);
    }

    /// <summary>
    /// Inserts a new row into the database table. By default the key column is excluded from the INSERT
    /// (useful when the key is an identity column). Returns the database scalar result if available
    /// (for example SCOPE_IDENTITY()), otherwise returns the number of rows affected.
    /// </summary>
    /// <param name="row">The row to insert.</param>
    /// <returns>Scalar result from the DB (e.g. new id) or rows affected.</returns>
    public object? Create(CruddyRow row)
    {
        ArgumentNullException.ThrowIfNull(row);

        var fields = GetFieldsForCreateOrUpdate(CrudOperation.Create);
        return Repository.Add(row, fields);
    }

    /// <summary>
    /// Gets a single row from the database table in <seealso cref="TableName"/> based on the specified key value
    /// </summary>
    /// <param name="id">The value of the key column for the row to retrieve.</param>
    /// <param name="columns">The columns to include in the result.</param>
    /// <returns>The row corresponding to the specified key value, or null if not found.</returns>
    public CruddyRow? GetTableRow(object id, string columns = "*") => Repository.GetById(id, columns);
}
