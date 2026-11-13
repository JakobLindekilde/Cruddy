using System.Text.Json;
using Lindekilde.Blazor.Cruddy.Components;
using Lindekilde.Blazor.Cruddy.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Lindekilde.Blazor.Cruddy.Tests.Repositories.DatabaseTests;

/// <summary>
/// Integration tests for <see cref="DapperRepository"/> against a real CruddyDB database.
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

    private static async Task<SqlConnection> OpenConnectionAsync()
    {
        var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        return connection;
    }

    private static Task<int> DeleteRowAsync(SqlConnection c, string table, int id) =>
        c.ExecuteAsync($"DELETE FROM dbo.{table} WHERE Id = @id", new { id });

    private const string DateTimeTable = "DateTimeTypes";
    private const string AllTypesTable = "TestAllTypes";

    private static DapperRepository CreateRepo(SqlConnection c, string table, string? keyColumn = "Id") =>
        new(c, "dbo", table, keyColumn);

    private static CruddyField Field(string column, Type type, bool nullable = false, bool isKey = false) =>
        new(column, column, type, nullable, isKey, false);

    private static readonly CruddyField[] DateTimeFields =
    [
        Field("DateTimeNotNull", typeof(DateTime)),
        Field("DateTimeNullable", typeof(DateTime), true),
        Field("TimeSpanNotNull", typeof(TimeSpan)),
        Field("TimeSpanNullable", typeof(TimeSpan), true),
        Field("TimeOnlyNotNull", typeof(TimeSpan)),
        Field("TimeOnlyNullable", typeof(TimeSpan), true),
    ];

    private static readonly CruddyField[] AllTypesFields =
    [
        Field("Int32NotNull", typeof(int)),
        Field("Int32Nullable", typeof(int), true),
        Field("Int64NotNull", typeof(long)),
        Field("Int64Nullable", typeof(long), true),
        Field("Int16NotNull", typeof(short)),
        Field("Int16Nullable", typeof(short), true),
        Field("ByteNotNull", typeof(byte)),
        Field("ByteNullable", typeof(byte), true),
        Field("DecimalNotNull", typeof(decimal)),
        Field("DecimalNullable", typeof(decimal), true),
        Field("DoubleNotNull", typeof(double)),
        Field("DoubleNullable", typeof(double), true),
        Field("SingleNotNull", typeof(float)),
        Field("SingleNullable", typeof(float), true),
        Field("BoolNotNull", typeof(bool)),
        Field("BoolNullable", typeof(bool), true),
        Field("StringNotNull", typeof(string)),
        Field("StringNullable", typeof(string), true),
        Field("GuidNotNull", typeof(Guid)),
        Field("GuidNullable", typeof(Guid), true),
    ];

    private static CruddyRow DateTimeRow(bool withNullables) => new()
    {
        ["DateTimeNotNull"] = new DateTime(2021, 3, 4, 5, 6, 7),
        ["DateTimeNullable"] = withNullables ? new DateTime(2022, 4, 5, 6, 7, 8) : null,
        ["TimeSpanNotNull"] = new TimeSpan(1, 2, 3),
        ["TimeSpanNullable"] = withNullables ? new TimeSpan(4, 5, 6) : null,
        ["TimeOnlyNotNull"] = new TimeSpan(7, 8, 9),
        ["TimeOnlyNullable"] = withNullables ? new TimeSpan(10, 11, 12) : null,
    };

    private static CruddyRow AllTypesRow(bool withNullables, Guid? guid = null) => new()
    {
        ["Int32NotNull"] = 123,
        ["Int32Nullable"] = withNullables ? -123 : null,
        ["Int64NotNull"] = 9_000_000_000L,
        ["Int64Nullable"] = withNullables ? -9_000_000_000L : null,
        ["Int16NotNull"] = (short)1234,
        ["Int16Nullable"] = withNullables ? (short)-1234 : null,
        ["ByteNotNull"] = (byte)200,
        ["ByteNullable"] = withNullables ? (byte)100 : null,
        ["DecimalNotNull"] = 12.3456m,
        ["DecimalNullable"] = withNullables ? -65.4321m : null,
        ["DoubleNotNull"] = 1.25d,
        ["DoubleNullable"] = withNullables ? -2.5d : null,
        ["SingleNotNull"] = 3.5f,
        ["SingleNullable"] = withNullables ? -4.75f : null,
        ["BoolNotNull"] = true,
        ["BoolNullable"] = withNullables ? false : null,
        ["StringNotNull"] = "Unit test æøå",
        ["StringNullable"] = withNullables ? "Nullable text" : null,
        ["GuidNotNull"] = guid ?? Guid.NewGuid(),
        ["GuidNullable"] = withNullables ? Guid.NewGuid() : null,
    };

    private static async Task<int> AddRowAsync(DapperRepository repo, CruddyRow row, IReadOnlyList<CruddyField> fields) =>
        Convert.ToInt32(await repo.AddAsync(row, fields));

    private static async Task DeleteRowsAsync(SqlConnection c, string table, IEnumerable<int> ids)
    {
        foreach (var id in ids.Where(id => id != 0))
        {
            await DeleteRowAsync(c, table, id);
        }
    }

    private static Task<int> CountRowsAsync(SqlConnection c, string table) =>
        c.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM dbo.{table}");

    private static void AssertRowMatches(CruddyRow expected, CruddyRow? actual, IEnumerable<CruddyField> fields)
    {
        Assert.NotNull(actual);
        foreach (var field in fields)
        {
            Assert.Equal(expected[field.AliasName], actual[field.AliasName]);
        }
    }

    // ---------- CreateParameters ----------

    [Fact]
    public void CreateParameters_NamesParametersSequentially()
    {
        CruddyField[] fields = [Field("A", typeof(int)), Field("B", typeof(string)), Field("C", typeof(bool))];
        var row = new CruddyRow { ["A"] = 1, ["B"] = "two", ["C"] = true };

        var dp = DapperRepository.CreateParameters(row, fields);

        Assert.Equal(["p0", "p1", "p2"], dp.ParameterNames);
        Assert.Equal(1, dp.Get<int>("p0"));
        Assert.Equal("two", dp.Get<string>("p1"));
        Assert.True(dp.Get<bool>("p2"));
    }

    [Fact]
    public void CreateParameters_EmptyFields_ReturnsNoParameters()
    {
        var dp = DapperRepository.CreateParameters(new CruddyRow { ["A"] = 1 }, []);

        Assert.Empty(dp.ParameterNames);
    }

    [Fact]
    public void CreateParameters_MissingKeyInRow_YieldsNullValue()
    {
        var dp = DapperRepository.CreateParameters(new CruddyRow(), [Field("A", typeof(string), true)]);

        Assert.Equal(["p0"], dp.ParameterNames);
        Assert.Null(dp.Get<string?>("p0"));
    }

    [Fact]
    public void CreateParameters_NullValueInRow_YieldsNullValue()
    {
        var dp = DapperRepository.CreateParameters(new CruddyRow { ["A"] = null }, [Field("A", typeof(int), true)]);

        Assert.Null(dp.Get<int?>("p0"));
    }

    [Fact]
    public void CreateParameters_UsesFieldNameNotColumn()
    {
        var field = new CruddyField("db_col", "Alias", typeof(int), false, false, false);
        var row = new CruddyRow { ["Alias"] = 5, ["db_col"] = 99 };

        var dp = DapperRepository.CreateParameters(row, [field]);

        Assert.Equal(5, dp.Get<int>("p0"));
    }

    [Fact]
    public void CreateParameters_IgnoresRowValuesWithoutField()
    {
        var row = new CruddyRow { ["A"] = 1, ["Extra"] = "ignored" };

        var dp = DapperRepository.CreateParameters(row, [Field("A", typeof(int))]);

        Assert.Equal(["p0"], dp.ParameterNames);
    }

    [Fact]
    public void CreateParameters_FieldsOrderDeterminesParameterOrder()
    {
        var row = new CruddyRow { ["A"] = 1, ["B"] = 2 };

        var dp = DapperRepository.CreateParameters(row, [Field("B", typeof(int)), Field("A", typeof(int))]);

        Assert.Equal(2, dp.Get<int>("p0"));
        Assert.Equal(1, dp.Get<int>("p1"));
    }

    // ---------- MapSqlType ----------

    [Theory]
    [InlineData("int", typeof(int))]
    [InlineData("bigint", typeof(long))]
    [InlineData("smallint", typeof(short))]
    [InlineData("tinyint", typeof(byte))]
    [InlineData("bit", typeof(bool))]
    [InlineData("decimal", typeof(decimal))]
    [InlineData("numeric", typeof(decimal))]
    [InlineData("money", typeof(decimal))]
    [InlineData("smallmoney", typeof(decimal))]
    [InlineData("float", typeof(double))]
    [InlineData("real", typeof(float))]
    [InlineData("date", typeof(DateOnly))]
    [InlineData("datetime", typeof(DateTime))]
    [InlineData("datetime2", typeof(DateTime))]
    [InlineData("smalldatetime", typeof(DateTime))]
    [InlineData("time", typeof(TimeSpan))]
    [InlineData("uniqueidentifier", typeof(Guid))]
    [InlineData("char", typeof(string))]
    [InlineData("varchar", typeof(string))]
    [InlineData("nchar", typeof(string))]
    [InlineData("nvarchar", typeof(string))]
    [InlineData("text", typeof(string))]
    [InlineData("ntext", typeof(string))]
    [InlineData("sysname", typeof(string))]
    public void MapSqlType_SupportedType_ReturnsClrType(string sqlType, Type expected)
    {
        Assert.Equal(expected, DapperRepository.MapSqlType(sqlType));
    }

    [Theory]
    [InlineData("INT", typeof(int))]
    [InlineData("NVarChar", typeof(string))]
    [InlineData("DateTime2", typeof(DateTime))]
    [InlineData("Time", typeof(TimeSpan))]
    public void MapSqlType_IsCaseInsensitive(string sqlType, Type expected)
    {
        Assert.Equal(expected, DapperRepository.MapSqlType(sqlType));
    }

    // ---------- GetColumns ----------

    [Fact]
    public async Task GetColumns_DateTimeTypes_ReturnsAllColumnsInTableOrder()
    {
        await using var c = await OpenConnectionAsync();

        var columns = await CreateRepo(c, DateTimeTable).GetColumnsAsync([]);

        Assert.Equal(
            ["Id", "DateTimeNotNull", "DateTimeNullable", "TimeSpanNotNull", "TimeSpanNullable", "TimeOnlyNotNull", "TimeOnlyNullable"],
            columns.Select(f => f.ColumnName));
    }

    [Fact]
    public async Task GetColumns_DateTimeTypes_MapsTypesNullabilityAndKey()
    {
        await using var c = await OpenConnectionAsync();

        var columns = (await CreateRepo(c, DateTimeTable).GetColumnsAsync([])).ToDictionary(f => f.ColumnName);

        Assert.Equal(typeof(int), columns["Id"].Type);
        Assert.True(columns["Id"].IsPrimaryKey);
        Assert.False(columns["Id"].IsNullable);

        foreach (var field in DateTimeFields)
        {
            var actual = columns[field.ColumnName];
            Assert.Equal(field.Type, actual.Type);
            Assert.Equal(field.IsNullable, actual.IsNullable);
            Assert.False(actual.IsPrimaryKey);
            Assert.False(actual.IsReadOnly);
        }
    }

    [Fact]
    public async Task GetColumns_TestAllTypes_MapsAllSupportedTypes()
    {
        await using var c = await OpenConnectionAsync();

        var columns = await CreateRepo(c, AllTypesTable).GetColumnsAsync([]);

        Assert.Equal(AllTypesFields.Length + 1, columns.Count);
        Assert.Equal("Id", columns[0].ColumnName);
        Assert.Equal(typeof(int), columns[0].Type);
        Assert.True(columns[0].IsPrimaryKey);

        var byColumn = columns.ToDictionary(f => f.ColumnName);
        foreach (var expected in AllTypesFields)
        {
            var actual = byColumn[expected.ColumnName];
            Assert.Equal(expected.Type, actual.Type);
            Assert.Equal(expected.IsNullable, actual.IsNullable);
            Assert.False(actual.IsPrimaryKey);
            Assert.False(actual.IsReadOnly);
        }
    }

    [Fact]
    public async Task GetColumns_SetsNameEqualToColumn()
    {
        await using var c = await OpenConnectionAsync();

        var columns = await CreateRepo(c, AllTypesTable).GetColumnsAsync([]);

        Assert.All(columns, f => Assert.Equal(f.ColumnName, f.AliasName));
    }

    [Fact]
    public async Task GetColumns_OnlyOnePrimaryKeyColumn()
    {
        await using var c = await OpenConnectionAsync();

        var columns = await CreateRepo(c, AllTypesTable).GetColumnsAsync([]);

        Assert.Equal(["Id"], columns.Where(f => f.IsPrimaryKey).Select(f => f.ColumnName));
    }

    [Fact]
    public async Task GetColumns_UnknownTable_ReturnsEmptyList()
    {
        await using var c = await OpenConnectionAsync();

        var columns = await CreateRepo(c, "NoSuchTable_" + Guid.NewGuid().ToString("N")).GetColumnsAsync([]);

        Assert.Empty(columns);
    }

    [Fact]
    public async Task GetColumns_UnknownSchema_ReturnsEmptyList()
    {
        await using var c = await OpenConnectionAsync();

        var columns = await new DapperRepository(c, "no_such_schema", DateTimeTable, "Id").GetColumnsAsync([]);

        Assert.Empty(columns);
    }

    // ---------- Add ----------

    [Fact]
    public async Task Add_DateTimeTypes_ReturnsIdentityAndStoresAllValues()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, DateTimeTable);
        var row = DateTimeRow(withNullables: true);
        var id = 0;
        try
        {
            var result = await repo.AddAsync(row, DateTimeFields);
            id = Convert.ToInt32(result);

            Assert.True(id > 0);
            AssertRowMatches(row, await repo.GetByIdAsync(id), DateTimeFields);
        }
        finally
        {
            await DeleteRowsAsync(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public async Task Add_DateTimeTypes_NullableValuesAreStoredAsNull()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, DateTimeTable);
        var row = DateTimeRow(withNullables: false);
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, row, DateTimeFields);

            var stored = await repo.GetByIdAsync(id);
            AssertRowMatches(row, stored, DateTimeFields);
            Assert.Null(stored!["DateTimeNullable"]);
            Assert.Null(stored["TimeSpanNullable"]);
            Assert.Null(stored["TimeOnlyNullable"]);
        }
        finally
        {
            await DeleteRowsAsync(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public async Task Add_DateTimeTypes_OnlySpecifiedFieldsAreInserted()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, DateTimeTable);
        var row = DateTimeRow(withNullables: true);
        var required = DateTimeFields.Where(f => !f.IsNullable).ToList();
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, row, required);

            var stored = await repo.GetByIdAsync(id);
            AssertRowMatches(row, stored, required);
            Assert.Null(stored!["DateTimeNullable"]);
            Assert.Null(stored["TimeSpanNullable"]);
            Assert.Null(stored["TimeOnlyNullable"]);
        }
        finally
        {
            await DeleteRowsAsync(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public async Task Add_DateTimeTypes_DateOnlyValueIsStoredAsDateTime()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, DateTimeTable);
        var row = DateTimeRow(withNullables: false);
        row["DateTimeNotNull"] = new DateOnly(2020, 5, 17);
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, row, DateTimeFields);

            Assert.Equal(new DateTime(2020, 5, 17), (await repo.GetByIdAsync(id))!["DateTimeNotNull"]);
        }
        finally
        {
            await DeleteRowsAsync(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public async Task Add_DateTimeTypes_TimeOnlyValueIsStoredAsTime()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, DateTimeTable);
        var row = DateTimeRow(withNullables: false);
        row["TimeOnlyNotNull"] = new TimeOnly(13, 45, 30);
        row["TimeOnlyNullable"] = new TimeOnly(23, 59, 59);
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, row, DateTimeFields);

            var stored = (await repo.GetByIdAsync(id))!;
            Assert.Equal(new TimeSpan(13, 45, 30), stored["TimeOnlyNotNull"]);
            Assert.Equal(new TimeSpan(23, 59, 59), stored["TimeOnlyNullable"]);
        }
        finally
        {
            await DeleteRowsAsync(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public async Task Add_DateTimeTypes_BoundaryValuesRoundTrip()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, DateTimeTable);
        var row = new CruddyRow
        {
            ["DateTimeNotNull"] = new DateTime(1753, 1, 1),
            ["DateTimeNullable"] = new DateTime(9999, 12, 31, 23, 59, 0),
            ["TimeSpanNotNull"] = TimeSpan.Zero,
            ["TimeSpanNullable"] = new TimeSpan(23, 59, 59),
            ["TimeOnlyNotNull"] = TimeSpan.Zero,
            ["TimeOnlyNullable"] = new TimeSpan(0, 0, 0, 0, 500),
        };
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, row, DateTimeFields);

            AssertRowMatches(row, await repo.GetByIdAsync(id), DateTimeFields);
        }
        finally
        {
            await DeleteRowsAsync(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public async Task Add_DateTimeTypes_TwoInsertsReturnDifferentIncreasingIdentities()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, DateTimeTable);
        var ids = new List<int>();
        try
        {
            ids.Add(await AddRowAsync(repo, DateTimeRow(false), DateTimeFields));
            ids.Add(await AddRowAsync(repo, DateTimeRow(true), DateTimeFields));

            Assert.True(ids[1] > ids[0]);
        }
        finally
        {
            await DeleteRowsAsync(c, DateTimeTable, ids);
        }
    }

    [Fact]
    public async Task Add_TestAllTypes_StoresAllValues()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, AllTypesTable);
        var row = AllTypesRow(withNullables: true);
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, row, AllTypesFields);

            Assert.True(id > 0);
            AssertRowMatches(row, await repo.GetByIdAsync(id), AllTypesFields);
        }
        finally
        {
            await DeleteRowsAsync(c, AllTypesTable, [id]);
        }
    }

    [Fact]
    public async Task Add_TestAllTypes_NullableValuesAreStoredAsNull()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, AllTypesTable);
        var row = AllTypesRow(withNullables: false);
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, row, AllTypesFields);

            var stored = await repo.GetByIdAsync(id);
            AssertRowMatches(row, stored, AllTypesFields);
            Assert.All(AllTypesFields.Where(f => f.IsNullable), f => Assert.Null(stored![f.AliasName]));
        }
        finally
        {
            await DeleteRowsAsync(c, AllTypesTable, [id]);
        }
    }

    [Fact]
    public async Task Add_TestAllTypes_ReturnsIdentityOfNewRow()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, AllTypesTable);
        var guid = Guid.NewGuid();
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, AllTypesRow(false, guid), AllTypesFields);

            var storedId = await c.ExecuteScalarAsync<int>($"SELECT Id FROM dbo.{AllTypesTable} WHERE GuidNotNull = @guid", new { guid });
            Assert.Equal(storedId, id);
        }
        finally
        {
            await DeleteRowsAsync(c, AllTypesTable, [id]);
        }
    }

    [Fact]
    public async Task Add_TestAllTypes_ExtremeNumericValuesRoundTrip()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, AllTypesTable);
        var row = AllTypesRow(withNullables: true);
        row["Int32NotNull"] = int.MinValue;
        row["Int32Nullable"] = int.MaxValue;
        row["Int64NotNull"] = long.MinValue;
        row["Int64Nullable"] = long.MaxValue;
        row["Int16NotNull"] = short.MinValue;
        row["Int16Nullable"] = short.MaxValue;
        row["ByteNotNull"] = byte.MinValue;
        row["ByteNullable"] = byte.MaxValue;
        row["BoolNotNull"] = false;
        row["StringNotNull"] = new string('x', 50);
        row["StringNullable"] = string.Empty;
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, row, AllTypesFields);

            AssertRowMatches(row, await repo.GetByIdAsync(id), AllTypesFields);
        }
        finally
        {
            await DeleteRowsAsync(c, AllTypesTable, [id]);
        }
    }

    [Fact]
    public async Task Add_MissingValueForNotNullColumn_Throws_AndInsertsNothing()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, AllTypesTable);
        var row = AllTypesRow(withNullables: false);
        row.Remove("StringNotNull");
        var before = await CountRowsAsync(c, AllTypesTable);

        await Assert.ThrowsAnyAsync<SqlException>(() => repo.AddAsync(row, AllTypesFields));

        Assert.Equal(before, await CountRowsAsync(c, AllTypesTable));
    }

    [Fact]
    public async Task Add_NullRow_ThrowsArgumentNullException()
    {
        await using var c = await OpenConnectionAsync();

        await Assert.ThrowsAsync<ArgumentNullException>(() => CreateRepo(c, DateTimeTable).AddAsync(null!, DateTimeFields));
    }

    [Fact]
    public async Task Add_NoFields_ReturnsZeroAndInsertsNothing()
    {
        await using var c = await OpenConnectionAsync();
        var before = await CountRowsAsync(c, DateTimeTable);

        var result = await CreateRepo(c, DateTimeTable).AddAsync(DateTimeRow(true), []);

        Assert.Equal(0, result);
        Assert.Equal(before, await CountRowsAsync(c, DateTimeTable));
    }

    // ---------- GetById ----------

    [Fact]
    public async Task GetById_DateTimeTypes_ReturnsRowWithAllColumns()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, DateTimeTable);
        var row = DateTimeRow(withNullables: true);
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, row, DateTimeFields);

            var stored = await repo.GetByIdAsync(id);

            Assert.NotNull(stored);
            Assert.Equal(DateTimeFields.Length + 1, stored.Count);
            Assert.Equal(id, stored["Id"]);
            AssertRowMatches(row, stored, DateTimeFields);
        }
        finally
        {
            await DeleteRowsAsync(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public async Task GetById_ColumnNamesAreCaseInsensitive()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, DateTimeTable);
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, DateTimeRow(true), DateTimeFields);

            var stored = (await repo.GetByIdAsync(id))!;

            Assert.Equal(stored["DateTimeNotNull"], stored["DATETIMENOTNULL"]);
            Assert.True(stored.ContainsKey("id"));
        }
        finally
        {
            await DeleteRowsAsync(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public async Task GetById_SpecificColumns_ReturnsOnlyThoseColumns()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, DateTimeTable);
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, DateTimeRow(true), DateTimeFields);

            var stored = await repo.GetByIdAsync(id, "Id, TimeSpanNotNull");

            Assert.NotNull(stored);
            Assert.Equal(["Id", "TimeSpanNotNull"], stored.Keys);
            Assert.Equal(new TimeSpan(1, 2, 3), stored["TimeSpanNotNull"]);
        }
        finally
        {
            await DeleteRowsAsync(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public async Task GetById_UnknownId_ReturnsNull()
    {
        await using var c = await OpenConnectionAsync();

        Assert.Null(await CreateRepo(c, DateTimeTable).GetByIdAsync(-1));
    }

    [Fact]
    public async Task GetById_DeletedRow_ReturnsNull()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, DateTimeTable);
        var id = await AddRowAsync(repo, DateTimeRow(false), DateTimeFields);
        await DeleteRowAsync(c, DateTimeTable, id);

        Assert.Null(await repo.GetByIdAsync(id));
    }

    [Fact]
    public async Task GetById_ReturnsOnlyTheRequestedRow()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, AllTypesTable);
        var first = AllTypesRow(false);
        var second = AllTypesRow(true);
        var ids = new List<int>();
        try
        {
            ids.Add(await AddRowAsync(repo, first, AllTypesFields));
            ids.Add(await AddRowAsync(repo, second, AllTypesFields));

            var stored = await repo.GetByIdAsync(ids[1]);

            Assert.Equal(ids[1], stored!["Id"]);
            AssertRowMatches(second, stored, AllTypesFields);
        }
        finally
        {
            await DeleteRowsAsync(c, AllTypesTable, ids);
        }
    }

    [Fact]
    public async Task GetById_UsesConfiguredKeyColumn_WithGuidKey()
    {
        await using var c = await OpenConnectionAsync();
        var setup = CreateRepo(c, AllTypesTable);
        var guid = Guid.NewGuid();
        var id = 0;
        try
        {
            id = await AddRowAsync(setup, AllTypesRow(false, guid), AllTypesFields);

            var stored = await CreateRepo(c, AllTypesTable, "GuidNotNull").GetByIdAsync(guid);

            Assert.NotNull(stored);
            Assert.Equal(id, stored["Id"]);
            Assert.Equal(guid, stored["GuidNotNull"]);
        }
        finally
        {
            await DeleteRowsAsync(c, AllTypesTable, [id]);
        }
    }

    // ---------- GetAll ----------

    [Fact]
    public async Task GetAll_DateTimeTypes_ReturnsMatchingRowsInSqlOrder()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, DateTimeTable);
        var first = DateTimeRow(false);
        var second = DateTimeRow(true);
        var ids = new List<int>();
        try
        {
            ids.Add(await AddRowAsync(repo, first, DateTimeFields));
            ids.Add(await AddRowAsync(repo, second, DateTimeFields));

            var rows = await repo.GetAllAsync($"SELECT * FROM dbo.{DateTimeTable} WHERE Id IN ({ids[0]}, {ids[1]}) ORDER BY Id DESC");

            Assert.Equal(2, rows.Count);
            Assert.Equal(ids[1], rows[0]["Id"]);
            Assert.Equal(ids[0], rows[1]["Id"]);
            AssertRowMatches(second, rows[0], DateTimeFields);
            AssertRowMatches(first, rows[1], DateTimeFields);
        }
        finally
        {
            await DeleteRowsAsync(c, DateTimeTable, ids);
        }
    }

    [Fact]
    public async Task GetAll_TestAllTypes_ReturnsAllTypedValues()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, AllTypesTable);
        var row = AllTypesRow(true);
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, row, AllTypesFields);

            var rows = await repo.GetAllAsync($"SELECT * FROM dbo.{AllTypesTable} WHERE Id = {id}");

            var stored = Assert.Single(rows);
            AssertRowMatches(row, stored, AllTypesFields);
        }
        finally
        {
            await DeleteRowsAsync(c, AllTypesTable, [id]);
        }
    }

    [Fact]
    public async Task GetAll_NullValuesAreReturnedAsNull()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, AllTypesTable);
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, AllTypesRow(false), AllTypesFields);

            var stored = Assert.Single(await repo.GetAllAsync($"SELECT * FROM dbo.{AllTypesTable} WHERE Id = {id}"));

            Assert.All(AllTypesFields.Where(f => f.IsNullable), f => Assert.Null(stored[f.AliasName]));
        }
        finally
        {
            await DeleteRowsAsync(c, AllTypesTable, [id]);
        }
    }

    [Fact]
    public async Task GetAll_NoMatchingRows_ReturnsEmptyList()
    {
        await using var c = await OpenConnectionAsync();

        var rows = await CreateRepo(c, DateTimeTable).GetAllAsync($"SELECT * FROM dbo.{DateTimeTable} WHERE Id = -1");

        Assert.NotNull(rows);
        Assert.Empty(rows);
    }

    [Fact]
    public async Task GetAll_RespectsSelectedColumnsAndAliases()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, DateTimeTable);
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, DateTimeRow(true), DateTimeFields);

            var stored = Assert.Single(await repo.GetAllAsync($"SELECT Id AS Key1, DateTimeNotNull AS Moment FROM dbo.{DateTimeTable} WHERE Id = {id}"));

            Assert.Equal(["Key1", "Moment"], stored.Keys);
            Assert.Equal(new DateTime(2021, 3, 4, 5, 6, 7), stored["moment"]);
        }
        finally
        {
            await DeleteRowsAsync(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public async Task GetAll_ReturnsNewListOnEachCall()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, DateTimeTable);
        var sql = $"SELECT * FROM dbo.{DateTimeTable} WHERE Id = -1";

        Assert.NotSame(await repo.GetAllAsync(sql), await repo.GetAllAsync(sql));
    }

    // ---------- Update ----------

    [Fact]
    public async Task Update_DateTimeTypes_UpdatesAllFields()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, DateTimeTable);
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, DateTimeRow(false), DateTimeFields);
            var updated = new CruddyRow
            {
                ["DateTimeNotNull"] = new DateTime(2000, 1, 2, 3, 4, 5),
                ["DateTimeNullable"] = new DateTime(2001, 2, 3, 4, 5, 6),
                ["TimeSpanNotNull"] = new TimeSpan(11, 12, 13),
                ["TimeSpanNullable"] = new TimeSpan(14, 15, 16),
                ["TimeOnlyNotNull"] = new TimeSpan(17, 18, 19),
                ["TimeOnlyNullable"] = new TimeSpan(20, 21, 22),
            };

            var affected = await repo.UpdateAsync(updated, id, DateTimeFields);

            Assert.Equal(1, affected);
            AssertRowMatches(updated, await repo.GetByIdAsync(id), DateTimeFields);
        }
        finally
        {
            await DeleteRowsAsync(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public async Task Update_DateTimeTypes_OnlyUpdatesSpecifiedFields()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, DateTimeTable);
        var original = DateTimeRow(true);
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, original, DateTimeFields);
            var newTime = new TimeSpan(22, 33, 44);
            var change = new CruddyRow { ["TimeSpanNotNull"] = newTime, ["DateTimeNotNull"] = new DateTime(1999, 9, 9) };

            var affected = await repo.UpdateAsync(change, id, [DateTimeFields[2]]);

            Assert.Equal(1, affected);
            var stored = (await repo.GetByIdAsync(id))!;
            Assert.Equal(newTime, stored["TimeSpanNotNull"]);
            Assert.Equal(original["DateTimeNotNull"], stored["DateTimeNotNull"]);
            Assert.Equal(original["TimeOnlyNullable"], stored["TimeOnlyNullable"]);
        }
        finally
        {
            await DeleteRowsAsync(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public async Task Update_DateTimeTypes_CanSetNullableValuesToNullAndBack()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, DateTimeTable);
        var nullableFields = DateTimeFields.Where(f => f.IsNullable).ToList();
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, DateTimeRow(true), DateTimeFields);

            await repo.UpdateAsync(DateTimeRow(false), id, nullableFields);
            var cleared = (await repo.GetByIdAsync(id))!;
            Assert.All(nullableFields, f => Assert.Null(cleared[f.AliasName]));

            var restored = DateTimeRow(true);
            await repo.UpdateAsync(restored, id, nullableFields);
            AssertRowMatches(restored, await repo.GetByIdAsync(id), nullableFields);
        }
        finally
        {
            await DeleteRowsAsync(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public async Task Update_DateTimeTypes_DateOnlyAndTimeOnlyValues()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, DateTimeTable);
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, DateTimeRow(false), DateTimeFields);
            var change = new CruddyRow
            {
                ["DateTimeNotNull"] = new DateOnly(2030, 6, 7),
                ["TimeOnlyNotNull"] = new TimeOnly(8, 9, 10),
            };

            var affected = await repo.UpdateAsync(change, id, [DateTimeFields[0], DateTimeFields[4]]);

            Assert.Equal(1, affected);
            var stored = (await repo.GetByIdAsync(id))!;
            Assert.Equal(new DateTime(2030, 6, 7), stored["DateTimeNotNull"]);
            Assert.Equal(new TimeSpan(8, 9, 10), stored["TimeOnlyNotNull"]);
        }
        finally
        {
            await DeleteRowsAsync(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public async Task Update_TestAllTypes_UpdatesAllFields()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, AllTypesTable);
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, AllTypesRow(false), AllTypesFields);
            var updated = AllTypesRow(true);
            updated["Int32NotNull"] = 456;
            updated["StringNotNull"] = "Updated";
            updated["BoolNotNull"] = false;

            var affected = await repo.UpdateAsync(updated, id, AllTypesFields);

            Assert.Equal(1, affected);
            AssertRowMatches(updated, await repo.GetByIdAsync(id), AllTypesFields);
        }
        finally
        {
            await DeleteRowsAsync(c, AllTypesTable, [id]);
        }
    }

    [Fact]
    public async Task Update_TestAllTypes_SetsNullableValuesToNull()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, AllTypesTable);
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, AllTypesRow(true), AllTypesFields);
            var updated = AllTypesRow(false);

            await repo.UpdateAsync(updated, id, AllTypesFields);

            var stored = (await repo.GetByIdAsync(id))!;
            Assert.All(AllTypesFields.Where(f => f.IsNullable), f => Assert.Null(stored[f.AliasName]));
        }
        finally
        {
            await DeleteRowsAsync(c, AllTypesTable, [id]);
        }
    }

    [Fact]
    public async Task Update_OnlyAffectsRowWithGivenKey()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, AllTypesTable);
        var untouched = AllTypesRow(true);
        var ids = new List<int>();
        try
        {
            ids.Add(await AddRowAsync(repo, AllTypesRow(false), AllTypesFields));
            ids.Add(await AddRowAsync(repo, untouched, AllTypesFields));

            await repo.UpdateAsync(AllTypesRow(true), ids[0], AllTypesFields);

            AssertRowMatches(untouched, await repo.GetByIdAsync(ids[1]), AllTypesFields);
        }
        finally
        {
            await DeleteRowsAsync(c, AllTypesTable, ids);
        }
    }

    [Fact]
    public async Task Update_UnknownKey_ReturnsZero()
    {
        await using var c = await OpenConnectionAsync();

        var affected = await CreateRepo(c, AllTypesTable).UpdateAsync(AllTypesRow(true), -1, AllTypesFields);

        Assert.Equal(0, affected);
    }

    [Fact]
    public async Task Update_NoFields_ReturnsZeroAndLeavesRowUnchanged()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, DateTimeTable);
        var original = DateTimeRow(true);
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, original, DateTimeFields);

            var affected = await repo.UpdateAsync(DateTimeRow(false), id, []);

            Assert.Equal(0, affected);
            AssertRowMatches(original, await repo.GetByIdAsync(id), DateTimeFields);
        }
        finally
        {
            await DeleteRowsAsync(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public async Task Update_NullRow_ThrowsArgumentNullException()
    {
        await using var c = await OpenConnectionAsync();

        await Assert.ThrowsAsync<ArgumentNullException>(() => CreateRepo(c, DateTimeTable).UpdateAsync(null!, 1, DateTimeFields));
    }

    [Fact]
    public async Task Update_NullForNotNullColumn_Throws_AndKeepsValue()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, AllTypesTable);
        var original = AllTypesRow(false);
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, original, AllTypesFields);
            var invalid = AllTypesRow(false);
            invalid["StringNotNull"] = null;

            await Assert.ThrowsAnyAsync<SqlException>(() => repo.UpdateAsync(invalid, id, AllTypesFields));

            AssertRowMatches(original, await repo.GetByIdAsync(id), AllTypesFields);
        }
        finally
        {
            await DeleteRowsAsync(c, AllTypesTable, [id]);
        }
    }

    // ---------- Delete ----------

    [Fact]
    public async Task Delete_DateTimeTypes_RemovesRow()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, DateTimeTable);
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, DateTimeRow(true), DateTimeFields);
            Assert.NotNull(await repo.GetByIdAsync(id));

            await repo.DeleteAsync(id);

            Assert.Null(await repo.GetByIdAsync(id));
        }
        finally
        {
            await DeleteRowsAsync(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public async Task Delete_TestAllTypes_RemovesRow()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, AllTypesTable);
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, AllTypesRow(true), AllTypesFields);

            await repo.DeleteAsync(id);

            Assert.Null(await repo.GetByIdAsync(id));
        }
        finally
        {
            await DeleteRowsAsync(c, AllTypesTable, [id]);
        }
    }

    [Fact]
    public async Task Delete_OnlyRemovesRowWithGivenKey()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, AllTypesTable);
        var kept = AllTypesRow(true);
        var ids = new List<int>();
        try
        {
            ids.Add(await AddRowAsync(repo, AllTypesRow(false), AllTypesFields));
            ids.Add(await AddRowAsync(repo, kept, AllTypesFields));
            var before = await CountRowsAsync(c, AllTypesTable);

            await repo.DeleteAsync(ids[0]);

            Assert.Equal(before - 1, await CountRowsAsync(c, AllTypesTable));
            Assert.Null(await repo.GetByIdAsync(ids[0]));
            AssertRowMatches(kept, await repo.GetByIdAsync(ids[1]), AllTypesFields);
        }
        finally
        {
            await DeleteRowsAsync(c, AllTypesTable, ids);
        }
    }

    [Fact]
    public async Task Delete_UnknownKey_DoesNotThrowAndDeletesNothing()
    {
        await using var c = await OpenConnectionAsync();
        var before = await CountRowsAsync(c, DateTimeTable);

        var exception = await Record.ExceptionAsync(() => CreateRepo(c, DateTimeTable).DeleteAsync(-1));

        Assert.Null(exception);
        Assert.Equal(before, await CountRowsAsync(c, DateTimeTable));
    }

    [Fact]
    public async Task Delete_CalledTwice_SecondCallIsHarmless()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, DateTimeTable);
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, DateTimeRow(false), DateTimeFields);
            await repo.DeleteAsync(id);

            var exception = await Record.ExceptionAsync(() => repo.DeleteAsync(id));

            Assert.Null(exception);
        }
        finally
        {
            await DeleteRowsAsync(c, DateTimeTable, [id]);
        }
    }

    // ---------- GetFKeyOptions ----------

    [Fact]
    public async Task GetFKeyOptions_TestAllTypes_ReturnsIdAndDisplayOrderedByDisplay()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, AllTypesTable);
        var prefix = "fk-" + Guid.NewGuid().ToString("N");
        var rowB = AllTypesRow(false);
        rowB["StringNotNull"] = prefix + "-B";
        var rowA = AllTypesRow(false);
        rowA["StringNotNull"] = prefix + "-A";
        var ids = new List<int>();
        try
        {
            ids.Add(await AddRowAsync(repo, rowB, AllTypesFields));
            ids.Add(await AddRowAsync(repo, rowA, AllTypesFields));

            var options = await repo.GetFKeyOptionsAsync(new FKeyInfo("FK", $"dbo.{AllTypesTable}", "Id", "StringNotNull"));

            var mine = options.Where(o => ((string)o.Display!).StartsWith(prefix)).ToList();
            Assert.Equal(2, mine.Count);
            Assert.Equal([prefix + "-A", prefix + "-B"], mine.Select(o => o.Display));
            Assert.Equal([ids[1], ids[0]], mine.Select(o => (int)o.Id!));
        }
        finally
        {
            await DeleteRowsAsync(c, AllTypesTable, ids);
        }
    }

    [Fact]
    public async Task GetFKeyOptions_ResultIsSortedByDisplay()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, AllTypesTable);
        var prefix = "fk-" + Guid.NewGuid().ToString("N");
        var ids = new List<int>();
        try
        {
            foreach (var suffix in new[] { "3", "1", "2" })
            {
                var row = AllTypesRow(false);
                row["StringNotNull"] = prefix + suffix;
                ids.Add(await AddRowAsync(repo, row, AllTypesFields));
            }

            var options = await repo.GetFKeyOptionsAsync(new FKeyInfo("FK", $"dbo.{AllTypesTable}", "Id", "StringNotNull"));

            var mine = options.Select(o => (string)o.Display!).Where(d => d.StartsWith(prefix)).ToList();
            Assert.Equal([prefix + "1", prefix + "2", prefix + "3"], mine);
        }
        finally
        {
            await DeleteRowsAsync(c, AllTypesTable, ids);
        }
    }

    [Fact]
    public async Task GetFKeyOptions_DateTimeTypes_MapsTimeSpanDisplay()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, DateTimeTable);
        var row = DateTimeRow(false);
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, row, DateTimeFields);

            var options = await repo.GetFKeyOptionsAsync(new FKeyInfo("FK", $"dbo.{DateTimeTable}", "Id", "TimeSpanNotNull"));

            var option = Assert.Single(options, o => (int)o.Id! == id);
            Assert.Equal(new TimeSpan(1, 2, 3), option.Display);
        }
        finally
        {
            await DeleteRowsAsync(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public async Task GetFKeyOptions_NullDisplayValue_IsReturnedAsNull()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, AllTypesTable);
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, AllTypesRow(false), AllTypesFields);

            var options = await repo.GetFKeyOptionsAsync(new FKeyInfo("FK", $"dbo.{AllTypesTable}", "Id", "StringNullable"));

            Assert.Null(Assert.Single(options, o => (int)o.Id! == id).Display);
        }
        finally
        {
            await DeleteRowsAsync(c, AllTypesTable, [id]);
        }
    }

    [Fact]
    public async Task GetFKeyOptions_DeletedRowIsNoLongerListed()
    {
        await using var c = await OpenConnectionAsync();
        var repo = CreateRepo(c, AllTypesTable);
        var id = 0;
        try
        {
            id = await AddRowAsync(repo, AllTypesRow(false), AllTypesFields);
            var info = new FKeyInfo("FK", $"dbo.{AllTypesTable}", "Id", "StringNotNull");
            Assert.Contains(await repo.GetFKeyOptionsAsync(info), o => (int)o.Id! == id);

            await repo.DeleteAsync(id);

            Assert.DoesNotContain(await repo.GetFKeyOptionsAsync(info), o => (int)o.Id! == id);
        }
        finally
        {
            await DeleteRowsAsync(c, AllTypesTable, [id]);
        }
    }
}
