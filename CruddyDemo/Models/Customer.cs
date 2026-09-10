namespace CruddyDemo.Models
{
    //public record Customer(int Id, string Name, string Email, int Phone)
    //{
    //    // TODO: Does Dapper always require a parameterless constructor? (I think so, but it need to be verified.)
    //    // Parameterless constructor initializing default values
    //    public Customer() : this(0, string.Empty, string.Empty, 0) { }
    //};

    public class Customer
    {
        public Customer() { }

        public int Id { get; set; }
        
        public required string Name { get; set; }

        public required string Navn { get; set; }

        public required string Email { get; set; }
        
        public int Phone { get; set; }

        public DateTime Birthdate { get; set; }

        public Decimal Salary { get; set; }

        public required string Child { get; set; }

    }
}
