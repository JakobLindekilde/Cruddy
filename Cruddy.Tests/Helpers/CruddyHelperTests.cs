using Cruddy.Helpers;
using Cruddy.Tests.Models;

namespace Cruddy.Tests.Helpers;

public class CruddyHelperTests
{
    private sealed class NoDefaultCtor
    {
        public NoDefaultCtor(int value) { Value = value; }
        public int Value { get; set; }
        public string? Text { get; set; }
    }

    private interface IShape { }

    #region DeepCopy

    [Fact]
    public void DeepCopy_ClassWithKey_ReturnsNewInstanceWithEqualValues()
    {
        var original = new ClassWithKey { Id = 5, Name = "Name", Description = "Desc" };

        var copy = CruddyHelper.DeepCopy(original);

        Assert.NotSame(original, copy);
        Assert.Equal(5, copy.Id);
        Assert.Equal("Name", copy.Name);
        Assert.Equal("Desc", copy.Description);
    }

    [Fact]
    public void DeepCopy_ModifyingCopy_DoesNotAffectOriginal()
    {
        var original = new ClassWithKey { Id = 1, Name = "Original" };

        var copy = CruddyHelper.DeepCopy(original);
        copy.Name = "Changed";
        copy.Id = 99;

        Assert.Equal("Original", original.Name);
        Assert.Equal(1, original.Id);
    }

    [Fact]
    public void DeepCopy_NullableProperties_NullPreserved()
    {
        var original = new ClassWithoutKey { Firstname = "A", Lastname = null, Hight = null };

        var copy = CruddyHelper.DeepCopy(original);

        Assert.Null(copy.Lastname);
        Assert.Null(copy.Hight);
        Assert.Equal("A", copy.Firstname);
    }

    [Fact]
    public void DeepCopy_NullableProperties_ValuesPreserved()
    {
        var original = new ClassWithoutKey { Id = 3, Firstname = "A", Lastname = "B", Age = 40, Hight = 180 };

        var copy = CruddyHelper.DeepCopy(original);

        Assert.Equal(3, copy.Id);
        Assert.Equal("B", copy.Lastname);
        Assert.Equal(40, copy.Age);
        Assert.Equal(180, copy.Hight);
    }

    [Fact]
    public void DeepCopy_DerivedType_CopiesBaseAndDerivedProperties()
    {
        var original = new Person { Id = 7, Name = "P", Description = "D", Email = "p@x.dk" };

        var copy = CruddyHelper.DeepCopy(original);

        Assert.NotSame(original, copy);
        Assert.Equal(7, copy.Id);
        Assert.Equal("P", copy.Name);
        Assert.Equal("D", copy.Description);
        Assert.Equal("p@x.dk", copy.Email);
    }

    [Fact]
    public void DeepCopy_AllTypes_PreservesAllValues()
    {
        var guid = Guid.NewGuid();
        var original = new TestAllType
        {
            Id = 1,
            Int32NotNull = int.MaxValue,
            Int32Nullable = 5,
            Int64NotNull = long.MaxValue,
            Int64Nullable = null,
            Int16NotNull = short.MinValue,
            Int16Nullable = 3,
            ByteNotNull = 255,
            ByteNullable = 1,
            DecimalNotNull = 123.45m,
            DecimalNullable = null,
            DoubleNotNull = 1.5,
            DoubleNullable = 2.5,
            SingleNotNull = 3.5f,
            SingleNullable = null,
            BoolNotNull = true,
            BoolNullable = false,
            StringNotNull = "text",
            StringNullable = null,
            GuidNotNull = guid,
            GuidNullable = guid
        };

        var copy = CruddyHelper.DeepCopy(original);

        Assert.NotSame(original, copy);
        Assert.Equal(original.Id, copy.Id);
        Assert.Equal(int.MaxValue, copy.Int32NotNull);
        Assert.Equal(5, copy.Int32Nullable);
        Assert.Equal(long.MaxValue, copy.Int64NotNull);
        Assert.Null(copy.Int64Nullable);
        Assert.Equal(short.MinValue, copy.Int16NotNull);
        Assert.Equal((short)3, copy.Int16Nullable);
        Assert.Equal((byte)255, copy.ByteNotNull);
        Assert.Equal((byte)1, copy.ByteNullable);
        Assert.Equal(123.45m, copy.DecimalNotNull);
        Assert.Null(copy.DecimalNullable);
        Assert.Equal(1.5, copy.DoubleNotNull);
        Assert.Equal(2.5, copy.DoubleNullable);
        Assert.Equal(3.5f, copy.SingleNotNull);
        Assert.Null(copy.SingleNullable);
        Assert.True(copy.BoolNotNull);
        Assert.False(copy.BoolNullable);
        Assert.Equal("text", copy.StringNotNull);
        Assert.Null(copy.StringNullable);
        Assert.Equal(guid, copy.GuidNotNull);
        Assert.Equal(guid, copy.GuidNullable);
    }

    [Fact]
    public void DeepCopy_DateTimeTypes_PreservesValues()
    {
        var original = new DateTimeType
        {
            Id = 2,
            DateTimeNotNull = new DateTime(2024, 5, 6, 7, 8, 9, DateTimeKind.Unspecified),
            DateTimeNullable = null,
            TimeSpanNotNull = TimeSpan.FromMinutes(90),
            TimeSpanNullable = TimeSpan.FromSeconds(5),
            TimeOnlyNotNull = new TimeOnly(12, 30),
            TimeOnlyNullable = null
        };

        var copy = CruddyHelper.DeepCopy(original);

        Assert.Equal(original.DateTimeNotNull, copy.DateTimeNotNull);
        Assert.Null(copy.DateTimeNullable);
        Assert.Equal(original.TimeSpanNotNull, copy.TimeSpanNotNull);
        Assert.Equal(original.TimeSpanNullable, copy.TimeSpanNullable);
        Assert.Equal(original.TimeOnlyNotNull, copy.TimeOnlyNotNull);
        Assert.Null(copy.TimeOnlyNullable);
    }

    [Fact]
    public void DeepCopy_PrivateProperties_AreNotCopied()
    {
        var original = new ClassWithKey { Id = 1, Name = "N" };

        var copy = CruddyHelper.DeepCopy(original);

        var priv = typeof(ClassWithKey).GetProperty("PrivateProperty",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        Assert.Equal("Private", priv.GetValue(copy));
    }

    [Fact]
    public void DeepCopy_Primitive_ReturnsSameValue()
    {
        Assert.Equal(42, CruddyHelper.DeepCopy(42));
        Assert.Equal("abc", CruddyHelper.DeepCopy("abc"));
    }

    [Fact]
    public void DeepCopy_List_CopiesElements()
    {
        var original = new List<ClassWithKey>
        {
            new() { Id = 1, Name = "A" },
            new() { Id = 2, Name = "B" }
        };

        var copy = CruddyHelper.DeepCopy(original);

        Assert.NotSame(original, copy);
        Assert.Equal(2, copy.Count);
        Assert.NotSame(original[0], copy[0]);
        Assert.Equal("B", copy[1].Name);
    }

    [Fact]
    public void DeepCopy_NoDefaultCtor_Works()
    {
        var copy = CruddyHelper.DeepCopy(new NoDefaultCtor(10) { Text = "t" });

        Assert.Equal(10, copy.Value);
        Assert.Equal("t", copy.Text);
    }

    [Fact]
    public void DeepCopy_NullItem_ReturnsNull()
    {
        var result = CruddyHelper.DeepCopy<ClassWithKey?>(null);

        Assert.Null(result);
    }

    #endregion

    #region NewInstance

    [Fact]
    public void NewInstance_ClassWithKey_ReturnsNewInstanceWithDefaults()
    {
        var instance = CruddyHelper.NewInstance<ClassWithKey>();

        Assert.NotNull(instance);
        Assert.Equal(0, instance.Id);
        Assert.Null(instance.Description);
    }

    [Fact]
    public void NewInstance_ClassWithoutKey_ReturnsNewInstance()
    {
        var instance = CruddyHelper.NewInstance<ClassWithoutKey>();

        Assert.NotNull(instance);
        Assert.Equal(0, instance.Age);
        Assert.Null(instance.Hight);
        Assert.Null(instance.Lastname);
    }

    [Fact]
    public void NewInstance_DerivedType_ReturnsDerivedInstance()
    {
        var instance = CruddyHelper.NewInstance<Person>();

        Assert.IsType<Person>(instance);
        Assert.Null(instance.Email);
    }

    [Fact]
    public void NewInstance_TestAllType_ReturnsDefaults()
    {
        var instance = CruddyHelper.NewInstance<TestAllType>();

        Assert.NotNull(instance);
        Assert.Equal(0, instance.Int32NotNull);
        Assert.Null(instance.Int32Nullable);
        Assert.False(instance.BoolNotNull);
        Assert.Equal(Guid.Empty, instance.GuidNotNull);
        Assert.Null(instance.StringNullable);
    }

    [Fact]
    public void NewInstance_DateTimeType_ReturnsDefaults()
    {
        var instance = CruddyHelper.NewInstance<DateTimeType>();

        Assert.NotNull(instance);
        Assert.Equal(default, instance.DateTimeNotNull);
        Assert.Null(instance.DateTimeNullable);
        Assert.Equal(TimeSpan.Zero, instance.TimeSpanNotNull);
        Assert.Null(instance.TimeOnlyNullable);
    }

    [Fact]
    public void NewInstance_CalledTwice_ReturnsDifferentInstances()
    {
        var a = CruddyHelper.NewInstance<ClassWithKey>();
        var b = CruddyHelper.NewInstance<ClassWithKey>();

        Assert.NotSame(a, b);
    }

    [Fact]
    public void NewInstance_ValueType_ReturnsDefault()
    {
        Assert.Equal(0, CruddyHelper.NewInstance<int>());
        Assert.Equal(Guid.Empty, CruddyHelper.NewInstance<Guid>());
    }

    [Fact]
    public void NewInstance_NoDefaultCtor_FallsBackToJsonDeserialization()
    {
        var instance = CruddyHelper.NewInstance<NoDefaultCtor>();

        Assert.NotNull(instance);
        Assert.Equal(0, instance.Value);
        Assert.Null(instance.Text);
    }

    [Fact]
    public void NewInstance_Interface_Throws()
    {
        Assert.ThrowsAny<Exception>(() => CruddyHelper.NewInstance<IShape>());
    }

    #endregion
}
