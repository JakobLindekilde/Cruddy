using Lindekilde.Blazor.Cruddy.Components;
using Lindekilde.Blazor.Cruddy.Repositories;
using Lindekilde.Blazor.Cruddy.Tests.Components;

namespace Lindekilde.Blazor.Cruddy.Tests.Repositories;

/// <summary>
/// Integration tests for <see cref="DapperRepository"/> against a fake repository.
/// </summary>
public class DapperRepositoryFakeRepoTests
{

    private static DapperRepository CreateFakeRepo(FakeDbConnection connection) =>
        new(connection, "dbo", "People", "PersonId");

    private static CruddyField Field(string column, Type type, bool nullable = false, bool isKey = false) =>
        new(column, column, type, nullable, isKey, false, false);

    private static Dictionary<string, object?> ColumnRow(string name, string typeName, bool isNullable, int isPrimaryKey, bool isComputed) =>
        new()
        {
            ["Name"] = name,
            ["TypeName"] = typeName,
            ["IsNullable"] = isNullable,
            ["IsPrimaryKey"] = isPrimaryKey,
            ["IsComputed"] = isComputed,
        };

    [Fact]
    public void GetColumns_PassesSchemaAndTableAsParameters()
    {
        var connection = new FakeDbConnection();

        new DapperRepository(connection, "mySchema", "myTable", "Id").GetColumns([]);

        var command = Assert.Single(connection.Commands);
        Assert.Equal("Reader", command.Kind);
        Assert.Equal("mySchema", command.Parameters["Schema"]);
        Assert.Equal("myTable", command.Parameters["Table"]);
    }

    [Fact]
    public void GetColumns_SkipsColumnsOfUnsupportedType()
    {
        var connection = new FakeDbConnection();
        connection.Rows.Add(ColumnRow("Id", "int", false, 1, false));
        connection.Rows.Add(ColumnRow("Blob", "varbinary", true, 0, false));
        connection.Rows.Add(ColumnRow("Name", "nvarchar", true, 0, false));
        connection.Rows.Add(ColumnRow("Stamp", "timestamp", false, 0, false));

        var columns = CreateFakeRepo(connection).GetColumns([]);

        Assert.Equal(["Id", "Name"], columns.Select(f => f.ColumnName));
    }

    [Fact]
    public void GetColumns_MapsFlagsFromRow()
    {
        var connection = new FakeDbConnection();
        connection.Rows.Add(ColumnRow("Id", "int", false, 1, false));
        connection.Rows.Add(ColumnRow("Born", "date", true, 0, false));
        connection.Rows.Add(ColumnRow("Total", "int", false, 0, true));

        var columns = CreateFakeRepo(connection).GetColumns([]);

        var id = columns[0];
        Assert.True(id.IsPrimaryKey);
        Assert.False(id.IsNullable);
        Assert.False(id.IsReadOnly);
        Assert.False(id.IsDate);

        var born = columns[1];
        Assert.Equal(typeof(DateTime), born.Type);
        Assert.True(born.IsDate);
        Assert.True(born.IsNullable);
        Assert.False(born.IsPrimaryKey);

        var total = columns[2];
        Assert.True(total.IsReadOnly);
        Assert.False(total.IsPrimaryKey);
    }

    [Fact]
    public void GetColumns_NoRows_ReturnsEmptyList()
    {
        var columns = CreateFakeRepo(new FakeDbConnection()).GetColumns([]);

        Assert.Empty(columns);
    }

    [Fact]
    public void Add_NoFields_ExecutesNoCommand()
    {
        var connection = new FakeDbConnection();

        CreateFakeRepo(connection).Add(new CruddyRow(), []);

        Assert.Empty(connection.Commands);
    }

    [Fact]
    public void Add_BuildsInsertSqlWithBracketedColumnsAndParameters()
    {
        var connection = new FakeDbConnection { ScalarResult = 7m };
        CruddyField[] fields = [Field("First Name", typeof(string)), Field("Age", typeof(int))];
        var row = new CruddyRow { ["First Name"] = "Ann", ["Age"] = 30 };

        var result = CreateFakeRepo(connection).Add(row, fields);

        Assert.Equal(7m, result);
        var command = Assert.Single(connection.Commands);
        Assert.Equal("Scalar", command.Kind);
        Assert.Equal(
            "INSERT INTO dbo.People ([First Name], [Age]) VALUES (@p0, @p1); SELECT SCOPE_IDENTITY();",
            command.Sql);
        Assert.Equal("Ann", command.Parameters["p0"]);
        Assert.Equal(30, command.Parameters["p1"]);
    }

    [Fact]
    public void Add_WhenIdentityQueryFails_FallsBackToRowsAffected()
    {
        var connection = new FakeDbConnection { ThrowOnScalar = true, NonQueryResult = 1 };
        CruddyField[] fields = [Field("Age", typeof(int))];

        var result = CreateFakeRepo(connection).Add(new CruddyRow { ["Age"] = 30 }, fields);

        Assert.Equal(1, result);
        Assert.Equal(["Scalar", "NonQuery"], connection.Commands.Select(cmd => cmd.Kind));
        Assert.Equal("INSERT INTO dbo.People ([Age]) VALUES (@p0)", connection.Commands[1].Sql);
        Assert.Equal(30, connection.Commands[1].Parameters["p0"]);
    }

    [Fact]
    public void GetById_BuildsExpectedSql()
    {
        var connection = new FakeDbConnection();
        connection.Rows.Add(new Dictionary<string, object?> { ["PersonId"] = 5 });

        var result = CreateFakeRepo(connection).GetById(5, "PersonId");

        Assert.NotNull(result);
        var command = Assert.Single(connection.Commands);
        Assert.Equal("SELECT PersonId FROM dbo.People WHERE PersonId = @Id", command.Sql);
        Assert.Equal(5, command.Parameters["Id"]);
    }

    [Fact]
    public void GetById_DefaultsToAllColumns()
    {
        var connection = new FakeDbConnection();

        var result = CreateFakeRepo(connection).GetById(1);

        Assert.Null(result);
        Assert.Equal("SELECT * FROM dbo.People WHERE PersonId = @Id", connection.Commands[0].Sql);
    }

    [Fact]
    public void Update_NoFields_ExecutesNoCommand()
    {
        var connection = new FakeDbConnection();

        var affected = CreateFakeRepo(connection).Update(new CruddyRow(), 1, []);

        Assert.Equal(0, affected);
        Assert.Empty(connection.Commands);
    }

    [Fact]
    public void Update_BuildsUpdateSqlWithKeyParameter()
    {
        var connection = new FakeDbConnection { NonQueryResult = 3 };
        CruddyField[] fields = [Field("First Name", typeof(string)), Field("Age", typeof(int))];
        var row = new CruddyRow { ["First Name"] = "Bob", ["Age"] = 41 };

        var affected = CreateFakeRepo(connection).Update(row, 99, fields);

        Assert.Equal(3, affected);
        var command = Assert.Single(connection.Commands);
        Assert.Equal("NonQuery", command.Kind);
        Assert.Equal("UPDATE dbo.People SET [First Name] = @p0, [Age] = @p1 WHERE PersonId = @keyValue", command.Sql);
        Assert.Equal("Bob", command.Parameters["p0"]);
        Assert.Equal(41, command.Parameters["p1"]);
        Assert.Equal(99, command.Parameters["keyValue"]);
    }

    [Fact]
    public void Delete_BuildsExpectedSql()
    {
        var connection = new FakeDbConnection();

        CreateFakeRepo(connection).Delete(12);

        var command = Assert.Single(connection.Commands);
        Assert.Equal("Scalar", command.Kind);
        Assert.Equal("DELETE FROM dbo.People WHERE PersonId = @keyValue", command.Sql);
        Assert.Equal(12, command.Parameters["keyValue"]);
    }
}
