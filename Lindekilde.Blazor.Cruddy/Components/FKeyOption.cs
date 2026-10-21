namespace Lindekilde.Blazor.Cruddy.Components;

/// <summary>
/// A single dropdown option for a foreign key.
/// </summary>
public class FKeyOption
{
    /// <summary>
    /// The value of the foreign key option, which corresponds to the primary key of the related table.
    /// </summary>
    public object? Id { get; set; }
    
    /// <summary>
    /// The display text of the foreign key option, which is a human-readable representation of the related table's primary key.
    /// </summary>
    public object? Display { get; set; }
}
