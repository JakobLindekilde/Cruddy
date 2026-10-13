namespace Cruddy.Components;

/// <summary>
/// Represents information about a foreign key relationship between two tables in a database.
/// </summary>
public class FKeyInfo
{

    /// <summary>Foreign key to column <seealso cref="PrimaryId"/> in table <seealso cref="PrimaryTable"/>.</summary>
    public string FKey { get; init; }

    /// <summary>Primary table name (that FKey references).</summary>
    public string PrimaryTable { get; init; }

    /// <summary>Primary key column name in the <seealso cref="PrimaryTable"/> table (that FKey references).</summary>
    public string PrimaryId { get; init; }

    /// <summary>
    /// The primary display column name in the <seealso cref="PrimaryTable"/> table (that FKey references). 
    /// This is used to display a human-readable value for the foreign key relationship.
    /// </summary>
    public string PrimaryDisplay { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FKeyInfo"/> class with the 
    /// specified foreign key, primary table, primary ID column, and primary display column.
    /// </summary>
    /// <param name="fKey">The foreign key column name.</param>
    /// <param name="primaryTable">The primary table name that FKey references.</param>
    /// <param name="primaryId">The primary ID column name in the primary table.</param>
    /// <param name="primaryDisplay">The primary display column name in the primary table.</param>
    public FKeyInfo(string fKey, string primaryTable, string primaryId, string primaryDisplay)
    {
        FKey = fKey;
        PrimaryTable = primaryTable;
        PrimaryId = primaryId;
        PrimaryDisplay = primaryDisplay;
    }

}
