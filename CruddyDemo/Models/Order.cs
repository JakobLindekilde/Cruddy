using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CruddyDemo.Models;

public partial class Order
{
    public int Id { get; set; }

    public int? Cid { get; set; }

    public int? Pid { get; set; }

    public int? Eid { get; set; }

    [DisplayName("Employee")]
    public int? Qty { get; set; }

    public int? Paid { get; set; }

    [DataType(DataType.Date)]
    public DateTime? OrderDate { get; set; }

    public virtual Customer? CidNavigation { get; set; }

    public virtual Employee? EidNavigation { get; set; }

    public virtual Product? PidNavigation { get; set; }
}

public partial class OrderCruddy : Order
{
    [DisplayName("Customer")]
    public string? CustomerName { get; set; }
    
    [DisplayName("Product")]
    public string? ProductName { get; set; }
}
