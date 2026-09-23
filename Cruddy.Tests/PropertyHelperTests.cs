using CruddyDemo.Helpers;

namespace Cruddy.Tests
{
    public class PropertyHelperTests
    {
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
        [InlineData(typeof(TimeSpan), false)]
        [InlineData(typeof(DateTimeOffset), false)]
        [InlineData(typeof(Guid), false)]
        public void IsDecimal_ReturnsTrueForDecimalTypes(Type type, bool expected)
        {
            Assert.Equal(expected, PropertyHelper.IsDecimal(type));
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
            Assert.Equal(expected, PropertyHelper.IsNumber(type));
        }

        [Fact]
        public void GetReadProperties_ReturnsOnlyReadableProperties()
        {
            var properties = PropertyHelper.GetReadProperties(typeof(ClassWithoutKey));

            Assert.Equal(5, properties.Length);
            Assert.Contains(properties, p => p.Name == "Id");
            Assert.Contains(properties, p => p.Name == "Firstname");
            Assert.Contains(properties, p => p.Name == "Lastname");
            Assert.Contains(properties, p => p.Name == "Age");
            Assert.Contains(properties, p => p.Name == "Hight");
        }

        [Fact]
        public void GetValue_ReturnsEmpty_WhenPropNameNullOrEmpty()
        {
            var sample = new ClassWithoutKey { Firstname = "John", Lastname = "Doe" };
            Assert.Equal(string.Empty, PropertyHelper.GetValue(sample, null!));
            Assert.Equal(string.Empty, PropertyHelper.GetValue(sample, string.Empty));
        }

        [Theory]
        [InlineData("Firstname", "John")]
        [InlineData("firstName", "John")]
        [InlineData("FirstName", "John")]
        [InlineData("Lastname", "Doe")]
        [InlineData("lastName", "Doe")]
        [InlineData("LastName", "Doe")]
        public void GetValue_ReturnsValue_CaseInsensitive(string propName, string expected)
        {
            var sample = new ClassWithoutKey { Firstname = "John", Lastname = "Doe" };
            Assert.Equal(expected, PropertyHelper.GetValue(sample, propName));
        }

        [Theory]
        [InlineData("Id", "Id/Pkey")]
        [InlineData("Name", "Name")]
        [InlineData("Description", "Info")]
        public void GetDisplayName_ReturnsDisplayNameAttributeOrFallback(string typeName, string expected)
        {
            var prop = typeof(ClassWithKey).GetProperty(typeName)!;
            var displayName = PropertyHelper.GetDisplayName(prop);
            Assert.Equal(expected, displayName);
        }

        [Fact]
        public void GetDisplayFormat_ReturnsDisplayFormatAttributeOrNull()
        {
            var prop = typeof(ClassWithoutKey).GetProperty("Id")!;
            var displayFormat = PropertyHelper.GetDisplayFormat(prop);
            Assert.Null(displayFormat);

            prop = typeof(ClassWithoutKey).GetProperty("Age")!;
            displayFormat = PropertyHelper.GetDisplayFormat(prop);
            Assert.Equal("{0:N0}", displayFormat);
        }

        [Fact]
        public void GetKey()
        {
            var hasKeyWithKey = PropertyHelper.GetKey(typeof(ClassWithKey));
            var hasKeyWithoutKey = PropertyHelper.GetKey(typeof(ClassWithoutKey));

            // Assert
            Assert.NotNull(hasKeyWithKey);
            Assert.Null(hasKeyWithoutKey);
        }

        [Fact]
        public void PropExists()
        {
            var hasIdProperty = PropertyHelper.PropExists(typeof(ClassWithoutKey), "Id");
            var hasFirstnameProperty = PropertyHelper.PropExists(typeof(ClassWithoutKey), "Firstname");
            var hasAgeProperty = PropertyHelper.PropExists(typeof(ClassWithoutKey), "Age");
            var hasNonExistentProperty = PropertyHelper.PropExists(typeof(ClassWithoutKey), "NonExistent");
            
            // Assert
            Assert.True(hasIdProperty);
            Assert.True(hasFirstnameProperty);
            Assert.True(hasAgeProperty);
            Assert.False(hasNonExistentProperty);       
        }
    }
}
