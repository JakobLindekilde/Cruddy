using System.ComponentModel.DataAnnotations;

namespace Cruddy.Tests.Models;

public class TestAllType
{
    [Key]
    public int Id { get; set; }                        // int IDENTITY, PK

    public int Int32NotNull { get; set; }              // int NOT NULL
    public int? Int32Nullable { get; set; }            // int NULL

    public long Int64NotNull { get; set; }             // bigint NOT NULL
    public long? Int64Nullable { get; set; }           // bigint NULL

    public short Int16NotNull { get; set; }            // smallint NOT NULL
    public short? Int16Nullable { get; set; }          // smallint NULL

    public byte ByteNotNull { get; set; }              // tinyint NOT NULL
    public byte? ByteNullable { get; set; }            // tinyint NULL

    public decimal DecimalNotNull { get; set; }        // decimal(18,0) NOT NULL
    public decimal? DecimalNullable { get; set; }      // decimal(18,0) NULL

    public double DoubleNotNull { get; set; }          // float(53) NOT NULL
    public double? DoubleNullable { get; set; }        // float(53) NULL

    public float SingleNotNull { get; set; }           // real NOT NULL
    public float? SingleNullable { get; set; }         // real NULL

    public bool BoolNotNull { get; set; }              // bit NOT NULL
    public bool? BoolNullable { get; set; }            // bit NULL

    public required string StringNotNull { get; set; } // nvarchar(100) NOT NULL
    public string? StringNullable { get; set; }        // nvarchar(100) NULL

    public Guid GuidNotNull { get; set; }              // uniqueidentifier NOT NULL
    public Guid? GuidNullable { get; set; }            // uniqueidentifier NULL
}
