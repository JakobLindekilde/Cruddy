using System.Reflection;
using System.Text.Json;
using Cruddy.Repositories;
using Cruddy.Tests.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Cruddy.Tests.Repositories.DatabaseTests;

/// <summary>
/// Integration tests for <see cref="DapperRepository{TEntity}"/> against the CruddyDB database.
/// Every test removes the rows it inserted in a finally block.
/// </summary>
public class DapperRepositoryTests
{
    private static readonly string ConnectionString = LoadConnectionString();

    private static string LoadConnectionString()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        return doc.RootElement.GetProperty("ConnectionStrings").GetProperty("CruddyDB").GetString()!;
    }

    private static SqlConnection OpenConnection()
    {
        var connection = new SqlConnection(ConnectionString);
        connection.Open();
        return connection;
    }

    private static DapperRepository<TestAllType> AllTypesRepo(SqlConnection c) => new(c, "dbo", "TestAllTypes", "Id");
    private static DapperRepository<DateTimeType> DateTimeRepo(SqlConnection c) => new(c, "dbo", "DateTimeTypes", "Id");

    private static PropertyInfo[] PropsExceptId<T>() =>
        typeof(T).GetProperties().Where(p => p.Name != "Id").ToArray();

    private static TestAllType NewAllType(string? marker = null) => new()
    {
        Int32NotNull = 42,
        Int32Nullable = 43,
        Int64NotNull = 9_000_000_000L,
        Int64Nullable = 9_000_000_001L,
        Int16NotNull = 12,
        Int16Nullable = 13,
        ByteNotNull = 7,
        ByteNullable = 8,
        DecimalNotNull = 123.4567m,
        DecimalNullable = 765.4321m,
        DoubleNotNull = 3.14159,
        DoubleNullable = 2.71828,
        SingleNotNull = 1.5f,
        SingleNullable = 2.5f,
        BoolNotNull = true,
        BoolNullable = false,
        StringNotNull = marker ?? "UnitTest-" + Guid.NewGuid().ToString("N")[..20],
        StringNullable = "nullable text",
        GuidNotNull = Guid.NewGuid(),
        GuidNullable = Guid.NewGuid()
    };

    private static TestAllType NewAllTypeWithNulls() => new()
    {
        Int32NotNull = 1,
        Int64NotNull = 2,
        Int16NotNull = 3,
        ByteNotNull = 4,
        DecimalNotNull = 5m,
        DoubleNotNull = 6,
        SingleNotNull = 7,
        BoolNotNull = false,
        StringNotNull = "UnitTest-" + Guid.NewGuid().ToString("N")[..20],
        GuidNotNull = Guid.NewGuid()
    };

    private static DateTimeType NewDateTimeType() => new()
    {
        DateTimeNotNull = new DateTime(2024, 5, 6, 7, 8, 9),
        DateTimeNullable = new DateTime(2025, 1, 2, 3, 4, 5),
        TimeSpanNotNull = new TimeSpan(1, 2, 3),
        TimeSpanNullable = new TimeSpan(4, 5, 6),
        TimeOnlyNotNull = new TimeOnly(10, 11, 12),
        TimeOnlyNullable = new TimeOnly(13, 14, 15)
    };

    private static int Insert<T>(DapperRepository<T> repo, T entity)
    {
        var id = repo.Add(entity, PropsExceptId<T>());
        Assert.NotNull(id);
        return Convert.ToInt32(id);
    }

    private static void DeleteRow(SqlConnection c, string table, int id) =>
        c.Execute($"DELETE FROM dbo.{table} WHERE Id = @id", new { id });

    // ---------- Add ----------

    [Fact]
    public void Add_TestAllType_ReturnsIdentityAndPersistsAllValues()
    {
        using var c = OpenConnection();
        var repo = AllTypesRepo(c);
        var entity = NewAllType();
        int id = 0;
        try
        {
            id = Insert(repo, entity);
            Assert.True(id > 0);

            var actual = repo.GetById(id);
            Assert.NotNull(actual);
            Assert.Equal(id, actual.Id);
            Assert.Equal(entity.Int32NotNull, actual.Int32NotNull);
            Assert.Equal(entity.Int32Nullable, actual.Int32Nullable);
            Assert.Equal(entity.Int64NotNull, actual.Int64NotNull);
            Assert.Equal(entity.Int64Nullable, actual.Int64Nullable);
            Assert.Equal(entity.Int16NotNull, actual.Int16NotNull);
            Assert.Equal(entity.Int16Nullable, actual.Int16Nullable);
            Assert.Equal(entity.ByteNotNull, actual.ByteNotNull);
            Assert.Equal(entity.ByteNullable, actual.ByteNullable);
            Assert.Equal(entity.DecimalNotNull, actual.DecimalNotNull);
            Assert.Equal(entity.DecimalNullable, actual.DecimalNullable);
            Assert.Equal(entity.DoubleNotNull, actual.DoubleNotNull, 6);
            Assert.Equal(entity.DoubleNullable!.Value, actual.DoubleNullable!.Value, 6);
            Assert.Equal(entity.SingleNotNull, actual.SingleNotNull);
            Assert.Equal(entity.SingleNullable, actual.SingleNullable);
            Assert.Equal(entity.BoolNotNull, actual.BoolNotNull);
            Assert.Equal(entity.BoolNullable, actual.BoolNullable);
            Assert.Equal(entity.StringNotNull, actual.StringNotNull);
            Assert.Equal(entity.StringNullable, actual.StringNullable);
            Assert.Equal(entity.GuidNotNull, actual.GuidNotNull);
            Assert.Equal(entity.GuidNullable, actual.GuidNullable);
        }
        finally
        {
            if (id > 0) DeleteRow(c, "TestAllTypes", id);
        }
    }

    [Fact]
    public void Add_TestAllType_NullValues_ArePersistedAsNull()
    {
        using var c = OpenConnection();
        var repo = AllTypesRepo(c);
        int id = 0;
        try
        {
            id = Insert(repo, NewAllTypeWithNulls());

            var actual = repo.GetById(id);
            Assert.NotNull(actual);
            Assert.Null(actual.Int32Nullable);
            Assert.Null(actual.Int64Nullable);
            Assert.Null(actual.Int16Nullable);
            Assert.Null(actual.ByteNullable);
            Assert.Null(actual.DecimalNullable);
            Assert.Null(actual.DoubleNullable);
            Assert.Null(actual.SingleNullable);
            Assert.Null(actual.BoolNullable);
            Assert.Null(actual.StringNullable);
            Assert.Null(actual.GuidNullable);
        }
        finally
        {
            if (id > 0) DeleteRow(c, "TestAllTypes", id);
        }
    }

    [Fact]
    public void Add_DateTimeType_ReturnsIdentityAndPersistsValues()
    {
        using var c = OpenConnection();
        var repo = DateTimeRepo(c);
        var entity = NewDateTimeType();
        int id = 0;
        try
        {
            id = Insert(repo, entity);
            Assert.True(id > 0);

            var actual = repo.GetById(id);
            Assert.NotNull(actual);
            Assert.Equal(entity.DateTimeNotNull, actual.DateTimeNotNull);
            Assert.Equal(entity.DateTimeNullable, actual.DateTimeNullable);
            Assert.Equal(entity.TimeSpanNotNull, actual.TimeSpanNotNull);
            Assert.Equal(entity.TimeSpanNullable, actual.TimeSpanNullable);
            Assert.Equal(entity.TimeOnlyNotNull, actual.TimeOnlyNotNull);
            Assert.Equal(entity.TimeOnlyNullable, actual.TimeOnlyNullable);
        }
        finally
        {
            if (id > 0) DeleteRow(c, "DateTimeTypes", id);
        }
    }

    [Fact]
    public void Add_DateTimeType_NullableValuesNull_ArePersistedAsNull()
    {
        using var c = OpenConnection();
        var repo = DateTimeRepo(c);
        var entity = NewDateTimeType();
        entity.DateTimeNullable = null;
        entity.TimeSpanNullable = null;
        entity.TimeOnlyNullable = null;
        int id = 0;
        try
        {
            id = Insert(repo, entity);

            var actual = repo.GetById(id);
            Assert.NotNull(actual);
            Assert.Null(actual.DateTimeNullable);
            Assert.Null(actual.TimeSpanNullable);
            Assert.Null(actual.TimeOnlyNullable);
            Assert.Equal(entity.DateTimeNotNull, actual.DateTimeNotNull);
        }
        finally
        {
            if (id > 0) DeleteRow(c, "DateTimeTypes", id);
        }
    }

    [Fact]
    public void Add_OnlySpecifiedProperties_AreInserted()
    {
        using var c = OpenConnection();
        var repo = DateTimeRepo(c);
        var entity = NewDateTimeType();
        var props = new[]
        {
            typeof(DateTimeType).GetProperty(nameof(DateTimeType.DateTimeNotNull))!,
            typeof(DateTimeType).GetProperty(nameof(DateTimeType.TimeSpanNotNull))!,
            typeof(DateTimeType).GetProperty(nameof(DateTimeType.TimeOnlyNotNull))!
        };
        int id = 0;
        try
        {
            id = Convert.ToInt32(repo.Add(entity, props));

            var actual = repo.GetById(id);
            Assert.NotNull(actual);
            Assert.Equal(entity.DateTimeNotNull, actual.DateTimeNotNull);
            Assert.Null(actual.DateTimeNullable);
        }
        finally
        {
            if (id > 0) DeleteRow(c, "DateTimeTypes", id);
        }
    }

    [Fact]
    public void Add_NullEntity_ThrowsArgumentNullException()
    {
        using var c = OpenConnection();
        var repo = DateTimeRepo(c);

        Assert.Throws<ArgumentNullException>(() => repo.Add(null!, PropsExceptId<DateTimeType>()));
    }

    [Fact]
    public void Add_NoProperties_ReturnsZeroAndInsertsNothing()
    {
        using var c = OpenConnection();
        var repo = AllTypesRepo(c);
        var entity = NewAllType();

        var before = c.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.TestAllTypes");
        var result = repo.Add(entity, []);
        var after = c.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.TestAllTypes");

        Assert.Equal(0, result);
        Assert.Equal(before, after);
    }

    [Fact]
    public void Add_MultipleRows_ReturnsIncreasingIdentities()
    {
        using var c = OpenConnection();
        var repo = AllTypesRepo(c);
        int id1 = 0, id2 = 0;
        try
        {
            id1 = Insert(repo, NewAllType());
            id2 = Insert(repo, NewAllType());

            Assert.True(id2 > id1);
        }
        finally
        {
            if (id1 > 0) DeleteRow(c, "TestAllTypes", id1);
            if (id2 > 0) DeleteRow(c, "TestAllTypes", id2);
        }
    }

    [Fact]
    public void Add_StringNotNullTooLong_ThrowsSqlException()
    {
        using var c = OpenConnection();
        var repo = AllTypesRepo(c);
        var entity = NewAllType(new string('x', 200));
        int id = 0;
        try
        {
            Assert.ThrowsAny<Exception>(() => id = Insert(repo, entity));
        }
        finally
        {
            if (id > 0) DeleteRow(c, "TestAllTypes", id);
            c.Execute("DELETE FROM dbo.TestAllTypes WHERE StringNotNull = @s", new { s = entity.StringNotNull });
        }
    }

    // ---------- GetById ----------

    [Fact]
    public void GetById_ExistingRow_ReturnsEntity()
    {
        using var c = OpenConnection();
        var repo = AllTypesRepo(c);
        var entity = NewAllType();
        int id = 0;
        try
        {
            id = Insert(repo, entity);

            var actual = repo.GetById(id);

            Assert.NotNull(actual);
            Assert.Equal(id, actual.Id);
            Assert.Equal(entity.StringNotNull, actual.StringNotNull);
        }
        finally
        {
            if (id > 0) DeleteRow(c, "TestAllTypes", id);
        }
    }

    [Fact]
    public void GetById_NonExistingId_ReturnsNull()
    {
        using var c = OpenConnection();
        var repo = AllTypesRepo(c);

        Assert.Null(repo.GetById(-1));
    }

    [Fact]
    public void GetById_SpecificColumns_OnlyThoseColumnsAreFilled()
    {
        using var c = OpenConnection();
        var repo = AllTypesRepo(c);
        var entity = NewAllType();
        int id = 0;
        try
        {
            id = Insert(repo, entity);

            var actual = repo.GetById(id, "Id, StringNotNull");

            Assert.NotNull(actual);
            Assert.Equal(id, actual.Id);
            Assert.Equal(entity.StringNotNull, actual.StringNotNull);
            Assert.Equal(0, actual.Int32NotNull);
            Assert.Null(actual.StringNullable);
        }
        finally
        {
            if (id > 0) DeleteRow(c, "TestAllTypes", id);
        }
    }

    [Fact]
    public void GetById_DateTimeType_ReturnsEntity()
    {
        using var c = OpenConnection();
        var repo = DateTimeRepo(c);
        int id = 0;
        try
        {
            id = Insert(repo, NewDateTimeType());

            var actual = repo.GetById(id);

            Assert.NotNull(actual);
            Assert.Equal(id, actual.Id);
        }
        finally
        {
            if (id > 0) DeleteRow(c, "DateTimeTypes", id);
        }
    }

    [Fact]
    public void GetById_InvalidColumn_ThrowsSqlException()
    {
        using var c = OpenConnection();
        var repo = AllTypesRepo(c);

        Assert.Throws<SqlException>(() => repo.GetById(1, "NoSuchColumn"));
    }

    // ---------- GetAll ----------

    [Fact]
    public void GetAll_ReturnsInsertedRows()
    {
        using var c = OpenConnection();
        var repo = AllTypesRepo(c);
        var e1 = NewAllType();
        var e2 = NewAllType();
        int id1 = 0, id2 = 0;
        try
        {
            id1 = Insert(repo, e1);
            id2 = Insert(repo, e2);

            var all = repo.GetAll("SELECT * FROM dbo.TestAllTypes");

            Assert.Contains(all, x => x.Id == id1 && x.StringNotNull == e1.StringNotNull);
            Assert.Contains(all, x => x.Id == id2 && x.StringNotNull == e2.StringNotNull);
        }
        finally
        {
            if (id1 > 0) DeleteRow(c, "TestAllTypes", id1);
            if (id2 > 0) DeleteRow(c, "TestAllTypes", id2);
        }
    }

    [Fact]
    public void GetAll_WithWhereClause_FiltersRows()
    {
        using var c = OpenConnection();
        var repo = AllTypesRepo(c);
        var e1 = NewAllType();
        var e2 = NewAllType();
        int id1 = 0, id2 = 0;
        try
        {
            id1 = Insert(repo, e1);
            id2 = Insert(repo, e2);

            var result = repo.GetAll($"SELECT * FROM dbo.TestAllTypes WHERE Id = {id1}");

            var single = Assert.Single(result);
            Assert.Equal(id1, single.Id);
        }
        finally
        {
            if (id1 > 0) DeleteRow(c, "TestAllTypes", id1);
            if (id2 > 0) DeleteRow(c, "TestAllTypes", id2);
        }
    }

    [Fact]
    public void GetAll_NoMatch_ReturnsEmptyList()
    {
        using var c = OpenConnection();
        var repo = AllTypesRepo(c);

        var result = repo.GetAll("SELECT * FROM dbo.TestAllTypes WHERE Id = -1");

        Assert.Empty(result);
    }

    [Fact]
    public void GetAll_DateTimeType_ReturnsInsertedRow()
    {
        using var c = OpenConnection();
        var repo = DateTimeRepo(c);
        int id = 0;
        try
        {
            id = Insert(repo, NewDateTimeType());

            var all = repo.GetAll("SELECT * FROM dbo.DateTimeTypes");

            Assert.Contains(all, x => x.Id == id);
        }
        finally
        {
            if (id > 0) DeleteRow(c, "DateTimeTypes", id);
        }
    }

    [Fact]
    public void GetAll_InvalidSql_ThrowsSqlException()
    {
        using var c = OpenConnection();
        var repo = AllTypesRepo(c);

        Assert.Throws<SqlException>(() => repo.GetAll("SELECT * FROM dbo.NoSuchTable"));
    }

    // ---------- Update ----------

    [Fact]
    public void Update_AllProperties_UpdatesRowAndReturnsOne()
    {
        using var c = OpenConnection();
        var repo = AllTypesRepo(c);
        int id = 0;
        try
        {
            id = Insert(repo, NewAllType());
            var updated = NewAllType();

            var affected = repo.Update(updated, id, PropsExceptId<TestAllType>());

            Assert.Equal(1, affected);
            var actual = repo.GetById(id);
            Assert.NotNull(actual);
            Assert.Equal(updated.StringNotNull, actual.StringNotNull);
            Assert.Equal(updated.GuidNotNull, actual.GuidNotNull);
            Assert.Equal(updated.DecimalNotNull, actual.DecimalNotNull);
        }
        finally
        {
            if (id > 0) DeleteRow(c, "TestAllTypes", id);
        }
    }

    [Fact]
    public void Update_SetNullableToNull_PersistsNull()
    {
        using var c = OpenConnection();
        var repo = AllTypesRepo(c);
        int id = 0;
        try
        {
            id = Insert(repo, NewAllType());
            var updated = NewAllType();
            updated.Int32Nullable = null;
            updated.StringNullable = null;
            updated.GuidNullable = null;

            repo.Update(updated, id, PropsExceptId<TestAllType>());

            var actual = repo.GetById(id);
            Assert.NotNull(actual);
            Assert.Null(actual.Int32Nullable);
            Assert.Null(actual.StringNullable);
            Assert.Null(actual.GuidNullable);
        }
        finally
        {
            if (id > 0) DeleteRow(c, "TestAllTypes", id);
        }
    }

    [Fact]
    public void Update_OnlySpecifiedProperties_LeavesOthersUnchanged()
    {
        using var c = OpenConnection();
        var repo = AllTypesRepo(c);
        var original = NewAllType();
        int id = 0;
        try
        {
            id = Insert(repo, original);
            var changed = NewAllType("UnitTest-changed");
            changed.Int32NotNull = 999;
            var props = new[]
            {
                typeof(TestAllType).GetProperty(nameof(TestAllType.StringNotNull))!,
                typeof(TestAllType).GetProperty(nameof(TestAllType.Int32NotNull))!
            };

            var affected = repo.Update(changed, id, props);

            Assert.Equal(1, affected);
            var actual = repo.GetById(id);
            Assert.NotNull(actual);
            Assert.Equal("UnitTest-changed", actual.StringNotNull);
            Assert.Equal(999, actual.Int32NotNull);
            Assert.Equal(original.GuidNotNull, actual.GuidNotNull);
            Assert.Equal(original.Int64NotNull, actual.Int64NotNull);
        }
        finally
        {
            if (id > 0) DeleteRow(c, "TestAllTypes", id);
        }
    }

    [Fact]
    public void Update_DateTimeType_UpdatesValues()
    {
        using var c = OpenConnection();
        var repo = DateTimeRepo(c);
        int id = 0;
        try
        {
            id = Insert(repo, NewDateTimeType());
            var updated = new DateTimeType
            {
                DateTimeNotNull = new DateTime(2030, 1, 1, 1, 1, 1),
                DateTimeNullable = null,
                TimeSpanNotNull = new TimeSpan(5, 0, 0),
                TimeSpanNullable = null,
                TimeOnlyNotNull = new TimeOnly(6, 0, 0),
                TimeOnlyNullable = null
            };

            var affected = repo.Update(updated, id, PropsExceptId<DateTimeType>());

            Assert.Equal(1, affected);
            var actual = repo.GetById(id);
            Assert.NotNull(actual);
            Assert.Equal(updated.DateTimeNotNull, actual.DateTimeNotNull);
            Assert.Null(actual.DateTimeNullable);
            Assert.Equal(updated.TimeSpanNotNull, actual.TimeSpanNotNull);
            Assert.Null(actual.TimeSpanNullable);
            Assert.Equal(updated.TimeOnlyNotNull, actual.TimeOnlyNotNull);
            Assert.Null(actual.TimeOnlyNullable);
        }
        finally
        {
            if (id > 0) DeleteRow(c, "DateTimeTypes", id);
        }
    }

    [Fact]
    public void Update_NonExistingKey_ReturnsZero()
    {
        using var c = OpenConnection();
        var repo = AllTypesRepo(c);

        var affected = repo.Update(NewAllType(), -1, PropsExceptId<TestAllType>());

        Assert.Equal(0, affected);
    }

    [Fact]
    public void Update_OnlyUpdatesRowWithGivenKey()
    {
        using var c = OpenConnection();
        var repo = AllTypesRepo(c);
        var other = NewAllType();
        int id1 = 0, id2 = 0;
        try
        {
            id1 = Insert(repo, NewAllType());
            id2 = Insert(repo, other);

            repo.Update(NewAllType("UnitTest-target"), id1, PropsExceptId<TestAllType>());

            Assert.Equal("UnitTest-target", repo.GetById(id1)!.StringNotNull);
            Assert.Equal(other.StringNotNull, repo.GetById(id2)!.StringNotNull);
        }
        finally
        {
            if (id1 > 0) DeleteRow(c, "TestAllTypes", id1);
            if (id2 > 0) DeleteRow(c, "TestAllTypes", id2);
        }
    }

    [Fact]
    public void Update_NullEntity_ThrowsArgumentNullException()
    {
        using var c = OpenConnection();
        var repo = AllTypesRepo(c);

        Assert.Throws<ArgumentNullException>(() => repo.Update(null!, 1, PropsExceptId<TestAllType>()));
    }

    [Fact]
    public void Update_NoProperties_ReturnsZeroAndChangesNothing()
    {
        using var c = OpenConnection();
        var repo = AllTypesRepo(c);
        var original = NewAllType();
        int id = 0;
        try
        {
            id = Insert(repo, original);

            var affected = repo.Update(NewAllType("UnitTest-ignored"), id, []);

            Assert.Equal(0, affected);
            Assert.Equal(original.StringNotNull, repo.GetById(id)!.StringNotNull);
        }
        finally
        {
            if (id > 0) DeleteRow(c, "TestAllTypes", id);
        }
    }

    // ---------- Delete ----------

    [Fact]
    public void Delete_ExistingRow_RemovesRow()
    {
        using var c = OpenConnection();
        var repo = AllTypesRepo(c);
        int id = 0;
        try
        {
            id = Insert(repo, NewAllType());
            Assert.NotNull(repo.GetById(id));

            repo.Delete(id);

            Assert.Null(repo.GetById(id));
        }
        finally
        {
            if (id > 0) DeleteRow(c, "TestAllTypes", id);
        }
    }

    [Fact]
    public void Delete_DateTimeType_RemovesRow()
    {
        using var c = OpenConnection();
        var repo = DateTimeRepo(c);
        int id = 0;
        try
        {
            id = Insert(repo, NewDateTimeType());

            repo.Delete(id);

            Assert.Null(repo.GetById(id));
        }
        finally
        {
            if (id > 0) DeleteRow(c, "DateTimeTypes", id);
        }
    }

    [Fact]
    public void Delete_NonExistingKey_DoesNotThrowAndKeepsOtherRows()
    {
        using var c = OpenConnection();
        var repo = AllTypesRepo(c);
        int id = 0;
        try
        {
            id = Insert(repo, NewAllType());

            var ex = Record.Exception(() => repo.Delete(-1));

            Assert.Null(ex);
            Assert.NotNull(repo.GetById(id));
        }
        finally
        {
            if (id > 0) DeleteRow(c, "TestAllTypes", id);
        }
    }

    [Fact]
    public void Delete_OnlyRemovesRowWithGivenKey()
    {
        using var c = OpenConnection();
        var repo = AllTypesRepo(c);
        int id1 = 0, id2 = 0;
        try
        {
            id1 = Insert(repo, NewAllType());
            id2 = Insert(repo, NewAllType());

            repo.Delete(id1);

            Assert.Null(repo.GetById(id1));
            Assert.NotNull(repo.GetById(id2));
        }
        finally
        {
            if (id1 > 0) DeleteRow(c, "TestAllTypes", id1);
            if (id2 > 0) DeleteRow(c, "TestAllTypes", id2);
        }
    }

    // ---------- Map ----------

    [Fact]
    public void Map_Null_ReturnsEmptyList()
    {
        var result = DapperRepository<TestAllType>.Map<TestAllType>(null!);

        Assert.Empty(result);
    }

    [Fact]
    public void Map_EmptyCollection_ReturnsEmptyList()
    {
        var result = DapperRepository<TestAllType>.Map<TestAllType>(new List<dynamic>());

        Assert.Empty(result);
    }

    [Fact]
    public void Map_DictionaryRows_MapsCaseInsensitively()
    {
        IEnumerable<dynamic> rows = new List<dynamic>
        {
            new Dictionary<string, object?> { ["id"] = 5, ["INT32NOTNULL"] = 10, ["StringNotNull"] = "abc" },
            new Dictionary<string, object?> { ["Id"] = 6, ["Int32NotNull"] = 11, ["StringNotNull"] = "def", ["StringNullable"] = null }
        };

        var result = DapperRepository<TestAllType>.Map<TestAllType>(rows);

        Assert.Equal(2, result.Count);
        Assert.Equal(5, result[0].Id);
        Assert.Equal(10, result[0].Int32NotNull);
        Assert.Equal("abc", result[0].StringNotNull);
        Assert.Equal("def", result[1].StringNotNull);
        Assert.Null(result[1].StringNullable);
    }

    [Fact]
    public void Map_UnknownProperties_AreIgnored()
    {
        IEnumerable<dynamic> rows = new List<dynamic>
        {
            new Dictionary<string, object?> { ["Id"] = 1, ["StringNotNull"] = "x", ["Unknown"] = "ignored" }
        };

        var result = DapperRepository<TestAllType>.Map<TestAllType>(rows);

        Assert.Single(result);
        Assert.Equal("x", result[0].StringNotNull);
    }

    [Fact]
    public void Map_TimeStringsAndDates_MapToTimeAndDateTypes()
    {
        IEnumerable<dynamic> rows = new List<dynamic>
        {
            new Dictionary<string, object?>
            {
                ["Id"] = 1,
                ["DateTimeNotNull"] = new DateTime(2024, 1, 2, 3, 4, 5),
                ["TimeSpanNotNull"] = new TimeSpan(1, 2, 3),
                ["TimeOnlyNotNull"] = new TimeSpan(4, 5, 6)
            }
        };

        var result = DapperRepository<DateTimeType>.Map<DateTimeType>(rows);

        var item = Assert.Single(result);
        Assert.Equal(new DateTime(2024, 1, 2, 3, 4, 5), item.DateTimeNotNull);
        Assert.Equal(new TimeSpan(1, 2, 3), item.TimeSpanNotNull);
        Assert.Equal(new TimeOnly(4, 5, 6), item.TimeOnlyNotNull);
    }
}
