using Lindekilde.Blazor.Cruddy.Components;
using System.Reflection;

#pragma warning disable BL0005 // Component parameter should not be set outside of its component.

namespace Lindekilde.Blazor.Cruddy.Tests.Components;

public class ForeignKeyDisplayTests
{
    private sealed class ForeignKeyEntity
    {
        public int? CategoryId { get; set; }
        public string? CategoryCode { get; set; }
        public Guid? TokenId { get; set; }
    }

    private sealed class TestCruddy : Cruddy<ForeignKeyEntity>
    {
        public int ColumnCount => MyColumns.Count;

        public bool Handle(PropertyInfo property, string title) => HandleFKey(property, title);

        public void ConfigureForeignKeys(string? foreignKeys)
        {
            FKeys = foreignKeys;
            FKeyInfoList = FillFKeyInfoList(FKeys, DefaultSchema, PluralizeTableName);
        }
    }

    private static PropertyInfo Property(string name) => typeof(ForeignKeyEntity).GetProperty(name)!;

    private static FKeyOption Option(object? id, object? display) => new() { Id = id, Display = display };

    private static TestCruddy Create(FakeDbConnection connection, string? foreignKeys)
    {
        var component = new TestCruddy { DbConnection = connection };
        component.ConfigureForeignKeys(foreignKeys);
        return component;
    }

    [Fact]
    public void HandleFKey_ConfiguredForeignKey_AddsColumnAndLoadsOptions()
    {
        var connection = new FakeDbConnection();
        connection.Rows.Add(new Dictionary<string, object?> { ["Id"] = 1, ["Display"] = "Books" });
        var component = Create(connection, "categoryid: Categories, Id, Name");

        var handled = component.Handle(Property(nameof(ForeignKeyEntity.CategoryId)), "Category");

        Assert.True(handled);
        Assert.Equal(1, component.ColumnCount);
        var command = Assert.Single(connection.Commands);
        Assert.Equal("SELECT Id AS Id, Name AS Display FROM dbo.Categories ORDER BY Name", command.Sql);
    }

    [Fact]
    public void HandleFKey_RepeatedCallsReuseLoadedOptions()
    {
        var connection = new FakeDbConnection();
        var component = Create(connection, "CategoryId: Categories, Id, Name");
        var property = Property(nameof(ForeignKeyEntity.CategoryId));

        Assert.True(component.Handle(property, "Category"));
        Assert.True(component.Handle(property, "Category"));

        Assert.Equal(2, component.ColumnCount);
        Assert.Single(connection.Commands);
    }

    [Fact]
    public void HandleFKey_NonForeignKey_DoesNotAddColumn()
    {
        var connection = new FakeDbConnection();
        var component = Create(connection, "CategoryId: Categories, Id, Name");

        var handled = component.Handle(Property(nameof(ForeignKeyEntity.CategoryCode)), "Category code");

        Assert.False(handled);
        Assert.Equal(0, component.ColumnCount);
        Assert.Single(connection.Commands);
    }

    [Fact]
    public void HandleFKey_NoForeignKeysConfigured_ReturnsFalseWithoutLoadingOptions()
    {
        var connection = new FakeDbConnection();
        var component = Create(connection, null);

        var handled = component.Handle(Property(nameof(ForeignKeyEntity.CategoryId)), "Category");

        Assert.False(handled);
        Assert.Equal(0, component.ColumnCount);
        Assert.Empty(connection.Commands);
    }

    [Fact]
    public void GetForeignKeyDisplayValue_MatchingIdentifier_ReturnsDisplayValue()
    {
        var entity = new ForeignKeyEntity { CategoryId = 7 };
        var options = new List<FKeyOption> { Option(7, "Books") };

        var result = Cruddy<ForeignKeyEntity>.GetForeignKeyDisplayValue(
            Property(nameof(ForeignKeyEntity.CategoryId)), entity, options);

        Assert.Equal("Books", result);
    }

    [Fact]
    public void GetForeignKeyDisplayValue_StringRepresentationsMatchAcrossTypes()
    {
        var entity = new ForeignKeyEntity { CategoryCode = "42" };
        var options = new List<FKeyOption> { Option(42, "Meaning of life") };

        var result = Cruddy<ForeignKeyEntity>.GetForeignKeyDisplayValue(
            Property(nameof(ForeignKeyEntity.CategoryCode)), entity, options);

        Assert.Equal("Meaning of life", result);
    }

    [Fact]
    public void GetForeignKeyDisplayValue_GuidStringRepresentationMatchesGuidProperty()
    {
        var id = Guid.NewGuid();
        var entity = new ForeignKeyEntity { TokenId = id };
        var options = new List<FKeyOption> { Option(id.ToString(), "Token") };

        var result = Cruddy<ForeignKeyEntity>.GetForeignKeyDisplayValue(
            Property(nameof(ForeignKeyEntity.TokenId)), entity, options);

        Assert.Equal("Token", result);
    }

    [Fact]
    public void GetForeignKeyDisplayValue_NoMatchingIdentifier_ReturnsOriginalValue()
    {
        var entity = new ForeignKeyEntity { CategoryId = 7 };
        var options = new List<FKeyOption> { Option(8, "Other category") };

        var result = Cruddy<ForeignKeyEntity>.GetForeignKeyDisplayValue(
            Property(nameof(ForeignKeyEntity.CategoryId)), entity, options);

        Assert.Equal(7, result);
    }

    [Fact]
    public void GetForeignKeyDisplayValue_EmptyOptions_ReturnsOriginalValue()
    {
        var entity = new ForeignKeyEntity { CategoryId = 7 };

        var result = Cruddy<ForeignKeyEntity>.GetForeignKeyDisplayValue(
            Property(nameof(ForeignKeyEntity.CategoryId)), entity, []);

        Assert.Equal(7, result);
    }

    [Fact]
    public void GetForeignKeyDisplayValue_NullValue_ReturnsNullEvenWhenOptionHasNullId()
    {
        var entity = new ForeignKeyEntity { CategoryId = null };
        var options = new List<FKeyOption> { Option(null, "Null category") };

        var result = Cruddy<ForeignKeyEntity>.GetForeignKeyDisplayValue(
            Property(nameof(ForeignKeyEntity.CategoryId)), entity, options);

        Assert.Null(result);
    }

    [Fact]
    public void GetForeignKeyDisplayValue_MatchingOptionWithNullDisplay_ReturnsNull()
    {
        var entity = new ForeignKeyEntity { CategoryId = 7 };
        var options = new List<FKeyOption> { Option(7, null) };

        var result = Cruddy<ForeignKeyEntity>.GetForeignKeyDisplayValue(
            Property(nameof(ForeignKeyEntity.CategoryId)), entity, options);

        Assert.Null(result);
    }

    [Fact]
    public void GetForeignKeyDisplayValue_DuplicateIdentifiers_ReturnsFirstDisplayValue()
    {
        var entity = new ForeignKeyEntity { CategoryId = 7 };
        var options = new List<FKeyOption> { Option(7, "First"), Option("7", "Second") };

        var result = Cruddy<ForeignKeyEntity>.GetForeignKeyDisplayValue(
            Property(nameof(ForeignKeyEntity.CategoryId)), entity, options);

        Assert.Equal("First", result);
    }

    [Fact]
    public void GetForeignKeyDisplayText_MatchingIdentifier_ReturnsDisplayAsText()
    {
        var entity = new ForeignKeyEntity { CategoryId = 7 };
        var options = new List<FKeyOption> { Option(7, 123) };

        var result = Cruddy<ForeignKeyEntity>.GetForeignKeyDisplayText(
            Property(nameof(ForeignKeyEntity.CategoryId)), entity, options);

        Assert.Equal("123", result);
    }

    [Fact]
    public void GetForeignKeyDisplayText_UnmatchedIdentifier_ReturnsOriginalValueAsText()
    {
        var entity = new ForeignKeyEntity { CategoryId = 7 };
        var options = new List<FKeyOption> { Option(8, "Other category") };

        var result = Cruddy<ForeignKeyEntity>.GetForeignKeyDisplayText(
            Property(nameof(ForeignKeyEntity.CategoryId)), entity, options);

        Assert.Equal("7", result);
    }

    [Fact]
    public void GetForeignKeyDisplayText_NullValue_ReturnsNull()
    {
        var entity = new ForeignKeyEntity { CategoryId = null };

        var result = Cruddy<ForeignKeyEntity>.GetForeignKeyDisplayText(
            Property(nameof(ForeignKeyEntity.CategoryId)), entity, []);

        Assert.Null(result);
    }

    [Fact]
    public void GetForeignKeyDisplayText_MatchingOptionWithNullDisplay_ReturnsNull()
    {
        var entity = new ForeignKeyEntity { CategoryId = 7 };
        var options = new List<FKeyOption> { Option(7, null) };

        var result = Cruddy<ForeignKeyEntity>.GetForeignKeyDisplayText(
            Property(nameof(ForeignKeyEntity.CategoryId)), entity, options);

        Assert.Null(result);
    }
}
