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

    private class UpperIdModel { public int ID { get; set; } }
    private class TypeIdModel { public int TypeIdModelId { get; set; } }
    private class TypeUpperIdModel { public int TypeUpperIdModelID { get; set; } }
    private class NoKeyModel { public string? Name { get; set; } }
    private class WithClassProp { public int Value { get; set; } public ClassWithKey? Child { get; set; } public string? Text { get; set; } }
    private class WriteOnlyModel
    {
        public int Readable { get; set; }
        public int WriteOnly { set { _ = value; } }
        private int Hidden { get; set; }
        public static int StaticProp { get; set; }
    }
    private class DisplayNameModel
    {
        [System.ComponentModel.DisplayName("Dn")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Da")]
        public int Both { get; set; }
        [System.ComponentModel.DisplayName("OnlyDn")]
        public int OnlyDisplayName { get; set; }
        [System.ComponentModel.DataAnnotations.Display(Description = "x")]
        public int DisplayWithoutName { get; set; }
        public int Plain { get; set; }
    }

    #region IsNullable

    [Fact]
    public void IsNullable_Null_ReturnsFalse()
    {
        Assert.False(PropertyHelper.IsNullable(null!));
    }

    [Fact]
    public void IsNullable_ClassWithKey_ReferenceTypes()
    {
        var t = typeof(ClassWithKey);
        Assert.False(PropertyHelper.IsNullable(t.GetProperty(nameof(ClassWithKey.Id))!));
        Assert.False(PropertyHelper.IsNullable(t.GetProperty(nameof(ClassWithKey.Name))!));
        Assert.True(PropertyHelper.IsNullable(t.GetProperty(nameof(ClassWithKey.Description))!));
    }

    [Fact]
    public void IsNullable_ClassWithoutKey_ReturnsExpected()
    {
        var t = typeof(ClassWithoutKey);
        Assert.False(PropertyHelper.IsNullable(t.GetProperty(nameof(ClassWithoutKey.Firstname))!));
        Assert.True(PropertyHelper.IsNullable(t.GetProperty(nameof(ClassWithoutKey.Lastname))!));
        Assert.False(PropertyHelper.IsNullable(t.GetProperty(nameof(ClassWithoutKey.Age))!));
        Assert.True(PropertyHelper.IsNullable(t.GetProperty(nameof(ClassWithoutKey.Hight))!));
    }

    [Fact]
    public void IsNullable_Person_InheritedAndOwnProperties()
    {
        var t = typeof(Person);
        Assert.False(PropertyHelper.IsNullable(t.GetProperty(nameof(Person.Name))!));
        Assert.True(PropertyHelper.IsNullable(t.GetProperty(nameof(Person.Description))!));
        Assert.True(PropertyHelper.IsNullable(t.GetProperty(nameof(Person.Email))!));
    }

    #endregion

    #region GetColumnProperties

    [Fact]
    public void GetColumnProperties_ClassWithKey_ReturnsAllPublicInstanceProperties()
    {
        var props = PropertyHelper.GetColumnProperties(typeof(ClassWithKey));

        Assert.Equal(["Id", "Name", "Description"], props.Select(p => p.Name).ToArray());
    }

    [Fact]
    public void GetColumnProperties_ExcludesPrivateAndProtectedProperties()
    {
        var props = PropertyHelper.GetColumnProperties(typeof(ClassWithoutKey));

        Assert.DoesNotContain(props, p => p.Name == "PrivateProperty");
        Assert.DoesNotContain(props, p => p.Name == "ProtectedProperty");
    }

    [Fact]
    public void GetColumnProperties_TestAllType_ReturnsAllProperties()
    {
        var props = PropertyHelper.GetColumnProperties(typeof(TestAllType));

        Assert.Equal(typeof(TestAllType).GetProperties().Length, props.Length);
        Assert.Contains(props, p => p.Name == nameof(TestAllType.StringNullable));
        Assert.Contains(props, p => p.Name == nameof(TestAllType.GuidNullable));
    }

    [Fact]
    public void GetColumnProperties_DateTimeType_ReturnsAllProperties()
    {
        var props = PropertyHelper.GetColumnProperties(typeof(DateTimeType));

        Assert.Equal(7, props.Length);
    }

    [Fact]
    public void GetColumnProperties_ExcludesClassTypedProperties_ButKeepsString()
    {
        var props = PropertyHelper.GetColumnProperties(typeof(WithClassProp));

        Assert.Contains(props, p => p.Name == "Value");
        Assert.Contains(props, p => p.Name == "Text");
        Assert.DoesNotContain(props, p => p.Name == "Child");
    }

    #endregion

    #region GetReadProperties

    [Fact]
    public void GetReadProperties_ExcludesWriteOnlyPrivateAndStatic()
    {
        var props = PropertyHelper.GetReadProperties(typeof(WriteOnlyModel));

        var prop = Assert.Single(props);
        Assert.Equal("Readable", prop.Name);
    }

    [Fact]
    public void GetReadProperties_Person_IncludesInheritedProperties()
    {
        var props = PropertyHelper.GetReadProperties(typeof(Person));

        Assert.Equal(4, props.Length);
        Assert.Contains(props, p => p.Name == "Email");
        Assert.Contains(props, p => p.Name == "Name");
    }

    [Fact]
    public void GetReadProperties_TestAllType_ReturnsAll()
    {
        Assert.Equal(typeof(TestAllType).GetProperties().Length,
            PropertyHelper.GetReadProperties(typeof(TestAllType)).Length);
    }

    #endregion

    #region GetKey / CalcKey

    [Fact]
    public void GetKey_ClassWithKey_ReturnsId()
    {
        Assert.Equal("Id", PropertyHelper.GetKey(typeof(ClassWithKey)));
    }

    [Fact]
    public void GetKey_TestAllTypeAndDateTimeType_ReturnId()
    {
        Assert.Equal("Id", PropertyHelper.GetKey(typeof(TestAllType)));
        Assert.Equal("Id", PropertyHelper.GetKey(typeof(DateTimeType)));
    }

    [Fact]
    public void GetKey_Person_ReturnsInheritedKey()
    {
        Assert.Equal("Id", PropertyHelper.GetKey(typeof(Person)));
    }

    [Fact]
    public void GetKey_NullType_ReturnsNull()
    {
        Assert.Null(PropertyHelper.GetKey(null!));
    }

    [Fact]
    public void CalcKey_TypeWithKeyAttribute_ReturnsKeyAttributeName()
    {
        Assert.Equal("Id", PropertyHelper.CalcKey(typeof(ClassWithKey)));
        Assert.Equal("Id", PropertyHelper.CalcKey(typeof(TestAllType)));
        Assert.Equal("Id", PropertyHelper.CalcKey(typeof(Person)));
    }

    [Fact]
    public void CalcKey_NoKeyAttribute_FallsBackToIdProperty()
    {
        Assert.Equal("Id", PropertyHelper.CalcKey(typeof(ClassWithoutKey)));
    }

    [Fact]
    public void CalcKey_UpperCaseId_ReturnsId()
    {
        // PropExists is case-insensitive, so "Id" matches "ID" first
        Assert.Equal("Id", PropertyHelper.CalcKey(typeof(UpperIdModel)));
    }

    [Fact]
    public void CalcKey_TypeNameId_ReturnsTypeNameId()
    {
        Assert.Equal("TypeIdModelId", PropertyHelper.CalcKey(typeof(TypeIdModel)));
    }

    [Fact]
    public void CalcKey_TypeNameUpperId_ReturnsTypeNameId()
    {
        Assert.Equal("TypeUpperIdModelId", PropertyHelper.CalcKey(typeof(TypeUpperIdModel)));
    }

    [Fact]
    public void CalcKey_NoKeyCandidates_DefaultsToId()
    {
        Assert.Equal("Id", PropertyHelper.CalcKey(typeof(NoKeyModel)));
    }

    #endregion

    #region GetProperty

    [Theory]
    [InlineData("Firstname")]
    [InlineData("firstname")]
    [InlineData("FIRSTNAME")]
    public void GetProperty_CaseInsensitive_ReturnsProperty(string name)
    {
        var prop = PropertyHelper.GetProperty(typeof(ClassWithoutKey), name);

        Assert.NotNull(prop);
        Assert.Equal("Firstname", prop.Name);
    }

    [Fact]
    public void GetProperty_NonExistent_ReturnsNull()
    {
        Assert.Null(PropertyHelper.GetProperty(typeof(ClassWithoutKey), "Nope"));
    }

    [Theory]
    [InlineData("PrivateProperty")]
    [InlineData("ProtectedProperty")]
    public void GetProperty_NonPublic_ReturnsNull(string name)
    {
        Assert.Null(PropertyHelper.GetProperty(typeof(ClassWithoutKey), name));
    }

    [Fact]
    public void GetProperty_InheritedProperty_ReturnsProperty()
    {
        var prop = PropertyHelper.GetProperty(typeof(Person), "name");

        Assert.NotNull(prop);
        Assert.Equal(typeof(string), prop.PropertyType);
    }

    [Fact]
    public void GetProperty_ReturnsCorrectPropertyType()
    {
        Assert.Equal(typeof(int?), PropertyHelper.GetProperty(typeof(TestAllType), "int32nullable")!.PropertyType);
        Assert.Equal(typeof(Guid), PropertyHelper.GetProperty(typeof(TestAllType), "GuidNotNull")!.PropertyType);
    }

    #endregion

    #region PropExists

    [Theory]
    [InlineData("id", true)]
    [InlineData("ID", true)]
    [InlineData("firstname", true)]
    [InlineData("Hight", true)]
    [InlineData("PrivateProperty", false)]
    [InlineData("ProtectedProperty", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("Unknown", false)]
    public void PropExists_ReturnsExpected(string? name, bool expected)
    {
        Assert.Equal(expected, PropertyHelper.PropExists(typeof(ClassWithoutKey), name!));
    }

    [Fact]
    public void PropExists_InheritedProperty_ReturnsTrue()
    {
        Assert.True(PropertyHelper.PropExists(typeof(Person), "Email"));
        Assert.True(PropertyHelper.PropExists(typeof(Person), "Description"));
    }

    #endregion

    #region GetValue

    [Fact]
    public void GetValue_NullPropertyValue_ReturnsEmpty()
    {
        var sample = new ClassWithoutKey { Firstname = "J", Lastname = null, Hight = null };

        Assert.Equal(string.Empty, PropertyHelper.GetValue(sample, "Lastname"));
        Assert.Equal(string.Empty, PropertyHelper.GetValue(sample, "Hight"));
    }

    [Fact]
    public void GetValue_NonExistentProperty_ReturnsEmpty()
    {
        var sample = new ClassWithoutKey { Firstname = "J" };

        Assert.Equal(string.Empty, PropertyHelper.GetValue(sample, "Nope"));
    }

    [Fact]
    public void GetValue_NonPublicProperty_ReturnsEmpty()
    {
        var sample = new ClassWithoutKey { Firstname = "J" };

        Assert.Equal(string.Empty, PropertyHelper.GetValue(sample, "PrivateProperty"));
        Assert.Equal(string.Empty, PropertyHelper.GetValue(sample, "ProtectedProperty"));
    }

    [Fact]
    public void GetValue_NumericAndBoolAndGuid_ReturnsToString()
    {
        var guid = Guid.NewGuid();
        var sample = new TestAllType
        {
            StringNotNull = "s",
            Int32NotNull = 42,
            BoolNotNull = true,
            GuidNotNull = guid
        };

        Assert.Equal("42", PropertyHelper.GetValue(sample, "Int32NotNull"));
        Assert.Equal(true.ToString(), PropertyHelper.GetValue(sample, "BoolNotNull"));
        Assert.Equal(guid.ToString(), PropertyHelper.GetValue(sample, "GuidNotNull"));
        Assert.Equal("s", PropertyHelper.GetValue(sample, "StringNotNull"));
    }

    [Fact]
    public void GetValue_InheritedProperty_ReturnsValue()
    {
        var person = new Person { Id = 3, Name = "N", Email = "e@x.dk" };

        Assert.Equal("e@x.dk", PropertyHelper.GetValue(person, "Email"));
        Assert.Equal("N", PropertyHelper.GetValue(person, "Name"));
        Assert.Equal("3", PropertyHelper.GetValue(person, "Id"));
    }

    #endregion

    #region GetDisplayName

    [Fact]
    public void GetDisplayName_DisplayNameAttributeTakesPrecedenceOverDisplay()
    {
        var prop = typeof(DisplayNameModel).GetProperty(nameof(DisplayNameModel.Both))!;

        Assert.Equal("Dn", PropertyHelper.GetDisplayName(prop));
    }

    [Fact]
    public void GetDisplayName_OnlyDisplayNameAttribute_ReturnsIt()
    {
        var prop = typeof(DisplayNameModel).GetProperty(nameof(DisplayNameModel.OnlyDisplayName))!;

        Assert.Equal("OnlyDn", PropertyHelper.GetDisplayName(prop));
    }

    [Fact]
    public void GetDisplayName_DisplayAttributeWithoutName_ReturnsPropertyName()
    {
        var prop = typeof(DisplayNameModel).GetProperty(nameof(DisplayNameModel.DisplayWithoutName))!;

        Assert.Equal("DisplayWithoutName", PropertyHelper.GetDisplayName(prop));
    }

    [Fact]
    public void GetDisplayName_NoAttributes_ReturnsPropertyName()
    {
        var prop = typeof(DisplayNameModel).GetProperty(nameof(DisplayNameModel.Plain))!;

        Assert.Equal("Plain", PropertyHelper.GetDisplayName(prop));
    }

    [Fact]
    public void GetDisplayName_InheritedProperty_UsesAttributeFromBase()
    {
        var prop = typeof(Person).GetProperty(nameof(Person.Id))!;

        Assert.Equal("Id/Pkey", PropertyHelper.GetDisplayName(prop));
    }

    #endregion

    #region GetDisplayFormat

    [Fact]
    public void GetDisplayFormat_PropertiesWithoutFormat_ReturnNull()
    {
        Assert.Null(PropertyHelper.GetDisplayFormat(typeof(ClassWithKey).GetProperty("Name")!));
        Assert.Null(PropertyHelper.GetDisplayFormat(typeof(ClassWithoutKey).GetProperty("Hight")!));
        Assert.Null(PropertyHelper.GetDisplayFormat(typeof(TestAllType).GetProperty("DecimalNotNull")!));
    }

    #endregion

    #region GetFormattedValue

    private static string WithInvariantCulture(Func<string> action)
    {
        var original = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            return action();
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void GetFormattedValue_WithDisplayFormat_AppliesFormat()
    {
        var item = new ClassWithoutKey { Firstname = "A", Age = 1234567 };
        var prop = typeof(ClassWithoutKey).GetProperty(nameof(ClassWithoutKey.Age))!;

        var result = WithInvariantCulture(() => PropertyHelper.GetFormattedValue(prop, item));

        Assert.Equal("1,234,567", result);
    }

    [Fact]
    public void GetFormattedValue_WithoutDisplayFormat_UsesToString()
    {
        var item = new ClassWithoutKey { Id = 12, Firstname = "A" };

        Assert.Equal("12", PropertyHelper.GetFormattedValue(typeof(ClassWithoutKey).GetProperty("Id")!, item));
        Assert.Equal("A", PropertyHelper.GetFormattedValue(typeof(ClassWithoutKey).GetProperty("Firstname")!, item));
    }

    [Fact]
    public void GetFormattedValue_NullValue_ReturnsEmpty()
    {
        var item = new ClassWithoutKey { Firstname = "A", Lastname = null, Hight = null };

        Assert.Equal(string.Empty, PropertyHelper.GetFormattedValue(typeof(ClassWithoutKey).GetProperty("Lastname")!, item));
        Assert.Equal(string.Empty, PropertyHelper.GetFormattedValue(typeof(ClassWithoutKey).GetProperty("Hight")!, item));
    }

    [Fact]
    public void GetFormattedValue_NullableWithValue_ReturnsToString()
    {
        var item = new ClassWithoutKey { Firstname = "A", Hight = 180 };

        Assert.Equal("180", PropertyHelper.GetFormattedValue(typeof(ClassWithoutKey).GetProperty("Hight")!, item));
    }

    [Fact]
    public void GetFormattedValue_ZeroWithFormat_AppliesFormat()
    {
        var item = new ClassWithoutKey { Firstname = "A", Age = 0 };
        var prop = typeof(ClassWithoutKey).GetProperty(nameof(ClassWithoutKey.Age))!;

        Assert.Equal("0", WithInvariantCulture(() => PropertyHelper.GetFormattedValue(prop, item)));
    }

    [Fact]
    public void GetFormattedValue_AllTypes_NonNullValuesFormatted()
    {
        var guid = Guid.NewGuid();
        var item = new TestAllType
        {
            StringNotNull = "txt",
            Int64NotNull = 99,
            BoolNotNull = true,
            GuidNotNull = guid
        };
        var t = typeof(TestAllType);

        Assert.Equal("txt", PropertyHelper.GetFormattedValue(t.GetProperty("StringNotNull")!, item));
        Assert.Equal("99", PropertyHelper.GetFormattedValue(t.GetProperty("Int64NotNull")!, item));
        Assert.Equal(true.ToString(), PropertyHelper.GetFormattedValue(t.GetProperty("BoolNotNull")!, item));
        Assert.Equal(guid.ToString(), PropertyHelper.GetFormattedValue(t.GetProperty("GuidNotNull")!, item));
        Assert.Equal(string.Empty, PropertyHelper.GetFormattedValue(t.GetProperty("Int32Nullable")!, item));
    }

    [Fact]
    public void GetFormattedValue_DateTimeType_ReturnsToString()
    {
        var dt = new DateTime(2024, 1, 2, 3, 4, 5);
        var item = new DateTimeType { DateTimeNotNull = dt };
        var prop = typeof(DateTimeType).GetProperty(nameof(DateTimeType.DateTimeNotNull))!;

        Assert.Equal(dt.ToString(), PropertyHelper.GetFormattedValue(prop, item));
        Assert.Equal(string.Empty,
            PropertyHelper.GetFormattedValue(typeof(DateTimeType).GetProperty(nameof(DateTimeType.DateTimeNullable))!, item));
    }

    #endregion
}
