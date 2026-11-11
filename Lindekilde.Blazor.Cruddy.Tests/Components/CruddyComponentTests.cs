#pragma warning disable BL0005 // Component parameter should not be set outside of its component.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

using Lindekilde.Blazor.Cruddy.Components;
using Lindekilde.Blazor.Cruddy.Repositories;

namespace Lindekilde.Blazor.Cruddy.Tests.Components;

using Cruddy = Lindekilde.Blazor.Cruddy.Components.Cruddy;

public class CruddyComponentTests
{
    private sealed class TestCruddy : Cruddy
    {
        public int ColumnCount => MyColumns.Count;
        public bool Handle(CruddyField field, string title) => HandleFKey(field, title);
        public string? Format(CruddyField field) => GetDisplayFormat(field);
        public void AddOptions(string name, List<FKeyOption> options) => FKeyOptions[name] = options;

        public void ConfigureForeignKeys(string? foreignKeys)
        {
            FKeys = foreignKeys;
            FKeyInfoList = FillFKeyInfoList(FKeys, DefaultSchema, PluralizeTableName);
        }
    }

    private static CruddyField Field(string column, Type? type = null, bool nullable = true, bool pk = false, bool isDate = false) =>
        new(column, column, type ?? typeof(string), nullable, pk, false, isDate);

    private static TestCruddy Create(FakeDbConnection? db = null) =>
        new() { TableName = "Persons", DbConnection = db ?? new FakeDbConnection(), RepositoryFactory = new TableRepositorySqlServerFactory() };

    private static FKeyOption Option(object? id, object? display) => new() { Id = id, Display = display };

    #region GetInputInfo

    [Fact]
    public void GetInputInfo_ReturnsMetadataFromField()
    {
        var dut = Create();

        var info = dut.GetInputInfo(Field("Name", nullable: false), null, CrudOperation.Read);

        Assert.Equal("Name", info.DisplayName);
        Assert.Equal(typeof(string), info.Type);
        Assert.Null(info.Value);
        Assert.Null(info.FormattedValue);
        Assert.False(info.Disabled);
        Assert.True(info.Required);
        Assert.False(info.IsKeyColumn);
        Assert.Null(info.DataType);
        Assert.True(info.Visible);
    }

    [Fact]
    public void GetInputInfo_NullableFieldIsNotRequired()
    {
        var info = Create().GetInputInfo(Field("Age", typeof(int)), null, CrudOperation.Read);

        Assert.Equal(typeof(int), info.Type);
        Assert.False(info.Required);
    }

    [Fact]
    public void GetInputInfo_DateFieldHasDataTypeDate()
    {
        var info = Create().GetInputInfo(Field("Born", typeof(DateTime), isDate: true), null, CrudOperation.Read);

        Assert.Equal(System.ComponentModel.DataAnnotations.DataType.Date, info.DataType);
    }

    [Fact]
    public void GetInputInfo_UsesAliasAsDisplayName()
    {
        var info = Create().GetInputInfo(Field("Name") with { Name = "Who" }, null, CrudOperation.Read);

        Assert.Equal("Who", info.DisplayName);
    }

    [Fact]
    public void GetInputInfo_ReturnsValueAndFormattedValue()
    {
        var dut = Create();
        dut.DefaultNumberFormat = "000";
        var row = new CruddyRow { ["Id"] = 7, ["Name"] = "Alice" };

        var id = dut.GetInputInfo(Field("Id", typeof(int)), row, CrudOperation.Read);
        var name = dut.GetInputInfo(Field("Name"), row, CrudOperation.Read);

        Assert.Equal(7, id.Value);
        Assert.Equal("007", id.FormattedValue);
        Assert.Equal("Alice", name.Value);
        Assert.Equal("Alice", name.FormattedValue);
    }

    [Fact]
    public void GetInputInfo_NullValue_GivesEmptyFormattedValue()
    {
        var info = Create().GetInputInfo(Field("Name"), new CruddyRow { ["Name"] = null }, CrudOperation.Read);

        Assert.Null(info.Value);
        Assert.Equal(string.Empty, info.FormattedValue);
    }

    [Theory]
    [InlineData(CrudOperation.Create, false, true)]
    [InlineData(CrudOperation.Create, true, false)]
    [InlineData(CrudOperation.Update, false, true)]
    [InlineData(CrudOperation.Read, false, true)]
    public void GetInputInfo_DisablesKeyFieldAccordingToOperation(CrudOperation operation, bool allowKeyOnCreate, bool expectedDisabled)
    {
        var dut = Create();
        dut.KeyColumn = "Id";
        dut.AllowKeyColumnEditOnCreate = allowKeyOnCreate;

        var info = dut.GetInputInfo(Field("id", typeof(int), false, true) with { Column = "Id" }, null, operation);

        Assert.True(info.IsKeyColumn);
        Assert.Equal(expectedDisabled, info.Disabled);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void GetInputInfo_HideKeyColumnControlsVisibility(bool hide, bool expectedVisible)
    {
        var dut = Create();
        dut.KeyColumn = "Id";
        dut.HideKeyColumn = hide;

        var key = dut.GetInputInfo(Field("Id", typeof(int)), null, CrudOperation.Read);
        var other = dut.GetInputInfo(Field("Name"), null, CrudOperation.Read);

        Assert.Equal(expectedVisible, key.Visible);
        Assert.True(other.Visible);
    }

    #endregion

    #region GetDisplayFormat

    [Theory]
    [InlineData(typeof(int), "number-default")]
    [InlineData(typeof(long), "number-default")]
    [InlineData(typeof(short), "number-default")]
    [InlineData(typeof(byte), "number-default")]
    [InlineData(typeof(decimal), "decimal-default")]
    [InlineData(typeof(double), "decimal-default")]
    [InlineData(typeof(float), "decimal-default")]
    [InlineData(typeof(DateTime), "datetime-default")]
    public void GetDisplayFormat_UsesDefaultForType(Type type, string expected)
    {
        var dut = Create();
        dut.DefaultNumberFormat = "number-default";
        dut.DefaultDecimalFormat = "decimal-default";
        dut.DefaultDateTimeFormat = "datetime-default";

        Assert.Equal(expected, dut.Format(Field("X", type)));
    }

    [Fact]
    public void GetDisplayFormat_DateField_UsesDateFormat()
    {
        var dut = Create();
        dut.DefaultDateFormat = "date-default";
        dut.DefaultDateTimeFormat = "datetime-default";

        Assert.Equal("date-default", dut.Format(Field("D", typeof(DateTime), isDate: true)));
    }

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(bool))]
    [InlineData(typeof(Guid))]
    public void GetDisplayFormat_OtherTypes_ReturnNull(Type type)
    {
        Assert.Null(Create().Format(Field("X", type)));
    }

    [Fact]
    public void GetDisplayFormat_EmptyDefault_ReturnsNull()
    {
        var dut = Create();
        dut.DefaultNumberFormat = "";

        Assert.Null(dut.Format(Field("X", typeof(int))));
    }

    #endregion

    #region GetFkeyOption

    private static InputInfo Info(CruddyField field, object? value) => new()
    {
        DisplayName = field.Name,
        DisplayFormat = null,
        Type = field.Type,
        Disabled = false,
        Required = false,
        HideKeyColumn = false,
        IsKeyColumn = false,
        Value = value
    };

    [Fact]
    public void GetFkeyOption_ExactMatch_ReturnsOption()
    {
        var dut = Create();
        var option = Option(42, "Books");
        dut.AddOptions("CategoryId", [option]);
        var field = Field("CategoryId", typeof(int));

        Assert.Same(option, dut.GetFkeyOption(field, Info(field, 42)));
    }

    [Fact]
    public void GetFkeyOption_StringRepresentationMatch_ReturnsOption()
    {
        var dut = Create();
        var option = Option(42, "Books");
        dut.AddOptions("CategoryCode", [option]);
        var field = Field("CategoryCode");

        Assert.Same(option, dut.GetFkeyOption(field, Info(field, "42")));
    }

    [Fact]
    public void GetFkeyOption_GuidAndStringMatch()
    {
        var dut = Create();
        var id = Guid.NewGuid();
        var option = Option(id.ToString(), "Token");
        dut.AddOptions("TokenId", [option]);
        var field = Field("TokenId", typeof(Guid));

        Assert.Same(option, dut.GetFkeyOption(field, Info(field, id)));
    }

    [Fact]
    public void GetFkeyOption_DuplicateMatches_ReturnsFirst()
    {
        var dut = Create();
        var first = Option(42, "First");
        dut.AddOptions("CategoryId", [first, Option("42", "Second")]);
        var field = Field("CategoryId", typeof(int));

        Assert.Same(first, dut.GetFkeyOption(field, Info(field, 42)));
    }

    [Fact]
    public void GetFkeyOption_NullValue_ReturnsNull()
    {
        var dut = Create();
        dut.AddOptions("CategoryId", [Option(null, "No category")]);
        var field = Field("CategoryId", typeof(int));

        Assert.Null(dut.GetFkeyOption(field, Info(field, null)));
    }

    [Fact]
    public void GetFkeyOption_NoOptionsOrNoMatch_ReturnsNull()
    {
        var dut = Create();
        var field = Field("CategoryId", typeof(int));
        Assert.Null(dut.GetFkeyOption(field, Info(field, 42)));

        dut.AddOptions("CategoryId", [Option(7, "Other")]);
        Assert.Null(dut.GetFkeyOption(field, Info(field, 42)));
    }

    #endregion

    #region HandleFKey

    [Fact]
    public void HandleFKey_ConfiguredForeignKey_AddsColumnAndLoadsOptions()
    {
        var db = new FakeDbConnection();
        db.Rows.Add(new Dictionary<string, object?> { ["Id"] = 1, ["Display"] = "Books" });
        var dut = Create(db);
        dut.ConfigureForeignKeys("categoryid: Categories, Id, Name");

        var handled = dut.Handle(Field("CategoryId", typeof(int)), "Category");

        Assert.True(handled);
        Assert.Equal(1, dut.ColumnCount);
        var command = Assert.Single(db.Commands);
        Assert.Equal("SELECT Id AS Id, Name AS Display FROM dbo.Categories ORDER BY Name", command.Sql);
    }

    [Fact]
    public void HandleFKey_RepeatedCallsReuseLoadedOptions()
    {
        var db = new FakeDbConnection();
        var dut = Create(db);
        dut.ConfigureForeignKeys("CategoryId: Categories, Id, Name");
        var field = Field("CategoryId", typeof(int));

        Assert.True(dut.Handle(field, "Category"));
        Assert.True(dut.Handle(field, "Category"));

        Assert.Equal(2, dut.ColumnCount);
        Assert.Single(db.Commands);
    }

    [Fact]
    public void HandleFKey_NonForeignKey_DoesNotAddColumn()
    {
        var db = new FakeDbConnection();
        var dut = Create(db);
        dut.ConfigureForeignKeys("CategoryId: Categories, Id, Name");

        Assert.False(dut.Handle(Field("CategoryCode"), "Category code"));
        Assert.Equal(0, dut.ColumnCount);
    }

    [Fact]
    public void HandleFKey_NoForeignKeys_ReturnsFalseWithoutLoadingOptions()
    {
        var db = new FakeDbConnection();
        var dut = Create(db);
        dut.ConfigureForeignKeys(null);

        Assert.False(dut.Handle(Field("CategoryId", typeof(int)), "Category"));
        Assert.Empty(db.Commands);
    }

    #endregion

    #region GetForeignKeyDisplayValue / Text

    private static CruddyRow Row(object? value) => new() { ["CategoryId"] = value };

    private static readonly CruddyField FkField = new("CategoryId", "CategoryId", typeof(int), true, false, false, false);

    [Fact]
    public void GetForeignKeyDisplayValue_MatchingIdentifier_ReturnsDisplay()
    {
        var result = Cruddy.GetForeignKeyDisplayValue(FkField, Row(7), [Option(7, "Books")]);
        Assert.Equal("Books", result);
    }

    [Fact]
    public void GetForeignKeyDisplayValue_StringRepresentationsMatch()
    {
        var result = Cruddy.GetForeignKeyDisplayValue(FkField, Row("42"), [Option(42, "Meaning of life")]);
        Assert.Equal("Meaning of life", result);
    }

    [Fact]
    public void GetForeignKeyDisplayValue_NoMatchOrEmptyOptions_ReturnsOriginal()
    {
        Assert.Equal(7, Cruddy.GetForeignKeyDisplayValue(FkField, Row(7), [Option(8, "Other")]));
        Assert.Equal(7, Cruddy.GetForeignKeyDisplayValue(FkField, Row(7), []));
    }

    [Fact]
    public void GetForeignKeyDisplayValue_NullValue_ReturnsNull()
    {
        Assert.Null(Cruddy.GetForeignKeyDisplayValue(FkField, Row(null), [Option(null, "Null category")]));
    }

    [Fact]
    public void GetForeignKeyDisplayValue_DuplicateIdentifiers_ReturnsFirst()
    {
        var result = Cruddy.GetForeignKeyDisplayValue(FkField, Row(7), [Option(7, "First"), Option("7", "Second")]);
        Assert.Equal("First", result);
    }

    [Fact]
    public void GetForeignKeyDisplayText_ReturnsTextOrNull()
    {
        Assert.Equal("123", Cruddy.GetForeignKeyDisplayText(FkField, Row(7), [Option(7, 123)]));
        Assert.Equal("7", Cruddy.GetForeignKeyDisplayText(FkField, Row(7), [Option(8, "Other")]));
        Assert.Null(Cruddy.GetForeignKeyDisplayText(FkField, Row(null), []));
        Assert.Null(Cruddy.GetForeignKeyDisplayText(FkField, Row(7), [Option(7, null)]));
    }

    #endregion
}
#pragma warning restore BL0005
#pragma warning restore CS8625
