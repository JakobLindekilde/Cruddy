using System.Globalization;
using Cruddy.Helpers;

namespace Cruddy.Tests.Helpers
{
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
    }
}
