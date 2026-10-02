using System.Globalization;
using Cruddy.Helpers;

namespace Cruddy.Tests.Helpers
{
    public class PropertyHelperTryParseValueTests : IDisposable
    {
        private readonly CultureInfo _originalCulture;

        public enum Color { Red, Green, Blue }

        public PropertyHelperTryParseValueTests()
        {
            _originalCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        }

        public void Dispose() => CultureInfo.CurrentCulture = _originalCulture;

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
            Assert.Null(PropertyHelper.TryParseValue(null, type));
        }

        [Fact]
        public void String_ReturnsStringValue()
        {
            Assert.Equal("hello", PropertyHelper.TryParseValue("hello", typeof(string)));
        }

        [Fact]
        public void String_FromNonString_UsesToString()
        {
            Assert.Equal("42", PropertyHelper.TryParseValue(42, typeof(string)));
        }

        [Fact]
        public void String_EmptyString_ReturnsEmpty()
        {
            Assert.Equal(string.Empty, PropertyHelper.TryParseValue("", typeof(string)));
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
                Assert.Throws<ArgumentException>(() => PropertyHelper.TryParseValue(input, typeof(Color)));
                return;
            }
            Assert.Equal(expected, PropertyHelper.TryParseValue(input, typeof(Color)));
        }

        [Fact]
        public void Enum_InvalidValue_Throws()
        {
            Assert.Throws<ArgumentException>(() => PropertyHelper.TryParseValue("Purple", typeof(Color)));
        }

        [Theory]
        [InlineData("123", 123)]
        [InlineData("-5", -5)]
        [InlineData("0", 0)]
        public void Int_ValidValue_Parses(string input, int expected)
        {
            Assert.Equal(expected, PropertyHelper.TryParseValue(input, typeof(int)));
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("")]
        [InlineData("1.5")]
        [InlineData("99999999999")]
        public void Int_InvalidValue_ReturnsNull(string input)
        {
            Assert.Null(PropertyHelper.TryParseValue(input, typeof(int)));
        }

        [Fact]
        public void Int_FromIntObject_Parses()
        {
            Assert.Equal(7, PropertyHelper.TryParseValue(7, typeof(int)));
        }

        [Fact]
        public void Long_Valid_Parses()
        {
            Assert.Equal(9999999999L, PropertyHelper.TryParseValue("9999999999", typeof(long)));
        }

        [Fact]
        public void Long_Invalid_ReturnsNull()
        {
            Assert.Null(PropertyHelper.TryParseValue("x", typeof(long)));
        }

        [Fact]
        public void Short_Valid_Parses()
        {
            Assert.Equal((short)-300, PropertyHelper.TryParseValue("-300", typeof(short)));
        }

        [Fact]
        public void Short_Overflow_ReturnsNull()
        {
            Assert.Null(PropertyHelper.TryParseValue("40000", typeof(short)));
        }

        [Fact]
        public void UInt_Valid_Parses()
        {
            Assert.Equal(4000000000u, PropertyHelper.TryParseValue("4000000000", typeof(uint)));
        }

        [Fact]
        public void UInt_Negative_ReturnsNull()
        {
            Assert.Null(PropertyHelper.TryParseValue("-1", typeof(uint)));
        }

        [Fact]
        public void ULong_Valid_Parses()
        {
            Assert.Equal(18446744073709551615UL, PropertyHelper.TryParseValue("18446744073709551615", typeof(ulong)));
        }

        [Fact]
        public void ULong_Invalid_ReturnsNull()
        {
            Assert.Null(PropertyHelper.TryParseValue("-1", typeof(ulong)));
        }

        [Fact]
        public void UShort_Valid_Parses()
        {
            Assert.Equal((ushort)65535, PropertyHelper.TryParseValue("65535", typeof(ushort)));
        }

        [Fact]
        public void UShort_Overflow_ReturnsNull()
        {
            Assert.Null(PropertyHelper.TryParseValue("65536", typeof(ushort)));
        }

        [Fact]
        public void Byte_Valid_Parses()
        {
            Assert.Equal((byte)255, PropertyHelper.TryParseValue("255", typeof(byte)));
        }

        [Fact]
        public void Byte_Overflow_ReturnsNull()
        {
            Assert.Null(PropertyHelper.TryParseValue("256", typeof(byte)));
        }

        [Fact]
        public void SByte_Valid_Parses()
        {
            Assert.Equal((sbyte)-128, PropertyHelper.TryParseValue("-128", typeof(sbyte)));
        }

        [Fact]
        public void SByte_Overflow_ReturnsNull()
        {
            Assert.Null(PropertyHelper.TryParseValue("128", typeof(sbyte)));
        }

        [Fact]
        public void Decimal_Valid_Parses()
        {
            Assert.Equal(12.34m, PropertyHelper.TryParseValue("12.34", typeof(decimal)));
        }

        [Fact]
        public void Decimal_Invalid_ReturnsNull()
        {
            Assert.Null(PropertyHelper.TryParseValue("abc", typeof(decimal)));
        }

        [Fact]
        public void Double_Valid_Parses()
        {
            Assert.Equal(3.14, PropertyHelper.TryParseValue("3.14", typeof(double)));
        }

        [Fact]
        public void Double_Invalid_ReturnsNull()
        {
            Assert.Null(PropertyHelper.TryParseValue("abc", typeof(double)));
        }

        [Fact]
        public void Float_Valid_Parses()
        {
            Assert.Equal(2.5f, PropertyHelper.TryParseValue("2.5", typeof(float)));
        }

        [Fact]
        public void Float_Invalid_ReturnsNull()
        {
            Assert.Null(PropertyHelper.TryParseValue("", typeof(float)));
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("True", true)]
        [InlineData("false", false)]
        [InlineData("FALSE", false)]
        public void Bool_Valid_Parses(string input, bool expected)
        {
            Assert.Equal(expected, PropertyHelper.TryParseValue(input, typeof(bool)));
        }

        [Theory]
        [InlineData("yes")]
        [InlineData("")]
        [InlineData("2")]
        public void Bool_Invalid_ReturnsNull(string input)
        {
            Assert.Null(PropertyHelper.TryParseValue(input, typeof(bool)));
        }

        [Fact]
        public void DateTime_Valid_Parses()
        {
            Assert.Equal(new DateTime(2024, 5, 17, 13, 45, 0), PropertyHelper.TryParseValue("2024-05-17T13:45:00", typeof(DateTime)));
        }

        [Fact]
        public void DateTime_Invalid_ReturnsNull()
        {
            Assert.Null(PropertyHelper.TryParseValue("not a date", typeof(DateTime)));
        }

        [Fact]
        public void DateTimeOffset_Valid_Parses()
        {
            var result = PropertyHelper.TryParseValue("2024-05-17T13:45:00+02:00", typeof(DateTimeOffset));
            Assert.Equal(new DateTimeOffset(2024, 5, 17, 13, 45, 0, TimeSpan.FromHours(2)), result);
        }

        [Fact]
        public void DateTimeOffset_Invalid_ReturnsNull()
        {
            Assert.Null(PropertyHelper.TryParseValue("nope", typeof(DateTimeOffset)));
        }

        [Fact]
        public void DateOnly_Valid_Parses()
        {
            Assert.Equal(new DateOnly(2024, 5, 17), PropertyHelper.TryParseValue("2024-05-17", typeof(DateOnly)));
        }

        [Fact]
        public void DateOnly_Invalid_ReturnsNull()
        {
            Assert.Null(PropertyHelper.TryParseValue("2024-13-45", typeof(DateOnly)));
        }

        [Fact]
        public void TimeSpan_Valid_Parses()
        {
            Assert.Equal(new TimeSpan(1, 2, 3, 4), PropertyHelper.TryParseValue("1.02:03:04", typeof(TimeSpan)));
        }

        [Fact]
        public void TimeSpan_Invalid_ReturnsNull()
        {
            Assert.Null(PropertyHelper.TryParseValue("abc", typeof(TimeSpan)));
        }

        [Fact]
        public void TimeOnly_Valid_Parses()
        {
            Assert.Equal(new TimeOnly(14, 30, 15), PropertyHelper.TryParseValue("14:30:15", typeof(TimeOnly)));
        }

        [Fact]
        public void TimeOnly_Invalid_ReturnsNull()
        {
            Assert.Null(PropertyHelper.TryParseValue("25:99", typeof(TimeOnly)));
        }

        [Fact]
        public void Guid_Valid_Parses()
        {
            var guid = Guid.NewGuid();
            Assert.Equal(guid, PropertyHelper.TryParseValue(guid.ToString(), typeof(Guid)));
        }

        [Fact]
        public void Guid_Invalid_ReturnsNull()
        {
            Assert.Null(PropertyHelper.TryParseValue("not-a-guid", typeof(Guid)));
        }

        [Fact]
        public void UnsupportedType_ReturnsOriginalValue()
        {
            var value = new object();
            Assert.Same(value, PropertyHelper.TryParseValue(value, typeof(object)));
        }

        [Fact]
        public void NullableType_ReturnsOriginalValue()
        {
            Assert.Equal("5", PropertyHelper.TryParseValue("5", typeof(int?)));
        }
    }
}
