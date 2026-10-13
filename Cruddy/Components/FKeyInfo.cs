namespace Cruddy.Components;

/// <summary>
/// Represents information about a foreign key relationship between two tables in a database.
/// </summary>
public class FKeyInfo
{

    /// <summary>Foreign key to column <seealso cref="PrimaryId"/> in table <seealso cref="PrimaryTable"/>.</summary>
    public string FKey { get; init; }

    public FKeyInfo(string fKey, string primaryTable, string primaryId, string primaryDisplay)
    {
        FKey = fKey;
        PrimaryTable = primaryTable;
        PrimaryId = primaryId;
        PrimaryDisplay = primaryDisplay;
    }

    /// <summary>Primary table name (that FKey references).</summary>
    public string PrimaryTable { get; init; }

    /// <summary>Primary key column name in the <seealso cref="PrimaryTable"/> table (that FKey references).</summary>
    public string PrimaryId { get; init; }

    /// <summary>
    /// The primary display column name in the <seealso cref="PrimaryTable"/> table (that FKey references). 
    /// This is used to display a human-readable value for the foreign key relationship.
    /// </summary>
    public string PrimaryDisplay { get; init; }
}
