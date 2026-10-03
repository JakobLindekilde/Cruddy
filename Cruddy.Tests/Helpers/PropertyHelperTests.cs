using Cruddy.Helpers;
using Cruddy.Tests.Models;

namespace Cruddy.Tests.Helpers;

public class PropertyHelperTests
{
    [Fact]
    public void IsNullable_ReturnsExpected()
    {
        var all = new TestAllType() { StringNotNull = "Test" };
        var t = all.GetType();

        Assert.False(PropertyHelper.IsNullable(t.GetProperty(nameof(all.Int32NotNull))!));
        Assert.True(PropertyHelper.IsNullable(t.GetProperty(nameof(all.Int32Nullable))!));
        Assert.False(PropertyHelper.IsNullable(t.GetProperty(nameof(all.Int64NotNull))!));
        Assert.True(PropertyHelper.IsNullable(t.GetProperty(nameof(all.Int64Nullable))!));
        Assert.False(PropertyHelper.IsNullable(t.GetProperty(nameof(all.Int16NotNull))!));
        Assert.True(PropertyHelper.IsNullable(t.GetProperty(nameof(all.Int16Nullable))!));
        Assert.False(PropertyHelper.IsNullable(t.GetProperty(nameof(all.ByteNotNull))!));
        Assert.True(PropertyHelper.IsNullable(t.GetProperty(nameof(all.ByteNullable))!));

        Assert.False(PropertyHelper.IsNullable(t.GetProperty(nameof(all.DecimalNotNull))!));
        Assert.True(PropertyHelper.IsNullable(t.GetProperty(nameof(all.DecimalNullable))!));
        Assert.False(PropertyHelper.IsNullable(t.GetProperty(nameof(all.DoubleNotNull))!));
        Assert.True(PropertyHelper.IsNullable(t.GetProperty(nameof(all.DoubleNullable))!));
        Assert.False(PropertyHelper.IsNullable(t.GetProperty(nameof(all.SingleNotNull))!));
        Assert.True(PropertyHelper.IsNullable(t.GetProperty(nameof(all.SingleNullable))!));

        Assert.False(PropertyHelper.IsNullable(t.GetProperty(nameof(all.BoolNotNull))!));
        Assert.True(PropertyHelper.IsNullable(t.GetProperty(nameof(all.BoolNullable))!));

        Assert.False(PropertyHelper.IsNullable(t.GetProperty(nameof(all.StringNotNull))!));
        Assert.True(PropertyHelper.IsNullable(t.GetProperty(nameof(all.StringNullable))!));

        Assert.False(PropertyHelper.IsNullable(t.GetProperty(nameof(all.GuidNotNull))!));
        Assert.True(PropertyHelper.IsNullable(t.GetProperty(nameof(all.GuidNullable))!));
    }

    [Fact]
    public void IsNullable_ReturnsExpectedWithDateTimeType()
    {
        var all = new DateTimeType();
        var t = all.GetType();

        Assert.False(PropertyHelper.IsNullable(t.GetProperty(nameof(all.DateTimeNotNull))!));
        Assert.True(PropertyHelper.IsNullable(t.GetProperty(nameof(all.DateTimeNullable))!));
        Assert.False(PropertyHelper.IsNullable(t.GetProperty(nameof(all.TimeSpanNotNull))!));
        Assert.True(PropertyHelper.IsNullable(t.GetProperty(nameof(all.TimeSpanNullable))!));
        Assert.False(PropertyHelper.IsNullable(t.GetProperty(nameof(all.TimeOnlyNotNull))!));
        Assert.True(PropertyHelper.IsNullable(t.GetProperty(nameof(all.TimeOnlyNullable))!));
    }

    [Fact]
    public void GetPublicProperties_IncludesPublicInstancePropertiesAndInheritedOnes()
    {
        var withoutKeyProps = PropertyHelper.GetColumnProperties(typeof(ClassWithoutKey));
        // Public instance properties declared on ClassWithoutKey (Id, Firstname, Lastname, Age, Hight)
        Assert.Equal(5, withoutKeyProps.Length);
        Assert.Contains(withoutKeyProps, p => p.Name == "Id");
        Assert.Contains(withoutKeyProps, p => p.Name == "Firstname");
        Assert.Contains(withoutKeyProps, p => p.Name == "Lastname");
        Assert.Contains(withoutKeyProps, p => p.Name == "Age");
        Assert.Contains(withoutKeyProps, p => p.Name == "Hight");

        var personProps = PropertyHelper.GetColumnProperties(typeof(Person));
        // Person inherits ClassWithKey (Id, Name, Description) and adds Email
        Assert.Contains(personProps, p => p.Name == "Id");
        Assert.Contains(personProps, p => p.Name == "Name");
        Assert.Contains(personProps, p => p.Name == "Description");
        Assert.Contains(personProps, p => p.Name == "Email");
        Assert.Equal(4, personProps.Length);
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
