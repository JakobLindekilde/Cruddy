using Lindekilde.Blazor.Cruddy.Components;
using System.Reflection;

namespace Lindekilde.Blazor.Cruddy.Tests.Components;

public class GetFkeyOptionTests
{
    private sealed class Entity
    {
        public int? CategoryId { get; set; }
        public string? CategoryCode { get; set; }
        public Guid? TokenId { get; set; }
    }

    private sealed class TestCruddy : Cruddy<Entity>
    {
        public void AddOptions(string propertyName, List<FKeyOption> options) => FKeyOptions[propertyName] = options;

        public FKeyOption? FindOption(PropertyInfo property, object? value) =>
            GetFkeyOption(property, new InputInfo
            {
                DisplayName = property.Name,
                DisplayFormat = null,
                Type = property.PropertyType,
                Disabled = false,
                Required = false,
                HideKeyColumn = false,
                IsKeyColumn = false,
                Value = value
            });
    }

    private static PropertyInfo Property(string name) => typeof(Entity).GetProperty(name)!;

    private static TestCruddy Create() => new() { DbConnection = new FakeDbConnection() };

    private static FKeyOption Option(object? id, object? display) => new() { Id = id, Display = display };

    [Fact]
    public void GetFkeyOption_ExactIdentifierMatch_ReturnsMatchingOption()
    {
        var component = Create();
        var option = Option(42, "Books");
        component.AddOptions(nameof(Entity.CategoryId), [option]);

        var result = component.FindOption(Property(nameof(Entity.CategoryId)), 42);

        Assert.Same(option, result);
    }

    [Fact]
    public void GetFkeyOption_StringRepresentationMatch_ReturnsMatchingOption()
    {
        var component = Create();
        var option = Option(42, "Books");
        component.AddOptions(nameof(Entity.CategoryCode), [option]);

        var result = component.FindOption(Property(nameof(Entity.CategoryCode)), "42");

        Assert.Same(option, result);
    }

    [Fact]
    public void GetFkeyOption_GuidAndStringIdentifiersMatch()
    {
        var component = Create();
        var id = Guid.NewGuid();
        var option = Option(id.ToString(), "Token");
        component.AddOptions(nameof(Entity.TokenId), [option]);

        var result = component.FindOption(Property(nameof(Entity.TokenId)), id);

        Assert.Same(option, result);
    }

    [Fact]
    public void GetFkeyOption_DuplicateMatches_ReturnsFirstOption()
    {
        var component = Create();
        var firstOption = Option(42, "First");
        component.AddOptions(nameof(Entity.CategoryId), [firstOption, Option("42", "Second")]);

        var result = component.FindOption(Property(nameof(Entity.CategoryId)), 42);

        Assert.Same(firstOption, result);
    }

    [Fact]
    public void GetFkeyOption_NullValue_ReturnsNullEvenWhenNullIdExists()
    {
        var component = Create();
        component.AddOptions(nameof(Entity.CategoryId), [Option(null, "No category")]);

        var result = component.FindOption(Property(nameof(Entity.CategoryId)), null);

        Assert.Null(result);
    }

    [Fact]
    public void GetFkeyOption_NoOptionsForProperty_ReturnsNull()
    {
        var component = Create();

        var result = component.FindOption(Property(nameof(Entity.CategoryId)), 42);

        Assert.Null(result);
    }

    [Fact]
    public void GetFkeyOption_NoMatchingIdentifier_ReturnsNull()
    {
        var component = Create();
        component.AddOptions(nameof(Entity.CategoryId), [Option(7, "Other category")]);

        var result = component.FindOption(Property(nameof(Entity.CategoryId)), 42);

        Assert.Null(result);
    }

    [Fact]
    public void GetFkeyOption_PropertyOptionLookup_IsCaseInsensitive()
    {
        var component = Create();
        var option = Option(42, "Books");
        component.AddOptions("categoryid", [option]);

        var result = component.FindOption(Property(nameof(Entity.CategoryId)), 42);

        Assert.Same(option, result);
    }
}
