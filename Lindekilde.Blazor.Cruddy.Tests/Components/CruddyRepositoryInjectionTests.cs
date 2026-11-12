#pragma warning disable BL0005 // Component parameters are set directly for these tests.

using System.Data.Common;
using Lindekilde.Blazor.Cruddy.Components;
using Lindekilde.Blazor.Cruddy.Repositories;

namespace Lindekilde.Blazor.Cruddy.Tests.Components;

public class CruddyRepositoryInjectionTests
{
    private sealed class TestCruddy : CruddyBase
    {
        public Task InitializeAsync() => OnInitializedAsync();
    }

    private sealed class RecordingFactory : IRepositoryFactory
    {
        public List<(DbConnection Connection, string Schema, string Table, string? Key)> Calls { get; } = [];
        public RecordingRepository Repository { get; } = new();

        public IRepository Create(DbConnection dbConnection, string schema, string tableName, string? keyColumn)
        {
            Calls.Add((dbConnection, schema, tableName, keyColumn));
            return Repository;
        }
    }

    private sealed class RecordingRepository : IRepository
    {
        public string? SelectSql { get; private set; }
        public object? DeletedKey { get; private set; }

        public List<CruddyField> GetColumns(Dictionary<string, string> columnAliasDict) =>
            [new CruddyField("PersonId", "PersonId", typeof(int), false, true, false)];

        public List<CruddyRow> GetAll(string sql)
        {
            SelectSql = sql;
            return [];
        }

        public object? Delete(object keyValue)
        {
            DeletedKey = keyValue;
            return 7;
        }

        public CruddyRow? GetById(object id, string columns = "*") => throw new NotSupportedException();
        public object? Add(CruddyRow row, IReadOnlyList<CruddyField> fields) => throw new NotSupportedException();
        public int Update(CruddyRow row, object keyValue, IReadOnlyList<CruddyField> fields) => throw new NotSupportedException();
        public List<FKeyOption> GetFKeyOptions(FKeyInfo info) => throw new NotSupportedException();
    }

    [Fact]
    public async Task InjectedFactory_UsesComponentSettingsAndInferredKey()
    {
        var factory = new RecordingFactory();
        var connection = new FakeDbConnection();
        var component = new TestCruddy
        {
            DbConnection = connection,
            DefaultSchema = "sales",
            TableName = "People",
            RepositoryFactory = factory
        };

        await component.InitializeAsync();
        var result = component.Delete(42);

        Assert.Equal("PersonId", component.KeyColumn);
        Assert.Equal("SELECT TOP 10000  * FROM sales.People", factory.Repository.SelectSql);
        Assert.Equal(42, factory.Repository.DeletedKey);
        Assert.Equal(7, result);
        Assert.Equal(3, factory.Calls.Count);
        Assert.All(factory.Calls, call =>
        {
            Assert.Same(connection, call.Connection);
            Assert.Equal("sales", call.Schema);
            Assert.Equal("People", call.Table);
        });
        Assert.Null(factory.Calls[0].Key);
        Assert.Equal("PersonId", factory.Calls[1].Key);
        Assert.Equal("PersonId", factory.Calls[2].Key);
    }
}
