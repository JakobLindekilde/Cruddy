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
        new(column, column, type, nullable, isKey, false);

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
    public async Task GetColumns_PassesSchemaAndTableAsParameters()
    {
        var connection = new FakeDbConnection();

        await new DapperRepository(connection, "mySchema", "myTable", "Id").GetColumnsAsync([]);

        var command = Assert.Single(connection.Commands);
        Assert.Equal("Reader", command.Kind);
        Assert.Equal("mySchema", command.Parameters["Schema"]);
        Assert.Equal("myTable", command.Parameters["Table"]);
    }

    [Fact]
    public async Task GetColumns_SkipsColumnsOfUnsupportedType()
    {
        var connection = new FakeDbConnection();
        connection.Rows.Add(ColumnRow("Id", "int", false, 1, false));
        connection.Rows.Add(ColumnRow("Blob", "varbinary", true, 0, false));
        connection.Rows.Add(ColumnRow("Name", "nvarchar", true, 0, false));
        connection.Rows.Add(ColumnRow("Stamp", "timestamp", false, 0, false));

        var columns = await CreateFakeRepo(connection).GetColumnsAsync([]);

        Assert.Equal(["Id", "Name"], columns.Select(f => f.ColumnName));
    }

    [Fact]
    public async Task GetColumns_MapsFlagsFromRow()
    {
        var connection = new FakeDbConnection();
        connection.Rows.Add(ColumnRow("Id", "int", false, 1, false));
        connection.Rows.Add(ColumnRow("Born", "date", true, 0, false));
        connection.Rows.Add(ColumnRow("Total", "int", false, 0, true));

        var columns = await CreateFakeRepo(connection).GetColumnsAsync([]);

        var id = columns[0];
        Assert.True(id.IsPrimaryKey);
        Assert.False(id.IsNullable);
        Assert.False(id.IsReadOnly);

        var born = columns[1];
        Assert.Equal(typeof(DateOnly), born.Type);
        Assert.True(born.IsNullable);
        Assert.False(born.IsPrimaryKey);

        var total = columns[2];
        Assert.True(total.IsReadOnly);
        Assert.False(total.IsPrimaryKey);
    }

    [Fact]
    public async Task GetColumns_NoRows_ReturnsEmptyList()
    {
        var columns = await CreateFakeRepo(new FakeDbConnection()).GetColumnsAsync([]);

        Assert.Empty(columns);
    }

    [Fact]
    public async Task Add_NoFields_ExecutesNoCommand()
    {
        var connection = new FakeDbConnection();

        await CreateFakeRepo(connection).AddAsync(new CruddyRow(), []);

        Assert.Empty(connection.Commands);
    }

    [Fact]
    public async Task Add_BuildsInsertSqlWithBracketedColumnsAndParameters()
    {
        var connection = new FakeDbConnection { ScalarResult = 7m };
        CruddyField[] fields = [Field("First Name", typeof(string)), Field("Age", typeof(int))];
        var row = new CruddyRow { ["First Name"] = "Ann", ["Age"] = 30 };

        var result = await CreateFakeRepo(connection).AddAsync(row, fields);

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
    public async Task Add_WhenIdentityQueryFails_FallsBackToRowsAffected()
    {
        var connection = new FakeDbConnection { ThrowOnScalar = true, NonQueryResult = 1 };
        CruddyField[] fields = [Field("Age", typeof(int))];

        var result = await CreateFakeRepo(connection).AddAsync(new CruddyRow { ["Age"] = 30 }, fields);

        Assert.Equal(1, result);
        Assert.Equal(["Scalar", "NonQuery"], connection.Commands.Select(cmd => cmd.Kind));
        Assert.Equal("INSERT INTO dbo.People ([Age]) VALUES (@p0)", connection.Commands[1].Sql);
        Assert.Equal(30, connection.Commands[1].Parameters["p0"]);
    }

    [Fact]
    public async Task GetById_BuildsExpectedSql()
    {
        var connection = new FakeDbConnection();
        connection.Rows.Add(new Dictionary<string, object?> { ["PersonId"] = 5 });

        var result = await CreateFakeRepo(connection).GetByIdAsync(5, "PersonId");

        Assert.NotNull(result);
        var command = Assert.Single(connection.Commands);
        Assert.Equal("SELECT PersonId FROM dbo.People WHERE PersonId = @Id", command.Sql);
        Assert.Equal(5, command.Parameters["Id"]);
    }

    [Fact]
    public async Task GetById_DefaultsToAllColumns()
    {
        var connection = new FakeDbConnection();

        var result = await CreateFakeRepo(connection).GetByIdAsync(1);

        Assert.Null(result);
        Assert.Equal("SELECT * FROM dbo.People WHERE PersonId = @Id", connection.Commands[0].Sql);
    }

    [Fact]
    public async Task Update_NoFields_ExecutesNoCommand()
    {
        var connection = new FakeDbConnection();

        var affected = await CreateFakeRepo(connection).UpdateAsync(new CruddyRow(), 1, []);

        Assert.Equal(0, affected);
        Assert.Empty(connection.Commands);
    }

    [Fact]
    public async Task Update_BuildsUpdateSqlWithKeyParameter()
    {
        var connection = new FakeDbConnection { NonQueryResult = 3 };
        CruddyField[] fields = [Field("First Name", typeof(string)), Field("Age", typeof(int))];
        var row = new CruddyRow { ["First Name"] = "Bob", ["Age"] = 41 };

        var affected = await CreateFakeRepo(connection).UpdateAsync(row, 99, fields);

        Assert.Equal(3, affected);
        var command = Assert.Single(connection.Commands);
        Assert.Equal("NonQuery", command.Kind);
        Assert.Equal("UPDATE dbo.People SET [First Name] = @p0, [Age] = @p1 WHERE PersonId = @keyValue", command.Sql);
        Assert.Equal("Bob", command.Parameters["p0"]);
        Assert.Equal(41, command.Parameters["p1"]);
        Assert.Equal(99, command.Parameters["keyValue"]);
    }

    [Fact]
    public async Task Delete_BuildsExpectedSql()
    {
        var connection = new FakeDbConnection();

        await CreateFakeRepo(connection).DeleteAsync(12);

        var command = Assert.Single(connection.Commands);
        Assert.Equal("Scalar", command.Kind);
        Assert.Equal("DELETE FROM dbo.People WHERE PersonId = @keyValue", command.Sql);
        Assert.Equal(12, command.Parameters["keyValue"]);
    }

    private static Task ExecuteOperationAsync(DapperRepository repository, string operation, CancellationToken token)
    {
        CruddyField[] fields = [Field("Age", typeof(int))];
        var row = new CruddyRow { ["Age"] = 30 };
        return operation switch
        {
            "Columns" => repository.GetColumnsAsync([], token),
            "All" => repository.GetAllAsync("SELECT * FROM dbo.People", token),
            "ById" => repository.GetByIdAsync(1, "*", token),
            "Add" => repository.AddAsync(row, fields, token),
            "Update" => repository.UpdateAsync(row, 1, fields, token),
            "Delete" => repository.DeleteAsync(1, token),
            "ForeignKeys" => repository.GetFKeyOptionsAsync(new FKeyInfo("FK", "dbo.T", "Id", "Name"), token),
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };
    }

    [Theory]
    [InlineData("Columns", "Reader")]
    [InlineData("All", "Reader")]
    [InlineData("ById", "Reader")]
    [InlineData("Add", "Scalar")]
    [InlineData("Update", "NonQuery")]
    [InlineData("Delete", "Scalar")]
    [InlineData("ForeignKeys", "Reader")]
    public async Task RepositoryOperations_UseAsyncCommandsAndForwardCancellation(string operation, string kind)
    {
        var connection = new FakeDbConnection();
        using var source = new CancellationTokenSource();

        await ExecuteOperationAsync(CreateFakeRepo(connection), operation, source.Token);

        var call = Assert.Single(connection.AsyncCommands);
        Assert.Equal(kind, call.Kind);
        Assert.Equal(source.Token, call.Token);
        Assert.Equal(kind, Assert.Single(connection.Commands).Kind);
    }

    [Theory]
    [InlineData("Columns")]
    [InlineData("All")]
    [InlineData("ById")]
    [InlineData("Add")]
    [InlineData("Update")]
    [InlineData("Delete")]
    [InlineData("ForeignKeys")]
    public async Task RepositoryOperations_CancelBeforeCommandExecution(string operation)
    {
        var connection = new FakeDbConnection();
        using var source = new CancellationTokenSource();
        source.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ExecuteOperationAsync(CreateFakeRepo(connection), operation, source.Token));

        var call = Assert.Single(connection.AsyncCommands);
        Assert.Equal(source.Token, call.Token);
        Assert.Empty(connection.Commands);
    }

    [Fact]
    public async Task Add_IdentityFailure_FallbackUsesAsyncCommandAndToken()
    {
        var connection = new FakeDbConnection { ThrowOnScalar = true };
        using var source = new CancellationTokenSource();

        await ExecuteOperationAsync(CreateFakeRepo(connection), "Add", source.Token);

        Assert.Equal(["Scalar", "NonQuery"], connection.AsyncCommands.Select(call => call.Kind));
        Assert.All(connection.AsyncCommands, call => Assert.Equal(source.Token, call.Token));
    }
}
