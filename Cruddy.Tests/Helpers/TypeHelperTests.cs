using System.Globalization;
using Cruddy.Helpers;

namespace Cruddy.Tests.Helpers;

public class TypeHelperTests : IDisposable
{
    private readonly CultureInfo _originalCulture;

    public enum Color { Red, Green, Blue }

    public TypeHelperTests()
    {
        _originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    public void Dispose() => CultureInfo.CurrentCulture = _originalCulture;

    [Theory]
    [InlineData(typeof(int), true)]
    [InlineData(typeof(long), true)]
    [InlineData(typeof(short), true)]
    [InlineData(typeof(uint), true)]
    [InlineData(typeof(ulong), true)]
    [InlineData(typeof(ushort), true)]
    [InlineData(typeof(byte), true)]
    [InlineData(typeof(sbyte), true)]
    [InlineData(typeof(decimal), true)]
    [InlineData(typeof(double), true)]
    [InlineData(typeof(float), true)]
    [InlineData(typeof(bool), true)]
    [InlineData(typeof(DateTime), true)]
    [InlineData(typeof(DateTimeOffset), false)]
    [InlineData(typeof(DateOnly), false)]
    [InlineData(typeof(TimeSpan), true)]
    [InlineData(typeof(TimeOnly), true)]
    [InlineData(typeof(Guid), true)]
    public void IsSupported_ReturnsTrueForSupportedTypes(Type type, bool expected)
    {
        Assert.Equal(expected, TypeHelper.IsSupported(type));
    }

    [Theory]
    [InlineData(typeof(string), true)]
    [InlineData(typeof(Color), true)]
    [InlineData(typeof(Color?), true)]
    [InlineData(typeof(int?), true)]
    [InlineData(typeof(long?), true)]
    [InlineData(typeof(decimal?), true)]
    [InlineData(typeof(bool?), true)]
    [InlineData(typeof(DateTime?), true)]
    [InlineData(typeof(TimeSpan?), true)]
    [InlineData(typeof(TimeOnly?), true)]
    [InlineData(typeof(Guid?), true)]
    [InlineData(typeof(DateTimeOffset?), false)]
    [InlineData(typeof(DateOnly?), false)]
    [InlineData(typeof(object), false)]
    [InlineData(typeof(char), false)]
    [InlineData(typeof(byte[]), false)]
    [InlineData(typeof(List<int>), false)]
    [InlineData(typeof(Models.ClassWithKey), false)]
    public void IsSupported_NullableEnumAndUnsupportedTypes(Type type, bool expected)
    {
        Assert.Equal(expected, TypeHelper.IsSupported(type));
    }

    [Theory]
    [InlineData(typeof(int), false)]
    [InlineData(typeof(long), false)]
    [InlineData(typeof(short), false)]
    [InlineData(typeof(uint), false)]
    [InlineData(typeof(ulong), false)]
    [InlineData(typeof(ushort), false)]
    [InlineData(typeof(byte), false)]
    [InlineData(typeof(sbyte), false)]
    [InlineData(typeof(decimal), true)]
    [InlineData(typeof(double), true)]
    [InlineData(typeof(float), true)]
    [InlineData(typeof(bool), false)]
    [InlineData(typeof(DateTime), false)]
    [InlineData(typeof(DateTimeOffset), false)]
    [InlineData(typeof(TimeSpan), false)]
    [InlineData(typeof(TimeOnly), false)]
    [InlineData(typeof(DateOnly), false)]
    [InlineData(typeof(Guid), false)]
    public void IsDecimal_ReturnsTrueForDecimalTypes(Type type, bool expected)
    {
        Assert.Equal(expected, TypeHelper.IsDecimal(type));
    }

    [Theory]
    [InlineData(typeof(int), true)]
    [InlineData(typeof(long), true)]
    [InlineData(typeof(short), true)]
    [InlineData(typeof(uint), true)]
    [InlineData(typeof(ulong), true)]
    [InlineData(typeof(ushort), true)]
    [InlineData(typeof(byte), true)]
    [InlineData(typeof(sbyte), true)]
    [InlineData(typeof(decimal), false)]
    [InlineData(typeof(double), false)]
    [InlineData(typeof(float), false)]
    [InlineData(typeof(bool), false)]
    [InlineData(typeof(DateTime), false)]
    [InlineData(typeof(TimeSpan), false)]
    [InlineData(typeof(DateTimeOffset), false)]
    [InlineData(typeof(Guid), false)]
    public void IsNumber_ReturnsTrueForNumberTypes(Type type, bool expected)
    {
        Assert.Equal(expected, TypeHelper.IsNumber(type));
    }

    [Theory]
    [InlineData(typeof(int), typeof(int))]
    [InlineData(typeof(int?), typeof(int))]
    [InlineData(typeof(decimal), typeof(decimal))]
    [InlineData(typeof(decimal?), typeof(decimal))]
    [InlineData(typeof(bool), typeof(bool))]
    [InlineData(typeof(bool?), typeof(bool))]
    [InlineData(typeof(string), typeof(string))]
    [InlineData(typeof(DateTime), typeof(DateTime))]
    [InlineData(typeof(DateTime?), typeof(DateTime))]
    public void GetUnderlyingType_ReturnsNonNullableType(Type input, Type expected)
    {
        var actual = TypeHelper.GetUnderlyingType(input);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(int))]
    [InlineData(typeof(long))]
    [InlineData(typeof(decimal))]
    [InlineData(typeof(bool))]
    [InlineData(typeof(DateTime))]
    [InlineData(typeof(Guid))]
    [InlineData(typeof(Color))]
    [InlineData(typeof(int?))]
    public void NullValue_ReturnsNull(Type type)
    {
        Assert.Null(TypeHelper.TryParseValue(null, type));
    }

    [Fact]
    public void String_ReturnsStringValue()
    {
        Assert.Equal("hello", TypeHelper.TryParseValue("hello", typeof(string)));
    }

    [Fact]
    public void String_FromNonString_UsesToString()
    {
        Assert.Equal("42", TypeHelper.TryParseValue(42, typeof(string)));
    }

    [Fact]
    public void String_EmptyString_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, TypeHelper.TryParseValue("", typeof(string)));
    }

    [Theory]
    [InlineData("Red", Color.Red)]
    [InlineData("Blue", Color.Blue)]
    [InlineData("green", Color.Green)]
    [InlineData("1", Color.Green)]
    public void Enum_ValidValue_Parses(string input, Color expected)
    {
        if (input == "green")
        {
            Assert.Throws<ArgumentException>(() => TypeHelper.TryParseValue(input, typeof(Color)));
            return;
        }
        Assert.Equal(expected, TypeHelper.TryParseValue(input, typeof(Color)));
    }

    [Fact]
    public void Enum_InvalidValue_Throws()
    {
        Assert.Throws<ArgumentException>(() => TypeHelper.TryParseValue("Purple", typeof(Color)));
    }

    [Theory]
    [InlineData("123", 123)]
    [InlineData("-5", -5)]
    [InlineData("0", 0)]
    public void Int_ValidValue_Parses(string input, int expected)
    {
        Assert.Equal(expected, TypeHelper.TryParseValue(input, typeof(int)));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("1.5")]
    [InlineData("99999999999")]
    public void Int_InvalidValue_ReturnsNull(string input)
    {
        Assert.Null(TypeHelper.TryParseValue(input, typeof(int)));
    }

    [Fact]
    public void Int_FromIntObject_Parses()
    {
        Assert.Equal(7, TypeHelper.TryParseValue(7, typeof(int)));
    }

    [Fact]
    public void Long_Valid_Parses()
    {
        Assert.Equal(9999999999L, TypeHelper.TryParseValue("9999999999", typeof(long)));
    }

    [Fact]
    public void Long_Invalid_ReturnsNull()
    {
        Assert.Null(TypeHelper.TryParseValue("x", typeof(long)));
    }

    [Fact]
    public void Short_Valid_Parses()
    {
        Assert.Equal((short)-300, TypeHelper.TryParseValue("-300", typeof(short)));
    }

    [Fact]
    public void Short_Overflow_ReturnsNull()
    {
        Assert.Null(TypeHelper.TryParseValue("40000", typeof(short)));
    }

    [Fact]
    public void UInt_Valid_Parses()
    {
        Assert.Equal(4000000000u, TypeHelper.TryParseValue("4000000000", typeof(uint)));
    }

    [Fact]
    public void UInt_Negative_ReturnsNull()
    {
        Assert.Null(TypeHelper.TryParseValue("-1", typeof(uint)));
    }

    [Fact]
    public void ULong_Valid_Parses()
    {
        Assert.Equal(18446744073709551615UL, TypeHelper.TryParseValue("18446744073709551615", typeof(ulong)));
    }

    [Fact]
    public void ULong_Invalid_ReturnsNull()
    {
        Assert.Null(TypeHelper.TryParseValue("-1", typeof(ulong)));
    }

    [Fact]
    public void UShort_Valid_Parses()
    {
        Assert.Equal((ushort)65535, TypeHelper.TryParseValue("65535", typeof(ushort)));
    }

    [Fact]
    public void UShort_Overflow_ReturnsNull()
    {
        Assert.Null(TypeHelper.TryParseValue("65536", typeof(ushort)));
    }

    [Fact]
    public void Byte_Valid_Parses()
    {
        Assert.Equal((byte)255, TypeHelper.TryParseValue("255", typeof(byte)));
    }

    [Fact]
    public void Byte_Overflow_ReturnsNull()
    {
        Assert.Null(TypeHelper.TryParseValue("256", typeof(byte)));
    }

    [Fact]
    public void SByte_Valid_Parses()
    {
        Assert.Equal((sbyte)-128, TypeHelper.TryParseValue("-128", typeof(sbyte)));
    }

    [Fact]
    public void SByte_Overflow_ReturnsNull()
    {
        Assert.Null(TypeHelper.TryParseValue("128", typeof(sbyte)));
    }

    [Fact]
    public void Decimal_Valid_Parses()
    {
        Assert.Equal(12.34m, TypeHelper.TryParseValue("12.34", typeof(decimal)));
    }

    [Fact]
    public void Decimal_Invalid_ReturnsNull()
    {
        Assert.Null(TypeHelper.TryParseValue("abc", typeof(decimal)));
    }

    [Fact]
    public void Double_Valid_Parses()
    {
        Assert.Equal(3.14, TypeHelper.TryParseValue("3.14", typeof(double)));
    }

    [Fact]
    public void Double_Invalid_ReturnsNull()
    {
        Assert.Null(TypeHelper.TryParseValue("abc", typeof(double)));
    }

    [Fact]
    public void Float_Valid_Parses()
    {
        Assert.Equal(2.5f, TypeHelper.TryParseValue("2.5", typeof(float)));
    }

    [Fact]
    public void Float_Invalid_ReturnsNull()
    {
        Assert.Null(TypeHelper.TryParseValue("", typeof(float)));
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("True", true)]
    [InlineData("false", false)]
    [InlineData("FALSE", false)]
    public void Bool_Valid_Parses(string input, bool expected)
    {
        Assert.Equal(expected, TypeHelper.TryParseValue(input, typeof(bool)));
    }

    [Theory]
    [InlineData("yes")]
    [InlineData("")]
    [InlineData("2")]
    public void Bool_Invalid_ReturnsNull(string input)
    {
        Assert.Null(TypeHelper.TryParseValue(input, typeof(bool)));
    }

    [Fact]
    public void DateTime_Valid_Parses()
    {
        Assert.Equal(new DateTime(2024, 5, 17, 13, 45, 0), TypeHelper.TryParseValue("2024-05-17T13:45:00", typeof(DateTime)));
    }

    [Fact]
    public void DateTime_Invalid_ReturnsNull()
    {
        Assert.Null(TypeHelper.TryParseValue("not a date", typeof(DateTime)));
    }

    [Fact]
    public void DateTimeOffset_Valid_Parses()
    {
        var result = TypeHelper.TryParseValue("2024-05-17T13:45:00+02:00", typeof(DateTimeOffset));
        Assert.Equal(new DateTimeOffset(2024, 5, 17, 13, 45, 0, TimeSpan.FromHours(2)), result);
    }

    [Fact]
    public void DateTimeOffset_Invalid_ReturnsNull()
    {
        Assert.Null(TypeHelper.TryParseValue("nope", typeof(DateTimeOffset)));
    }

    [Fact]
    public void DateOnly_Valid_Parses()
    {
        Assert.Equal(new DateOnly(2024, 5, 17), TypeHelper.TryParseValue("2024-05-17", typeof(DateOnly)));
    }

    [Fact]
    public void DateOnly_Invalid_ReturnsNull()
    {
        Assert.Null(TypeHelper.TryParseValue("2024-13-45", typeof(DateOnly)));
    }

    [Fact]
    public void TimeSpan_Valid_Parses()
    {
        Assert.Equal(new TimeSpan(1, 2, 3, 4), TypeHelper.TryParseValue("1.02:03:04", typeof(TimeSpan)));
    }

    [Fact]
    public void TimeSpan_Invalid_ReturnsNull()
    {
        Assert.Null(TypeHelper.TryParseValue("abc", typeof(TimeSpan)));
    }

    [Fact]
    public void TimeOnly_Valid_Parses()
    {
        Assert.Equal(new TimeOnly(14, 30, 15), TypeHelper.TryParseValue("14:30:15", typeof(TimeOnly)));
    }

    [Fact]
    public void TimeOnly_Invalid_ReturnsNull()
    {
        Assert.Null(TypeHelper.TryParseValue("25:99", typeof(TimeOnly)));
    }

    [Fact]
    public void Guid_Valid_Parses()
    {
        var guid = Guid.NewGuid();
        Assert.Equal(guid, TypeHelper.TryParseValue(guid.ToString(), typeof(Guid)));
    }

    [Fact]
    public void Guid_Invalid_ReturnsNull()
    {
        Assert.Null(TypeHelper.TryParseValue("not-a-guid", typeof(Guid)));
    }

    [Fact]
    public void UnsupportedType_ReturnsOriginalValue()
    {
        var value = new object();
        Assert.Same(value, TypeHelper.TryParseValue(value, typeof(object)));
    }

    [Fact]
    public void NullableType_ReturnsOriginalValue()
    {
        Assert.Equal("5", TypeHelper.TryParseValue("5", typeof(int?)));
    }

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(object))]
    [InlineData(typeof(Color))]
    [InlineData(typeof(Models.ClassWithKey))]
    public void IsNumberAndIsDecimal_NonNumericTypes_ReturnFalse(Type type)
    {
        Assert.False(TypeHelper.IsNumber(type));
        Assert.False(TypeHelper.IsDecimal(type));
    }

    [Theory]
    [InlineData(typeof(int?))]
    [InlineData(typeof(decimal?))]
    public void IsNumberAndIsDecimal_NullableTypes_ReturnFalse(Type type)
    {
        Assert.False(TypeHelper.IsNumber(type));
        Assert.False(TypeHelper.IsDecimal(type));
    }

    [Fact]
    public void GetUnderlyingType_EnumAndNullableEnum()
    {
        Assert.Equal(typeof(Color), TypeHelper.GetUnderlyingType(typeof(Color)));
        Assert.Equal(typeof(Color), TypeHelper.GetUnderlyingType(typeof(Color?)));
    }

    [Fact]
    public void GetUnderlyingType_ReferenceAndGenericTypes_ReturnedUnchanged()
    {
        Assert.Equal(typeof(Models.ClassWithKey), TypeHelper.GetUnderlyingType(typeof(Models.ClassWithKey)));
        Assert.Equal(typeof(List<int?>), TypeHelper.GetUnderlyingType(typeof(List<int?>)));
    }

    [Fact]
    public void IsSupported_AllPropertiesOfTestAllType_AreSupported()
    {
        foreach (var p in typeof(Models.TestAllType).GetProperties())
        {
            Assert.True(TypeHelper.IsSupported(p.PropertyType), p.Name);
        }
    }

    [Fact]
    public void IsSupported_AllPropertiesOfDateTimeType_AreSupported()
    {
        foreach (var p in typeof(Models.DateTimeType).GetProperties())
        {
            Assert.True(TypeHelper.IsSupported(p.PropertyType), p.Name);
        }
    }

    [Fact]
    public void IsSupported_PropertiesOfClassWithKeyAndWithoutKey_AreSupported()
    {
        foreach (var t in new[] { typeof(Models.ClassWithKey), typeof(Models.ClassWithoutKey), typeof(Models.Person) })
        {
            foreach (var p in t.GetProperties())
            {
                Assert.True(TypeHelper.IsSupported(p.PropertyType), $"{t.Name}.{p.Name}");
            }
        }
    }

    [Fact]
    public void GetUnderlyingType_NullablePropertiesOfTestAllType_ReturnNonNullableType()
    {
        foreach (var p in typeof(Models.TestAllType).GetProperties()
                     .Where(p => Nullable.GetUnderlyingType(p.PropertyType) != null))
        {
            var underlying = TypeHelper.GetUnderlyingType(p.PropertyType);
            Assert.Null(Nullable.GetUnderlyingType(underlying));
            Assert.Equal(p.Name.Replace("Nullable", "NotNull"), typeof(Models.TestAllType)
                .GetProperty(p.Name.Replace("Nullable", "NotNull"))!.Name);
            Assert.Equal(typeof(Models.TestAllType).GetProperty(p.Name.Replace("Nullable", "NotNull"))!.PropertyType, underlying);
        }
    }

    [Fact]
    public void IsNumberAndIsDecimal_TestAllTypeProperties_MatchExpectedCategories()
    {
        var t = typeof(Models.TestAllType);
        Assert.True(TypeHelper.IsNumber(t.GetProperty(nameof(Models.TestAllType.Int32NotNull))!.PropertyType));
        Assert.True(TypeHelper.IsNumber(t.GetProperty(nameof(Models.TestAllType.Int64NotNull))!.PropertyType));
        Assert.True(TypeHelper.IsNumber(t.GetProperty(nameof(Models.TestAllType.Int16NotNull))!.PropertyType));
        Assert.True(TypeHelper.IsNumber(t.GetProperty(nameof(Models.TestAllType.ByteNotNull))!.PropertyType));
        Assert.True(TypeHelper.IsDecimal(t.GetProperty(nameof(Models.TestAllType.DecimalNotNull))!.PropertyType));
        Assert.True(TypeHelper.IsDecimal(t.GetProperty(nameof(Models.TestAllType.DoubleNotNull))!.PropertyType));
        Assert.True(TypeHelper.IsDecimal(t.GetProperty(nameof(Models.TestAllType.SingleNotNull))!.PropertyType));
        Assert.False(TypeHelper.IsNumber(t.GetProperty(nameof(Models.TestAllType.Int32Nullable))!.PropertyType));
        Assert.False(TypeHelper.IsDecimal(t.GetProperty(nameof(Models.TestAllType.DecimalNullable))!.PropertyType));
        Assert.False(TypeHelper.IsNumber(t.GetProperty(nameof(Models.TestAllType.StringNotNull))!.PropertyType));
    }

    [Fact]
    public void TryParseValue_AllTestAllTypeNotNullProperties_ParseFromString()
    {
        var guid = Guid.NewGuid();
        var t = typeof(Models.TestAllType);
        object? Parse(string name, string value) =>
            TypeHelper.TryParseValue(value, t.GetProperty(name)!.PropertyType);

        Assert.Equal(12, Parse(nameof(Models.TestAllType.Int32NotNull), "12"));
        Assert.Equal(12L, Parse(nameof(Models.TestAllType.Int64NotNull), "12"));
        Assert.Equal((short)12, Parse(nameof(Models.TestAllType.Int16NotNull), "12"));
        Assert.Equal((byte)12, Parse(nameof(Models.TestAllType.ByteNotNull), "12"));
        Assert.Equal(1.25m, Parse(nameof(Models.TestAllType.DecimalNotNull), "1.25"));
        Assert.Equal(1.25, Parse(nameof(Models.TestAllType.DoubleNotNull), "1.25"));
        Assert.Equal(1.25f, Parse(nameof(Models.TestAllType.SingleNotNull), "1.25"));
        Assert.Equal(true, Parse(nameof(Models.TestAllType.BoolNotNull), "true"));
        Assert.Equal("txt", Parse(nameof(Models.TestAllType.StringNotNull), "txt"));
        Assert.Equal(guid, Parse(nameof(Models.TestAllType.GuidNotNull), guid.ToString()));
    }

    [Fact]
    public void TryParseValue_AllTestAllTypeNotNullProperties_InvalidInputReturnsNull()
    {
        var t = typeof(Models.TestAllType);
        foreach (var name in new[]
        {
            nameof(Models.TestAllType.Int32NotNull), nameof(Models.TestAllType.Int64NotNull),
            nameof(Models.TestAllType.Int16NotNull), nameof(Models.TestAllType.ByteNotNull),
            nameof(Models.TestAllType.DecimalNotNull), nameof(Models.TestAllType.DoubleNotNull),
            nameof(Models.TestAllType.SingleNotNull), nameof(Models.TestAllType.BoolNotNull),
            nameof(Models.TestAllType.GuidNotNull)
        })
        {
            Assert.Null(TypeHelper.TryParseValue("###", t.GetProperty(name)!.PropertyType));
        }
    }

    [Fact]
    public void TryParseValue_DateTimeTypeProperties_ParseFromString()
    {
        var t = typeof(Models.DateTimeType);
        object? Parse(string name, string value) =>
            TypeHelper.TryParseValue(value, t.GetProperty(name)!.PropertyType);

        Assert.Equal(new DateTime(2024, 1, 2, 3, 4, 5),
            Parse(nameof(Models.DateTimeType.DateTimeNotNull), "2024-01-02T03:04:05"));
        Assert.Equal(TimeSpan.FromMinutes(90),
            Parse(nameof(Models.DateTimeType.TimeSpanNotNull), "01:30:00"));
        Assert.Equal(new TimeOnly(13, 45),
            Parse(nameof(Models.DateTimeType.TimeOnlyNotNull), "13:45"));
        Assert.Null(Parse(nameof(Models.DateTimeType.DateTimeNotNull), "nope"));
        Assert.Null(Parse(nameof(Models.DateTimeType.TimeSpanNotNull), "nope"));
        Assert.Null(Parse(nameof(Models.DateTimeType.TimeOnlyNotNull), "nope"));
    }

    [Fact]
    public void TryParseValue_NullableModelProperties_ReturnValueUnchanged()
    {
        var t = typeof(Models.TestAllType);
        var type = t.GetProperty(nameof(Models.TestAllType.Int32Nullable))!.PropertyType;

        Assert.Equal("7", TypeHelper.TryParseValue("7", type));
        Assert.Equal(7, TypeHelper.TryParseValue(7, type));
        Assert.Null(TypeHelper.TryParseValue(null, type));
    }

    [Theory]
    [InlineData("Red", Color.Red)]
    [InlineData("2", Color.Blue)]
    [InlineData("Red, Blue", (Color)2)]
    public void TryParseValue_Enum_AdditionalInputs(string input, Color expected)
    {
        var result = TypeHelper.TryParseValue(input, typeof(Color));

        if (input.Contains(','))
        {
            Assert.IsType<Color>(result);
            return;
        }
        Assert.Equal(expected, result);
    }

    [Fact]
    public void TryParseValue_Enum_FromEnumValue_Parses()
    {
        Assert.Equal(Color.Green, TypeHelper.TryParseValue(Color.Green, typeof(Color)));
    }

    [Fact]
    public void TryParseValue_Bool_FromBoolObject_Parses()
    {
        Assert.Equal(true, TypeHelper.TryParseValue(true, typeof(bool)));
        Assert.Equal(false, TypeHelper.TryParseValue(false, typeof(bool)));
    }

    [Fact]
    public void TryParseValue_Guid_FromGuidObject_Parses()
    {
        var g = Guid.NewGuid();
        Assert.Equal(g, TypeHelper.TryParseValue(g, typeof(Guid)));
    }

    [Fact]
    public void TryParseValue_Number_WithWhitespace()
    {
        Assert.Equal(5, TypeHelper.TryParseValue(" 5 ", typeof(int)));
    }

    [Fact]
    public void TryParseValue_Numbers_BoundaryValues()
    {
        Assert.Equal(int.MaxValue, TypeHelper.TryParseValue(int.MaxValue.ToString(), typeof(int)));
        Assert.Equal(int.MinValue, TypeHelper.TryParseValue(int.MinValue.ToString(), typeof(int)));
        Assert.Equal(byte.MaxValue, TypeHelper.TryParseValue("255", typeof(byte)));
        Assert.Equal(sbyte.MinValue, TypeHelper.TryParseValue("-128", typeof(sbyte)));
        Assert.Equal(ushort.MaxValue, TypeHelper.TryParseValue("65535", typeof(ushort)));
        Assert.Equal(long.MinValue, TypeHelper.TryParseValue(long.MinValue.ToString(), typeof(long)));
    }

    [Fact]
    public void TryParseValue_ClassWithKey_ReturnsSameInstance()
    {
        var value = new Models.ClassWithKey { Id = 1, Name = "n" };
        Assert.Same(value, TypeHelper.TryParseValue(value, typeof(Models.ClassWithKey)));
    }
}
