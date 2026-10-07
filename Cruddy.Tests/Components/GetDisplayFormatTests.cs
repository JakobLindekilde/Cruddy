using Cruddy.Components;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

#pragma warning disable BL0005 // Component parameter should not be set outside of its component.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

namespace Cruddy.Tests.Components;

public class GetDisplayFormatTests
{
    private class FormatModel
    {
        public string Text { get; set; } = string.Empty;
        public int Int { get; set; }
        public int? NullableInt { get; set; }
        public long Long { get; set; }
        public short Short { get; set; }
        public byte Byte { get; set; }
        public decimal Decimal { get; set; }
        public decimal? NullableDecimal { get; set; }
        public double Double { get; set; }
        public float Float { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime? NullableDateTime { get; set; }
        public DateOnly DateOnly { get; set; }
        public DateOnly? NullableDateOnly { get; set; }
        public TimeOnly TimeOnly { get; set; }
        public TimeOnly? NullableTimeOnly { get; set; }
        public bool Bool { get; set; }
        public Guid Guid { get; set; }
        public TimeSpan TimeSpan { get; set; }

        [DisplayFormat(DataFormatString = "attr-format")]
        public int AttributeInt { get; set; }

        [DisplayFormat(DataFormatString = "attr-format")]
        public decimal AttributeDecimal { get; set; }

        [DisplayFormat(DataFormatString = "attr-format")]
        public DateTime AttributeDateTime { get; set; }

        [DisplayFormat(DataFormatString = "attr-format")]
        public string AttributeText { get; set; } = string.Empty;

        [DisplayFormat(DataFormatString = "")]
        public int EmptyAttributeInt { get; set; }

        [DisplayFormat(DataFormatString = "")]
        public DateTime EmptyAttributeDateTime { get; set; }
    }

    private class TestCruddy : Cruddy<FormatModel>
    {
        public string? Call(PropertyInfo prop) => GetDisplayFormat(prop);
    }

    private static TestCruddy Create(
        string? number = "number-default",
        string? decimalFormat = "decimal-default",
        string? date = "date-default",
        string? dateTime = "datetime-default",
        string? time = "time-default") =>
        new()
        {
            DbConnection = null,
            DefaultNumberFormat = number,
            DefaultDecimalFormat = decimalFormat,
            DefaultDateFormat = date,
            DefaultDateTimeFormat = dateTime,
            DefaultTimeFormat = time
        };

    private static PropertyInfo Property(string name) => typeof(FormatModel).GetProperty(name)!;

    [Theory]
    [InlineData(nameof(FormatModel.Int), "number-default")]
    [InlineData(nameof(FormatModel.NullableInt), "number-default")]
    [InlineData(nameof(FormatModel.Long), "number-default")]
    [InlineData(nameof(FormatModel.Short), "number-default")]
    [InlineData(nameof(FormatModel.Byte), "number-default")]
    [InlineData(nameof(FormatModel.Decimal), "decimal-default")]
    [InlineData(nameof(FormatModel.NullableDecimal), "decimal-default")]
    [InlineData(nameof(FormatModel.Double), "decimal-default")]
    [InlineData(nameof(FormatModel.Float), "decimal-default")]
    [InlineData(nameof(FormatModel.DateTime), "datetime-default")]
    [InlineData(nameof(FormatModel.NullableDateTime), "datetime-default")]
    [InlineData(nameof(FormatModel.DateOnly), "date-default")]
    [InlineData(nameof(FormatModel.NullableDateOnly), "date-default")]
    [InlineData(nameof(FormatModel.TimeOnly), "time-default")]
    [InlineData(nameof(FormatModel.NullableTimeOnly), "time-default")]
    public void ReturnsDefaultFormatForType(string propertyName, string expected)
    {
        Assert.Equal(expected, Create().Call(Property(propertyName)));
    }

    [Theory]
    [InlineData(nameof(FormatModel.Text))]
    [InlineData(nameof(FormatModel.Bool))]
    [InlineData(nameof(FormatModel.Guid))]
    [InlineData(nameof(FormatModel.TimeSpan))]
    public void ReturnsNullForTypesWithoutDefaultFormat(string propertyName)
    {
        Assert.Null(Create().Call(Property(propertyName)));
    }

    [Theory]
    [InlineData(nameof(FormatModel.AttributeInt))]
    [InlineData(nameof(FormatModel.AttributeDecimal))]
    [InlineData(nameof(FormatModel.AttributeDateTime))]
    [InlineData(nameof(FormatModel.AttributeText))]
    public void AttributeTakesPrecedenceOverDefaults(string propertyName)
    {
        Assert.Equal("attr-format", Create().Call(Property(propertyName)));
    }

    [Fact]
    public void AttributeTakesPrecedence_EvenWhenDefaultsAreNull()
    {
        var dut = Create(null, null, null, null, null);

        Assert.Equal("attr-format", dut.Call(Property(nameof(FormatModel.AttributeInt))));
    }

    [Theory]
    [InlineData(nameof(FormatModel.EmptyAttributeInt), "number-default")]
    [InlineData(nameof(FormatModel.EmptyAttributeDateTime), "datetime-default")]
    public void EmptyAttributeFallsBackToDefault(string propertyName, string expected)
    {
        Assert.Equal(expected, Create().Call(Property(propertyName)));
    }

    [Theory]
    [InlineData(nameof(FormatModel.Int))]
    [InlineData(nameof(FormatModel.Long))]
    [InlineData(nameof(FormatModel.NullableInt))]
    public void NumberDefaultNullOrEmpty_ReturnsNull(string propertyName)
    {
        Assert.Null(Create(number: null).Call(Property(propertyName)));
        Assert.Null(Create(number: "").Call(Property(propertyName)));
    }

    [Theory]
    [InlineData(nameof(FormatModel.Decimal))]
    [InlineData(nameof(FormatModel.Double))]
    [InlineData(nameof(FormatModel.NullableDecimal))]
    public void DecimalDefaultNullOrEmpty_ReturnsNull(string propertyName)
    {
        Assert.Null(Create(decimalFormat: null).Call(Property(propertyName)));
        Assert.Null(Create(decimalFormat: "").Call(Property(propertyName)));
    }

    [Theory]
    [InlineData(nameof(FormatModel.DateTime))]
    [InlineData(nameof(FormatModel.NullableDateTime))]
    public void DateTimeDefaultNullOrEmpty_ReturnsNull(string propertyName)
    {
        Assert.Null(Create(dateTime: null).Call(Property(propertyName)));
        Assert.Null(Create(dateTime: "").Call(Property(propertyName)));
    }

    [Theory]
    [InlineData(nameof(FormatModel.DateOnly))]
    [InlineData(nameof(FormatModel.NullableDateOnly))]
    public void DateDefaultNullOrEmpty_ReturnsNull(string propertyName)
    {
        Assert.Null(Create(date: null).Call(Property(propertyName)));
        Assert.Null(Create(date: "").Call(Property(propertyName)));
    }

    [Theory]
    [InlineData(nameof(FormatModel.TimeOnly))]
    [InlineData(nameof(FormatModel.NullableTimeOnly))]
    public void TimeDefaultNullOrEmpty_ReturnsNull(string propertyName)
    {
        Assert.Null(Create(time: null).Call(Property(propertyName)));
        Assert.Null(Create(time: "").Call(Property(propertyName)));
    }

    [Fact]
    public void DefaultsAreIndependentPerType()
    {
        var dut = Create(number: null);

        Assert.Null(dut.Call(Property(nameof(FormatModel.Int))));
        Assert.Equal("decimal-default", dut.Call(Property(nameof(FormatModel.Decimal))));
        Assert.Equal("date-default", dut.Call(Property(nameof(FormatModel.DateOnly))));
    }
}
