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

        public Task<List<CruddyField>> GetColumnsAsync(Dictionary<string, string> columnAliasDict, CancellationToken cancellationToken = default) =>
            Task.FromResult<List<CruddyField>>([new CruddyField("PersonId", "PersonId", typeof(int), false, true, false)]);

        public Task<List<CruddyRow>> GetAllAsync(string sql, CancellationToken cancellationToken = default)
        {
            SelectSql = sql;
            return Task.FromResult<List<CruddyRow>>([]);
        }

        public Task<object?> DeleteAsync(object keyValue, CancellationToken cancellationToken = default)
        {
            DeletedKey = keyValue;
            return Task.FromResult<object?>(7);
        }

        public Task<CruddyRow?> GetByIdAsync(object id, string columns = "*", CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<object?> AddAsync(CruddyRow row, IReadOnlyList<CruddyField> fields, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<int> UpdateAsync(CruddyRow row, object keyValue, IReadOnlyList<CruddyField> fields, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<List<FKeyOption>> GetFKeyOptionsAsync(FKeyInfo info, CancellationToken cancellationToken = default) => throw new NotSupportedException();
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
