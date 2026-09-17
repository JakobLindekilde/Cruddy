using System.ComponentModel.DataAnnotations;

namespace CruddyDemo.Models;

public partial record Address
{
    public int Id { get; init; }

    public int CustomerId { get; init; }

    public string Town { get; init; } = null!;

    public string Street { get; init; } = null!;

    public int StreetNo { get; init; }

    [Display(Name = "Zip")]
    public string ZipCode { get; init; } = null!;

    public AddressType AddressType { get; init; }

    public virtual Customer? Customer { get; init; } = null!;
}
