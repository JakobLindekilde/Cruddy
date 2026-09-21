using CruddyDemo.Helpers;

namespace Cruddy.Tests
{
    public class PropertyHelperTests
    {
        [Theory]
        [InlineData("Decimal",true)]
        [InlineData("Double",true)]
        [InlineData("Single",true)]
        [InlineData("decimal", false)]
        [InlineData("double", false)]
        [InlineData("single", false)]
        [InlineData("Int32", false)]
        [InlineData("Bool", false)]
        [InlineData("DateTime",false)]
        [InlineData("String", false)]
        [InlineData("Number", false)]
        [InlineData("5.67", false)]
        public void IsDecimal_ReturnsTrueForDecimalTypes(string typeName, bool expected)
        {
            Assert.Equal(expected, PropertyHelper.IsDecimal(typeName));
        }

        [Theory]
        [InlineData("Int32", true)]
        [InlineData("Int64", true)]
        [InlineData("Int16", true)]
        [InlineData("UInt32", true)]
        [InlineData("UInt64", true)]
        [InlineData("UInt16", true)]
        [InlineData("int32", false)]
        [InlineData("int64", false)]
        [InlineData("int16", false)]
        [InlineData("uint32", false)]
        [InlineData("uint64", false)]
        [InlineData("uint16", false)]
        [InlineData("int", false)]
        [InlineData("long", false)]
        [InlineData("byte", false)]
        [InlineData("sbyte", false)]
        [InlineData("decimal", false)]
        [InlineData("double", false)]
        [InlineData("single", false)]
        [InlineData("bool", false)]
        [InlineData("datetime", false)]
        [InlineData("string", false)]
        [InlineData("number", false)]
        [InlineData("5", false)]
        public void IsNumber_ReturnsTrueForNumberTypes(string typeName, bool expected)
        {
            Assert.Equal(expected, PropertyHelper.IsNumber(typeName));
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
