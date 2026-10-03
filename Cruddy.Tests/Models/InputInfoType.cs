using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Cruddy.Tests.Models;

public class InputInfoType
{
    [Key]
    public int Id { get; set; }

    [DisplayName("Component Display Name")]
    [Display(Name = "Data Annotation Display Name")]
    public string Name { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    [DataType(DataType.Time)]
    public DateTime Time { get; set; }

    [DisplayFormat(DataFormatString = "yyyy-MM-dd")]
    public DateTime FormattedDate { get; set; }

    public DateOnly DateOnly { get; set; }
    public TimeOnly TimeOnly { get; set; }
    public decimal Decimal { get; set; }
    public int Number { get; set; }
    public int? NullableNumber { get; set; }
    public string? NullableName { get; set; }
}
