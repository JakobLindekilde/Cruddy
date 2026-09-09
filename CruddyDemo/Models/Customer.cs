namespace CruddyDemo.Models
{
    public record Customer(int Id, string Name, string Email, int Phone)
    {
        // Parameterless constructor initializing default values
        public Customer() : this(0, string.Empty, string.Empty, 0) { }
    };

    //public class Customer
    //{
    //    public Customer() { }

    //    public int Id { get; set; }
    //    public required string Name { get; set; }
    //    public required string Email { get; set; }
    //    public int Phone { get; set; }
    //}
}
