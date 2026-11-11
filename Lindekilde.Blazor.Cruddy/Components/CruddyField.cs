namespace Lindekilde.Blazor.Cruddy.Components;

/// <summary>
/// Describes a table column that is used as a CRUD field. The information is read from the database catalog
/// (sys.columns, sys.tables, sys.types, sys.indexes etc.).
/// </summary>
/// <param name="Column">The column name in the database table.</param>
/// <param name="Name">The name used for the field in a row. Same as <paramref name="Column"/> unless an alias is specified in TableColumns.</param>
/// <param name="Type">The (non-nullable) CLR type that corresponds to the SQL type of the column.</param>
/// <param name="IsNullable">Whether the column allows NULL.</param>
/// <param name="IsPrimaryKey">Whether the column is part of the primary key.</param>
/// <param name="IsReadOnly">Whether the column can not be written to (computed columns and rowversion/timestamp).</param>
/// <param name="IsDate">Whether the column is a SQL date column (no time part).</param>
public sealed record CruddyField(
    string Column,
    string Name,
    Type Type,
    bool IsNullable,
    bool IsPrimaryKey,
    bool IsReadOnly,
    bool IsDate);
