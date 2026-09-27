namespace CruddyDemo.Models;

public partial class KeyColumnIsGuid
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public bool Active { get; set; }
}
