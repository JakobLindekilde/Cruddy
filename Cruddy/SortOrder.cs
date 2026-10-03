using Cruddy.Components;

namespace Cruddy;

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
