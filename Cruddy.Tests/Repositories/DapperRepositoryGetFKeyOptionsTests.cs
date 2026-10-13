using Cruddy.Components;
using Cruddy.Repositories;
using Cruddy.Tests.Components;
using Cruddy.Tests.Models;

namespace Cruddy.Tests.Repositories;

/// <summary>
/// Unit tests for <see cref="DapperRepository{TEntity}.GetFKeyOptions"/> using an in-memory fake connection.
/// </summary>
public class DapperRepositoryGetFKeyOptionsTests
{
    private static DapperRepository<ClassWithKey> CreateRepo(FakeDbConnection connection) =>
        new(connection, "dbo", "People", "Id");

    private static Dictionary<string, object?> Row(object? id, object? display) =>
        new() { ["Id"] = id, ["Display"] = display };

    [Fact]
    public void GetFKeyOptions_BuildsExpectedSql()
    {
        var connection = new FakeDbConnection();
        var info = new FKeyInfo("CountryId", "dbo.Countries", "Id", "Name");

        CreateRepo(connection).GetFKeyOptions(info);

        var command = Assert.Single(connection.Commands);
        Assert.Equal("Reader", command.Kind);
        Assert.Equal("SELECT Id AS Id, Name AS Display FROM dbo.Countries ORDER BY Name", command.Sql);
    }

    [Fact]
    public void GetFKeyOptions_UsesCustomColumnNamesAndTable()
    {
        var connection = new FakeDbConnection();
        var info = new FKeyInfo("FK", "sales.Customers", "CustomerId", "CustomerName");

        CreateRepo(connection).GetFKeyOptions(info);

        Assert.Equal(
            "SELECT CustomerId AS Id, CustomerName AS Display FROM sales.Customers ORDER BY CustomerName",
            connection.Commands[0].Sql);
    }

    [Fact]
    public void GetFKeyOptions_DoesNotUseRepositorySchemaOrTable()
    {
        var connection = new FakeDbConnection();
        var repo = new DapperRepository<ClassWithKey>(connection, "other", "OtherTable", "OtherKey");

        repo.GetFKeyOptions(new FKeyInfo("FK", "dbo.Countries", "Id", "Name"));

        var sql = connection.Commands[0].Sql;
        Assert.DoesNotContain("other.", sql);
        Assert.DoesNotContain("OtherTable", sql);
        Assert.DoesNotContain("OtherKey", sql);
    }

    [Fact]
    public void GetFKeyOptions_NoRows_ReturnsEmptyList()
    {
        var connection = new FakeDbConnection();

        var result = CreateRepo(connection).GetFKeyOptions(new FKeyInfo("FK", "dbo.T", "Id", "Name"));

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetFKeyOptions_SingleRow_MapsIdAndDisplay()
    {
        var connection = new FakeDbConnection();
        connection.Rows.Add(Row(1, "Denmark"));

        var result = CreateRepo(connection).GetFKeyOptions(new FKeyInfo("FK", "dbo.T", "Id", "Name"));

        var option = Assert.Single(result);
        Assert.Equal(1, option.Id);
        Assert.Equal("Denmark", option.Display);
    }

    [Fact]
    public void GetFKeyOptions_MultipleRows_PreservesOrderAndCount()
    {
        var connection = new FakeDbConnection();
        connection.Rows.Add(Row(3, "Alpha"));
        connection.Rows.Add(Row(1, "Beta"));
        connection.Rows.Add(Row(2, "Gamma"));

        var result = CreateRepo(connection).GetFKeyOptions(new FKeyInfo("FK", "dbo.T", "Id", "Name"));

        Assert.Equal(3, result.Count);
        Assert.Equal(["Alpha", "Beta", "Gamma"], result.Select(o => o.Display));
        Assert.Equal([3, 1, 2], result.Select(o => (int)o.Id!));
    }

    [Fact]
    public void GetFKeyOptions_NullDisplay_MapsToNull()
    {
        var connection = new FakeDbConnection();
        connection.Rows.Add(Row(1, null));
        connection.Rows.Add(Row(2, "Two"));

        var result = CreateRepo(connection).GetFKeyOptions(new FKeyInfo("FK", "dbo.T", "Id", "Name"));

        Assert.Null(result[0].Display);
        Assert.Equal("Two", result[1].Display);
    }

    [Fact]
    public void GetFKeyOptions_GuidIds_AreMapped()
    {
        var id = Guid.NewGuid();
        var connection = new FakeDbConnection();
        connection.Rows.Add(Row(id, "Guid row"));

        var result = CreateRepo(connection).GetFKeyOptions(new FKeyInfo("FK", "dbo.T", "Id", "Name"));

        Assert.Equal(id, Assert.Single(result).Id);
    }

    [Fact]
    public void GetFKeyOptions_StringIds_AreMapped()
    {
        var connection = new FakeDbConnection();
        connection.Rows.Add(Row("DK", "Denmark"));
        connection.Rows.Add(Row("SE", "Sweden"));

        var result = CreateRepo(connection).GetFKeyOptions(new FKeyInfo("FK", "dbo.T", "Code", "Name"));

        Assert.Equal(["DK", "SE"], result.Select(o => (string)o.Id!));
    }

    [Fact]
    public void GetFKeyOptions_ReturnsNewListOnEachCall()
    {
        var connection = new FakeDbConnection();
        connection.Rows.Add(Row(1, "One"));
        var repo = CreateRepo(connection);
        var info = new FKeyInfo("FK", "dbo.T", "Id", "Name");

        var first = repo.GetFKeyOptions(info);
        var second = repo.GetFKeyOptions(info);

        Assert.NotSame(first, second);
        Assert.Equal(2, connection.Commands.Count);
    }

    [Fact]
    public void GetFKeyOptions_ExecutesNoParameters()
    {
        var connection = new FakeDbConnection();

        CreateRepo(connection).GetFKeyOptions(new FKeyInfo("FK", "dbo.T", "Id", "Name"));

        Assert.Empty(connection.Commands[0].Parameters);
    }
}
