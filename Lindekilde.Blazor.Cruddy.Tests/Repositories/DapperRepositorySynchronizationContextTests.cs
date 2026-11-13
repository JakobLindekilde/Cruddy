using System.Collections.Concurrent;
using Lindekilde.Blazor.Cruddy.Components;
using Lindekilde.Blazor.Cruddy.Repositories;
using Lindekilde.Blazor.Cruddy.Tests.Components;

namespace Lindekilde.Blazor.Cruddy.Tests.Repositories;

public class DapperRepositorySynchronizationContextTests
{
    private sealed class TestCruddy : CruddyBase
    {
        public Task InitializeAsync() => OnInitializedAsync();
        public IReadOnlyList<CruddyRow> LoadedRows => Rows;
    }

    private sealed class BlockedSynchronizationContext : SynchronizationContext
    {
        private readonly ConcurrentQueue<(SendOrPostCallback Callback, object? State)> _callbacks = new();
        private bool _released;
        private readonly object _lock = new();

        public override void Post(SendOrPostCallback callback, object? state)
        {
            lock (_lock)
            {
                if (!_released)
                {
                    _callbacks.Enqueue((callback, state));
                    return;
                }
            }
            ThreadPool.QueueUserWorkItem(_ => callback(state));
        }

        public void Release()
        {
            lock (_lock)
            {
                _released = true;
                while (_callbacks.TryDequeue(out var item))
                {
                    ThreadPool.QueueUserWorkItem(_ => item.Callback(item.State));
                }
            }
        }
    }

    [Theory]
    [InlineData("Columns")]
    [InlineData("All")]
    [InlineData("ById")]
    [InlineData("ForeignKeys")]
    [InlineData("Add")]
    [InlineData("AddFallback")]
    [InlineData("Update")]
    [InlineData("Delete")]
    public async Task Result_CompletesWhenDatabaseOperationSuspendsOnSingleThreadContext(string operation)
    {
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var connection = new FakeDbConnection { CommandGate = gate.Task, CommandStarted = started, ThrowOnScalar = operation == "AddFallback" };
        var context = new BlockedSynchronizationContext();
        var worker = Task.Run(() =>
        {
            SynchronizationContext.SetSynchronizationContext(context);
            try
            {
                var repo = new DapperRepository(connection, "dbo", "People", "Id");
                CruddyField[] fields = [new CruddyField("Age", "Age", typeof(int), false, false, false)];
                var row = new CruddyRow { ["Age"] = 30 };
#pragma warning disable xUnit1031 // This test deliberately reproduces synchronous Blazor callers.
                return operation switch
                {
                    "Columns" => (object?)repo.GetColumnsAsync([]).Result,
                    "All" => repo.GetAllAsync("SELECT * FROM dbo.People").Result,
                    "ById" => repo.GetByIdAsync(1).Result,
                    "ForeignKeys" => repo.GetFKeyOptionsAsync(new FKeyInfo("FK", "dbo.People", "Id", "Name")).Result,
                    "Add" or "AddFallback" => repo.AddAsync(row, fields).Result,
                    "Update" => repo.UpdateAsync(row, 1, fields).Result,
                    "Delete" => repo.DeleteAsync(1).Result,
                    _ => throw new ArgumentOutOfRangeException(nameof(operation))
                };
#pragma warning restore xUnit1031
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(null);
            }
        });

        try
        {
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(worker.IsCompleted);
            gate.SetResult();
            var result = await worker.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal(operation == "AddFallback" ? 2 : 1, connection.Commands.Count);
            if (operation is "Add" or "Delete") Assert.Equal(42, result);
            if (operation is "Update" or "AddFallback") Assert.Equal(1, result);
        }
        finally
        {
            gate.TrySetResult();
            context.Release();
        }
    }

    [Fact]
    public async Task ComponentInitialization_LoadsRowsWhenCatalogQuerySuspends()
    {
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var connection = new FakeDbConnection { CommandGate = gate.Task, CommandStarted = started };
        connection.Rows.Add(new Dictionary<string, object?>
        {
            ["Name"] = "Id", ["TypeName"] = "int", ["IsNullable"] = false,
            ["IsPrimaryKey"] = 1, ["IsComputed"] = false
        });
        var context = new BlockedSynchronizationContext();
#pragma warning disable BL0005 // Component parameters are set directly for this test.
        var component = new TestCruddy { DbConnection = connection, TableName = "People", RepositoryFactory = new DapperRepositoryFactory() };
#pragma warning restore BL0005
        var worker = Task.Run(() =>
        {
            SynchronizationContext.SetSynchronizationContext(context);
            try
            {
                return component.InitializeAsync();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(null);
            }
        });

        try
        {
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(worker.IsCompleted);
            gate.SetResult();
            await worker.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal("Id", component.KeyColumn);
            Assert.Equal("Id", Assert.Single(component.Fields).ColumnName);
            Assert.Equal("Id", Assert.Single(component.LoadedRows)["Name"]);
            Assert.Equal(2, connection.Commands.Count);
        }
        finally
        {
            gate.TrySetResult();
            context.Release();
        }
    }
}
