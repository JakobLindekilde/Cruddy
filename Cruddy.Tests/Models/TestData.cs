using System.ComponentModel.DataAnnotations;

namespace Cruddy.Tests.Models;

public class ClassWithKey
{
    public ClassWithKey() { }

    [Display(Name = "Id/Pkey")]
    [Key]
    public int Id { get; set; }

    public required string Name { get; set; }

    [Display(Name = "Info")]
    public string? Description { get; set; }

    private string PrivateProperty { get; set; } = "Private";

    protected string ProtectedProperty { get; set; } = "Protected";
}

public class ClassWithoutKey
{
    public ClassWithoutKey() { }

    public int Id { get; set; }

    public required string Firstname { get; set; }

    public string? Lastname { get; set; }

    [DisplayFormat(DataFormatString = "{0:N0}")]
    public int Age { get; set; }

    public int? Hight { get; set; }

    private string PrivateProperty { get; set; } = "Private";

    protected string ProtectedProperty { get; set; } = "Protected";
}

public class Person : ClassWithKey
{
    public Person() { }

    public string? Email { get; set; }
}
