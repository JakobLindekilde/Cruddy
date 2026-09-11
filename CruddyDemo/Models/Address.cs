using System.ComponentModel;

namespace CruddyDemo.Models;

public partial class Address
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string Town { get; set; } = null!;

    public string Street { get; set; } = null!;

    public int StreetNo { get; set; }

    public string ZipCode { get; set; } = null!;

    public AddressType AddressType { get; set; }

    public virtual Customer? Customer { get; set; } = null!;
}

