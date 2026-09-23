using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CruddyDemo.Models;

//public record CustomerRec(int Id, string Name, string Email, int Phone)
//{
//    // TODO: Does Dapper always require a parameterless constructor? (I think so, but it need to be verified.)
//    // Parameterless constructor initializing default values
//    public CustomerRec() : this(0, string.Empty, string.Empty, 0) { }
//};

public class Customer
{
    public Customer() { }

    [Key]
    public int Id { get; set; }

    [DisplayName("Customer Name")]
    public required string Name { get; set; }

    [MaxLength(30, ErrorMessage = "Email cannot exceed 30 digits.")]    
    public required string Email { get; set; }

    public bool Vip { get; set; }

    public int Phone { get; set; }

    [DisplayName("Birth Date")]
    [DisplayFormat(DataFormatString = "dd-MM-yyyy", ApplyFormatInEditMode = true)]
    public DateTime Birthdate { get; set; }

}
