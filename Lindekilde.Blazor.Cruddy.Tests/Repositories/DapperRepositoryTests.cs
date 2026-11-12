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

    private static SqlConnection OpenConnection()
    {
        var connection = new SqlConnection(ConnectionString);
        connection.Open();
        return connection;
    }

    private static void DeleteRow(SqlConnection c, string table, int id) =>
        c.Execute($"DELETE FROM dbo.{table} WHERE Id = @id", new { id });

    private const string DateTimeTable = "DateTimeTypes";
    private const string AllTypesTable = "TestAllTypes";

    private static DapperRepository CreateRepo(SqlConnection c, string table, string? keyColumn = "Id") =>
        new(c, "dbo", table, keyColumn);

    private static CruddyField Field(string column, Type type, bool nullable = false, bool isKey = false) =>
        new(column, column, type, nullable, isKey, false, false);

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

    private static int AddRow(DapperRepository repo, CruddyRow row, IReadOnlyList<CruddyField> fields) =>
        Convert.ToInt32(repo.Add(row, fields));

    private static void DeleteRows(SqlConnection c, string table, IEnumerable<int> ids)
    {
        foreach (var id in ids.Where(id => id != 0))
        {
            DeleteRow(c, table, id);
        }
    }

    private static int CountRows(SqlConnection c, string table) =>
        c.ExecuteScalar<int>($"SELECT COUNT(*) FROM dbo.{table}");

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
        var field = new CruddyField("db_col", "Alias", typeof(int), false, false, false, false);
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
    [InlineData("date", typeof(DateTime))]
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
        Assert.Equal(expected, DapperRepository.MapSqlType(sqlType, out _));
    }

    [Theory]
    [InlineData("INT", typeof(int))]
    [InlineData("NVarChar", typeof(string))]
    [InlineData("DateTime2", typeof(DateTime))]
    [InlineData("Time", typeof(TimeSpan))]
    public void MapSqlType_IsCaseInsensitive(string sqlType, Type expected)
    {
        Assert.Equal(expected, DapperRepository.MapSqlType(sqlType, out _));
    }

    [Theory]
    [InlineData("varbinary")]
    [InlineData("binary")]
    [InlineData("image")]
    [InlineData("xml")]
    [InlineData("datetimeoffset")]
    [InlineData("geography")]
    [InlineData("hierarchyid")]
    [InlineData("sql_variant")]
    [InlineData("timestamp")]
    [InlineData("")]
    [InlineData("unknown")]
    public void MapSqlType_UnsupportedType_ReturnsNull(string sqlType)
    {
        Assert.Null(DapperRepository.MapSqlType(sqlType, out var isDate));
        Assert.False(isDate);
    }

    [Theory]
    [InlineData("date", true)]
    [InlineData("DATE", true)]
    [InlineData("datetime", false)]
    [InlineData("datetime2", false)]
    [InlineData("smalldatetime", false)]
    [InlineData("time", false)]
    [InlineData("int", false)]
    [InlineData("nvarchar", false)]
    public void MapSqlType_IsDateOnlyForDateType(string sqlType, bool expectedIsDate)
    {
        DapperRepository.MapSqlType(sqlType, out var isDate);

        Assert.Equal(expectedIsDate, isDate);
    }

    // ---------- GetColumns ----------

    [Fact]
    public void GetColumns_DateTimeTypes_ReturnsAllColumnsInTableOrder()
    {
        using var c = OpenConnection();

        var columns = CreateRepo(c, DateTimeTable).GetColumns([]);

        Assert.Equal(
            ["Id", "DateTimeNotNull", "DateTimeNullable", "TimeSpanNotNull", "TimeSpanNullable", "TimeOnlyNotNull", "TimeOnlyNullable"],
            columns.Select(f => f.ColumnName));
    }

    [Fact]
    public void GetColumns_DateTimeTypes_MapsTypesNullabilityAndKey()
    {
        using var c = OpenConnection();

        var columns = CreateRepo(c, DateTimeTable).GetColumns([]).ToDictionary(f => f.ColumnName);

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
            Assert.False(actual.IsDate);
        }
    }

    [Fact]
    public void GetColumns_TestAllTypes_MapsAllSupportedTypes()
    {
        using var c = OpenConnection();

        var columns = CreateRepo(c, AllTypesTable).GetColumns([]);

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
    public void GetColumns_SetsNameEqualToColumn()
    {
        using var c = OpenConnection();

        var columns = CreateRepo(c, AllTypesTable).GetColumns([]);

        Assert.All(columns, f => Assert.Equal(f.ColumnName, f.AliasName));
    }

    [Fact]
    public void GetColumns_OnlyOnePrimaryKeyColumn()
    {
        using var c = OpenConnection();

        var columns = CreateRepo(c, AllTypesTable).GetColumns([]);

        Assert.Equal(["Id"], columns.Where(f => f.IsPrimaryKey).Select(f => f.ColumnName));
    }

    [Fact]
    public void GetColumns_UnknownTable_ReturnsEmptyList()
    {
        using var c = OpenConnection();

        var columns = CreateRepo(c, "NoSuchTable_" + Guid.NewGuid().ToString("N")).GetColumns([]);

        Assert.Empty(columns);
    }

    [Fact]
    public void GetColumns_UnknownSchema_ReturnsEmptyList()
    {
        using var c = OpenConnection();

        var columns = new DapperRepository(c, "no_such_schema", DateTimeTable, "Id").GetColumns([]);

        Assert.Empty(columns);
    }

    // ---------- Add ----------

    [Fact]
    public void Add_DateTimeTypes_ReturnsIdentityAndStoresAllValues()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, DateTimeTable);
        var row = DateTimeRow(withNullables: true);
        var id = 0;
        try
        {
            var result = repo.Add(row, DateTimeFields);
            id = Convert.ToInt32(result);

            Assert.True(id > 0);
            AssertRowMatches(row, repo.GetById(id), DateTimeFields);
        }
        finally
        {
            DeleteRows(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public void Add_DateTimeTypes_NullableValuesAreStoredAsNull()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, DateTimeTable);
        var row = DateTimeRow(withNullables: false);
        var id = 0;
        try
        {
            id = AddRow(repo, row, DateTimeFields);

            var stored = repo.GetById(id);
            AssertRowMatches(row, stored, DateTimeFields);
            Assert.Null(stored!["DateTimeNullable"]);
            Assert.Null(stored["TimeSpanNullable"]);
            Assert.Null(stored["TimeOnlyNullable"]);
        }
        finally
        {
            DeleteRows(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public void Add_DateTimeTypes_OnlySpecifiedFieldsAreInserted()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, DateTimeTable);
        var row = DateTimeRow(withNullables: true);
        var required = DateTimeFields.Where(f => !f.IsNullable).ToList();
        var id = 0;
        try
        {
            id = AddRow(repo, row, required);

            var stored = repo.GetById(id);
            AssertRowMatches(row, stored, required);
            Assert.Null(stored!["DateTimeNullable"]);
            Assert.Null(stored["TimeSpanNullable"]);
            Assert.Null(stored["TimeOnlyNullable"]);
        }
        finally
        {
            DeleteRows(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public void Add_DateTimeTypes_DateOnlyValueIsStoredAsDateTime()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, DateTimeTable);
        var row = DateTimeRow(withNullables: false);
        row["DateTimeNotNull"] = new DateOnly(2020, 5, 17);
        var id = 0;
        try
        {
            id = AddRow(repo, row, DateTimeFields);

            Assert.Equal(new DateTime(2020, 5, 17), repo.GetById(id)!["DateTimeNotNull"]);
        }
        finally
        {
            DeleteRows(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public void Add_DateTimeTypes_TimeOnlyValueIsStoredAsTime()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, DateTimeTable);
        var row = DateTimeRow(withNullables: false);
        row["TimeOnlyNotNull"] = new TimeOnly(13, 45, 30);
        row["TimeOnlyNullable"] = new TimeOnly(23, 59, 59);
        var id = 0;
        try
        {
            id = AddRow(repo, row, DateTimeFields);

            var stored = repo.GetById(id)!;
            Assert.Equal(new TimeSpan(13, 45, 30), stored["TimeOnlyNotNull"]);
            Assert.Equal(new TimeSpan(23, 59, 59), stored["TimeOnlyNullable"]);
        }
        finally
        {
            DeleteRows(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public void Add_DateTimeTypes_BoundaryValuesRoundTrip()
    {
        using var c = OpenConnection();
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
            id = AddRow(repo, row, DateTimeFields);

            AssertRowMatches(row, repo.GetById(id), DateTimeFields);
        }
        finally
        {
            DeleteRows(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public void Add_DateTimeTypes_TwoInsertsReturnDifferentIncreasingIdentities()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, DateTimeTable);
        var ids = new List<int>();
        try
        {
            ids.Add(AddRow(repo, DateTimeRow(false), DateTimeFields));
            ids.Add(AddRow(repo, DateTimeRow(true), DateTimeFields));

            Assert.True(ids[1] > ids[0]);
        }
        finally
        {
            DeleteRows(c, DateTimeTable, ids);
        }
    }

    [Fact]
    public void Add_TestAllTypes_StoresAllValues()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, AllTypesTable);
        var row = AllTypesRow(withNullables: true);
        var id = 0;
        try
        {
            id = AddRow(repo, row, AllTypesFields);

            Assert.True(id > 0);
            AssertRowMatches(row, repo.GetById(id), AllTypesFields);
        }
        finally
        {
            DeleteRows(c, AllTypesTable, [id]);
        }
    }

    [Fact]
    public void Add_TestAllTypes_NullableValuesAreStoredAsNull()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, AllTypesTable);
        var row = AllTypesRow(withNullables: false);
        var id = 0;
        try
        {
            id = AddRow(repo, row, AllTypesFields);

            var stored = repo.GetById(id);
            AssertRowMatches(row, stored, AllTypesFields);
            Assert.All(AllTypesFields.Where(f => f.IsNullable), f => Assert.Null(stored![f.AliasName]));
        }
        finally
        {
            DeleteRows(c, AllTypesTable, [id]);
        }
    }

    [Fact]
    public void Add_TestAllTypes_ReturnsIdentityOfNewRow()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, AllTypesTable);
        var guid = Guid.NewGuid();
        var id = 0;
        try
        {
            id = AddRow(repo, AllTypesRow(false, guid), AllTypesFields);

            var storedId = c.ExecuteScalar<int>($"SELECT Id FROM dbo.{AllTypesTable} WHERE GuidNotNull = @guid", new { guid });
            Assert.Equal(storedId, id);
        }
        finally
        {
            DeleteRows(c, AllTypesTable, [id]);
        }
    }

    [Fact]
    public void Add_TestAllTypes_ExtremeNumericValuesRoundTrip()
    {
        using var c = OpenConnection();
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
            id = AddRow(repo, row, AllTypesFields);

            AssertRowMatches(row, repo.GetById(id), AllTypesFields);
        }
        finally
        {
            DeleteRows(c, AllTypesTable, [id]);
        }
    }

    [Fact]
    public void Add_MissingValueForNotNullColumn_Throws_AndInsertsNothing()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, AllTypesTable);
        var row = AllTypesRow(withNullables: false);
        row.Remove("StringNotNull");
        var before = CountRows(c, AllTypesTable);

        Assert.ThrowsAny<SqlException>(() => repo.Add(row, AllTypesFields));

        Assert.Equal(before, CountRows(c, AllTypesTable));
    }

    [Fact]
    public void Add_NullRow_ThrowsArgumentNullException()
    {
        using var c = OpenConnection();

        Assert.Throws<ArgumentNullException>(() => CreateRepo(c, DateTimeTable).Add(null!, DateTimeFields));
    }

    [Fact]
    public void Add_NoFields_ReturnsZeroAndInsertsNothing()
    {
        using var c = OpenConnection();
        var before = CountRows(c, DateTimeTable);

        var result = CreateRepo(c, DateTimeTable).Add(DateTimeRow(true), []);

        Assert.Equal(0, result);
        Assert.Equal(before, CountRows(c, DateTimeTable));
    }

    // ---------- GetById ----------

    [Fact]
    public void GetById_DateTimeTypes_ReturnsRowWithAllColumns()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, DateTimeTable);
        var row = DateTimeRow(withNullables: true);
        var id = 0;
        try
        {
            id = AddRow(repo, row, DateTimeFields);

            var stored = repo.GetById(id);

            Assert.NotNull(stored);
            Assert.Equal(DateTimeFields.Length + 1, stored.Count);
            Assert.Equal(id, stored["Id"]);
            AssertRowMatches(row, stored, DateTimeFields);
        }
        finally
        {
            DeleteRows(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public void GetById_ColumnNamesAreCaseInsensitive()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, DateTimeTable);
        var id = 0;
        try
        {
            id = AddRow(repo, DateTimeRow(true), DateTimeFields);

            var stored = repo.GetById(id)!;

            Assert.Equal(stored["DateTimeNotNull"], stored["DATETIMENOTNULL"]);
            Assert.True(stored.ContainsKey("id"));
        }
        finally
        {
            DeleteRows(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public void GetById_SpecificColumns_ReturnsOnlyThoseColumns()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, DateTimeTable);
        var id = 0;
        try
        {
            id = AddRow(repo, DateTimeRow(true), DateTimeFields);

            var stored = repo.GetById(id, "Id, TimeSpanNotNull");

            Assert.NotNull(stored);
            Assert.Equal(["Id", "TimeSpanNotNull"], stored.Keys);
            Assert.Equal(new TimeSpan(1, 2, 3), stored["TimeSpanNotNull"]);
        }
        finally
        {
            DeleteRows(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public void GetById_UnknownId_ReturnsNull()
    {
        using var c = OpenConnection();

        Assert.Null(CreateRepo(c, DateTimeTable).GetById(-1));
    }

    [Fact]
    public void GetById_DeletedRow_ReturnsNull()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, DateTimeTable);
        var id = AddRow(repo, DateTimeRow(false), DateTimeFields);
        DeleteRow(c, DateTimeTable, id);

        Assert.Null(repo.GetById(id));
    }

    [Fact]
    public void GetById_ReturnsOnlyTheRequestedRow()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, AllTypesTable);
        var first = AllTypesRow(false);
        var second = AllTypesRow(true);
        var ids = new List<int>();
        try
        {
            ids.Add(AddRow(repo, first, AllTypesFields));
            ids.Add(AddRow(repo, second, AllTypesFields));

            var stored = repo.GetById(ids[1]);

            Assert.Equal(ids[1], stored!["Id"]);
            AssertRowMatches(second, stored, AllTypesFields);
        }
        finally
        {
            DeleteRows(c, AllTypesTable, ids);
        }
    }

    [Fact]
    public void GetById_UsesConfiguredKeyColumn_WithGuidKey()
    {
        using var c = OpenConnection();
        var setup = CreateRepo(c, AllTypesTable);
        var guid = Guid.NewGuid();
        var id = 0;
        try
        {
            id = AddRow(setup, AllTypesRow(false, guid), AllTypesFields);

            var stored = CreateRepo(c, AllTypesTable, "GuidNotNull").GetById(guid);

            Assert.NotNull(stored);
            Assert.Equal(id, stored["Id"]);
            Assert.Equal(guid, stored["GuidNotNull"]);
        }
        finally
        {
            DeleteRows(c, AllTypesTable, [id]);
        }
    }

    // ---------- GetAll ----------

    [Fact]
    public void GetAll_DateTimeTypes_ReturnsMatchingRowsInSqlOrder()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, DateTimeTable);
        var first = DateTimeRow(false);
        var second = DateTimeRow(true);
        var ids = new List<int>();
        try
        {
            ids.Add(AddRow(repo, first, DateTimeFields));
            ids.Add(AddRow(repo, second, DateTimeFields));

            var rows = repo.GetAll($"SELECT * FROM dbo.{DateTimeTable} WHERE Id IN ({ids[0]}, {ids[1]}) ORDER BY Id DESC");

            Assert.Equal(2, rows.Count);
            Assert.Equal(ids[1], rows[0]["Id"]);
            Assert.Equal(ids[0], rows[1]["Id"]);
            AssertRowMatches(second, rows[0], DateTimeFields);
            AssertRowMatches(first, rows[1], DateTimeFields);
        }
        finally
        {
            DeleteRows(c, DateTimeTable, ids);
        }
    }

    [Fact]
    public void GetAll_TestAllTypes_ReturnsAllTypedValues()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, AllTypesTable);
        var row = AllTypesRow(true);
        var id = 0;
        try
        {
            id = AddRow(repo, row, AllTypesFields);

            var rows = repo.GetAll($"SELECT * FROM dbo.{AllTypesTable} WHERE Id = {id}");

            var stored = Assert.Single(rows);
            AssertRowMatches(row, stored, AllTypesFields);
        }
        finally
        {
            DeleteRows(c, AllTypesTable, [id]);
        }
    }

    [Fact]
    public void GetAll_NullValuesAreReturnedAsNull()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, AllTypesTable);
        var id = 0;
        try
        {
            id = AddRow(repo, AllTypesRow(false), AllTypesFields);

            var stored = Assert.Single(repo.GetAll($"SELECT * FROM dbo.{AllTypesTable} WHERE Id = {id}"));

            Assert.All(AllTypesFields.Where(f => f.IsNullable), f => Assert.Null(stored[f.AliasName]));
        }
        finally
        {
            DeleteRows(c, AllTypesTable, [id]);
        }
    }

    [Fact]
    public void GetAll_NoMatchingRows_ReturnsEmptyList()
    {
        using var c = OpenConnection();

        var rows = CreateRepo(c, DateTimeTable).GetAll($"SELECT * FROM dbo.{DateTimeTable} WHERE Id = -1");

        Assert.NotNull(rows);
        Assert.Empty(rows);
    }

    [Fact]
    public void GetAll_RespectsSelectedColumnsAndAliases()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, DateTimeTable);
        var id = 0;
        try
        {
            id = AddRow(repo, DateTimeRow(true), DateTimeFields);

            var stored = Assert.Single(repo.GetAll($"SELECT Id AS Key1, DateTimeNotNull AS Moment FROM dbo.{DateTimeTable} WHERE Id = {id}"));

            Assert.Equal(["Key1", "Moment"], stored.Keys);
            Assert.Equal(new DateTime(2021, 3, 4, 5, 6, 7), stored["moment"]);
        }
        finally
        {
            DeleteRows(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public void GetAll_ReturnsNewListOnEachCall()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, DateTimeTable);
        var sql = $"SELECT * FROM dbo.{DateTimeTable} WHERE Id = -1";

        Assert.NotSame(repo.GetAll(sql), repo.GetAll(sql));
    }

    // ---------- Update ----------

    [Fact]
    public void Update_DateTimeTypes_UpdatesAllFields()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, DateTimeTable);
        var id = 0;
        try
        {
            id = AddRow(repo, DateTimeRow(false), DateTimeFields);
            var updated = new CruddyRow
            {
                ["DateTimeNotNull"] = new DateTime(2000, 1, 2, 3, 4, 5),
                ["DateTimeNullable"] = new DateTime(2001, 2, 3, 4, 5, 6),
                ["TimeSpanNotNull"] = new TimeSpan(11, 12, 13),
                ["TimeSpanNullable"] = new TimeSpan(14, 15, 16),
                ["TimeOnlyNotNull"] = new TimeSpan(17, 18, 19),
                ["TimeOnlyNullable"] = new TimeSpan(20, 21, 22),
            };

            var affected = repo.Update(updated, id, DateTimeFields);

            Assert.Equal(1, affected);
            AssertRowMatches(updated, repo.GetById(id), DateTimeFields);
        }
        finally
        {
            DeleteRows(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public void Update_DateTimeTypes_OnlyUpdatesSpecifiedFields()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, DateTimeTable);
        var original = DateTimeRow(true);
        var id = 0;
        try
        {
            id = AddRow(repo, original, DateTimeFields);
            var newTime = new TimeSpan(22, 33, 44);
            var change = new CruddyRow { ["TimeSpanNotNull"] = newTime, ["DateTimeNotNull"] = new DateTime(1999, 9, 9) };

            var affected = repo.Update(change, id, [DateTimeFields[2]]);

            Assert.Equal(1, affected);
            var stored = repo.GetById(id)!;
            Assert.Equal(newTime, stored["TimeSpanNotNull"]);
            Assert.Equal(original["DateTimeNotNull"], stored["DateTimeNotNull"]);
            Assert.Equal(original["TimeOnlyNullable"], stored["TimeOnlyNullable"]);
        }
        finally
        {
            DeleteRows(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public void Update_DateTimeTypes_CanSetNullableValuesToNullAndBack()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, DateTimeTable);
        var nullableFields = DateTimeFields.Where(f => f.IsNullable).ToList();
        var id = 0;
        try
        {
            id = AddRow(repo, DateTimeRow(true), DateTimeFields);

            repo.Update(DateTimeRow(false), id, nullableFields);
            var cleared = repo.GetById(id)!;
            Assert.All(nullableFields, f => Assert.Null(cleared[f.AliasName]));

            var restored = DateTimeRow(true);
            repo.Update(restored, id, nullableFields);
            AssertRowMatches(restored, repo.GetById(id), nullableFields);
        }
        finally
        {
            DeleteRows(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public void Update_DateTimeTypes_DateOnlyAndTimeOnlyValues()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, DateTimeTable);
        var id = 0;
        try
        {
            id = AddRow(repo, DateTimeRow(false), DateTimeFields);
            var change = new CruddyRow
            {
                ["DateTimeNotNull"] = new DateOnly(2030, 6, 7),
                ["TimeOnlyNotNull"] = new TimeOnly(8, 9, 10),
            };

            var affected = repo.Update(change, id, [DateTimeFields[0], DateTimeFields[4]]);

            Assert.Equal(1, affected);
            var stored = repo.GetById(id)!;
            Assert.Equal(new DateTime(2030, 6, 7), stored["DateTimeNotNull"]);
            Assert.Equal(new TimeSpan(8, 9, 10), stored["TimeOnlyNotNull"]);
        }
        finally
        {
            DeleteRows(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public void Update_TestAllTypes_UpdatesAllFields()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, AllTypesTable);
        var id = 0;
        try
        {
            id = AddRow(repo, AllTypesRow(false), AllTypesFields);
            var updated = AllTypesRow(true);
            updated["Int32NotNull"] = 456;
            updated["StringNotNull"] = "Updated";
            updated["BoolNotNull"] = false;

            var affected = repo.Update(updated, id, AllTypesFields);

            Assert.Equal(1, affected);
            AssertRowMatches(updated, repo.GetById(id), AllTypesFields);
        }
        finally
        {
            DeleteRows(c, AllTypesTable, [id]);
        }
    }

    [Fact]
    public void Update_TestAllTypes_SetsNullableValuesToNull()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, AllTypesTable);
        var id = 0;
        try
        {
            id = AddRow(repo, AllTypesRow(true), AllTypesFields);
            var updated = AllTypesRow(false);

            repo.Update(updated, id, AllTypesFields);

            var stored = repo.GetById(id)!;
            Assert.All(AllTypesFields.Where(f => f.IsNullable), f => Assert.Null(stored[f.AliasName]));
        }
        finally
        {
            DeleteRows(c, AllTypesTable, [id]);
        }
    }

    [Fact]
    public void Update_OnlyAffectsRowWithGivenKey()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, AllTypesTable);
        var untouched = AllTypesRow(true);
        var ids = new List<int>();
        try
        {
            ids.Add(AddRow(repo, AllTypesRow(false), AllTypesFields));
            ids.Add(AddRow(repo, untouched, AllTypesFields));

            repo.Update(AllTypesRow(true), ids[0], AllTypesFields);

            AssertRowMatches(untouched, repo.GetById(ids[1]), AllTypesFields);
        }
        finally
        {
            DeleteRows(c, AllTypesTable, ids);
        }
    }

    [Fact]
    public void Update_UnknownKey_ReturnsZero()
    {
        using var c = OpenConnection();

        var affected = CreateRepo(c, AllTypesTable).Update(AllTypesRow(true), -1, AllTypesFields);

        Assert.Equal(0, affected);
    }

    [Fact]
    public void Update_NoFields_ReturnsZeroAndLeavesRowUnchanged()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, DateTimeTable);
        var original = DateTimeRow(true);
        var id = 0;
        try
        {
            id = AddRow(repo, original, DateTimeFields);

            var affected = repo.Update(DateTimeRow(false), id, []);

            Assert.Equal(0, affected);
            AssertRowMatches(original, repo.GetById(id), DateTimeFields);
        }
        finally
        {
            DeleteRows(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public void Update_NullRow_ThrowsArgumentNullException()
    {
        using var c = OpenConnection();

        Assert.Throws<ArgumentNullException>(() => CreateRepo(c, DateTimeTable).Update(null!, 1, DateTimeFields));
    }

    [Fact]
    public void Update_NullForNotNullColumn_Throws_AndKeepsValue()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, AllTypesTable);
        var original = AllTypesRow(false);
        var id = 0;
        try
        {
            id = AddRow(repo, original, AllTypesFields);
            var invalid = AllTypesRow(false);
            invalid["StringNotNull"] = null;

            Assert.ThrowsAny<SqlException>(() => repo.Update(invalid, id, AllTypesFields));

            AssertRowMatches(original, repo.GetById(id), AllTypesFields);
        }
        finally
        {
            DeleteRows(c, AllTypesTable, [id]);
        }
    }

    // ---------- Delete ----------

    [Fact]
    public void Delete_DateTimeTypes_RemovesRow()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, DateTimeTable);
        var id = 0;
        try
        {
            id = AddRow(repo, DateTimeRow(true), DateTimeFields);
            Assert.NotNull(repo.GetById(id));

            repo.Delete(id);

            Assert.Null(repo.GetById(id));
        }
        finally
        {
            DeleteRows(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public void Delete_TestAllTypes_RemovesRow()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, AllTypesTable);
        var id = 0;
        try
        {
            id = AddRow(repo, AllTypesRow(true), AllTypesFields);

            repo.Delete(id);

            Assert.Null(repo.GetById(id));
        }
        finally
        {
            DeleteRows(c, AllTypesTable, [id]);
        }
    }

    [Fact]
    public void Delete_OnlyRemovesRowWithGivenKey()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, AllTypesTable);
        var kept = AllTypesRow(true);
        var ids = new List<int>();
        try
        {
            ids.Add(AddRow(repo, AllTypesRow(false), AllTypesFields));
            ids.Add(AddRow(repo, kept, AllTypesFields));
            var before = CountRows(c, AllTypesTable);

            repo.Delete(ids[0]);

            Assert.Equal(before - 1, CountRows(c, AllTypesTable));
            Assert.Null(repo.GetById(ids[0]));
            AssertRowMatches(kept, repo.GetById(ids[1]), AllTypesFields);
        }
        finally
        {
            DeleteRows(c, AllTypesTable, ids);
        }
    }

    [Fact]
    public void Delete_UnknownKey_DoesNotThrowAndDeletesNothing()
    {
        using var c = OpenConnection();
        var before = CountRows(c, DateTimeTable);

        var exception = Record.Exception(() => CreateRepo(c, DateTimeTable).Delete(-1));

        Assert.Null(exception);
        Assert.Equal(before, CountRows(c, DateTimeTable));
    }

    [Fact]
    public void Delete_CalledTwice_SecondCallIsHarmless()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, DateTimeTable);
        var id = 0;
        try
        {
            id = AddRow(repo, DateTimeRow(false), DateTimeFields);
            repo.Delete(id);

            var exception = Record.Exception(() => repo.Delete(id));

            Assert.Null(exception);
        }
        finally
        {
            DeleteRows(c, DateTimeTable, [id]);
        }
    }

    // ---------- GetFKeyOptions ----------

    [Fact]
    public void GetFKeyOptions_TestAllTypes_ReturnsIdAndDisplayOrderedByDisplay()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, AllTypesTable);
        var prefix = "fk-" + Guid.NewGuid().ToString("N");
        var rowB = AllTypesRow(false);
        rowB["StringNotNull"] = prefix + "-B";
        var rowA = AllTypesRow(false);
        rowA["StringNotNull"] = prefix + "-A";
        var ids = new List<int>();
        try
        {
            ids.Add(AddRow(repo, rowB, AllTypesFields));
            ids.Add(AddRow(repo, rowA, AllTypesFields));

            var options = repo.GetFKeyOptions(new FKeyInfo("FK", $"dbo.{AllTypesTable}", "Id", "StringNotNull"));

            var mine = options.Where(o => ((string)o.Display!).StartsWith(prefix)).ToList();
            Assert.Equal(2, mine.Count);
            Assert.Equal([prefix + "-A", prefix + "-B"], mine.Select(o => o.Display));
            Assert.Equal([ids[1], ids[0]], mine.Select(o => (int)o.Id!));
        }
        finally
        {
            DeleteRows(c, AllTypesTable, ids);
        }
    }

    [Fact]
    public void GetFKeyOptions_ResultIsSortedByDisplay()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, AllTypesTable);
        var prefix = "fk-" + Guid.NewGuid().ToString("N");
        var ids = new List<int>();
        try
        {
            foreach (var suffix in new[] { "3", "1", "2" })
            {
                var row = AllTypesRow(false);
                row["StringNotNull"] = prefix + suffix;
                ids.Add(AddRow(repo, row, AllTypesFields));
            }

            var options = repo.GetFKeyOptions(new FKeyInfo("FK", $"dbo.{AllTypesTable}", "Id", "StringNotNull"));

            var mine = options.Select(o => (string)o.Display!).Where(d => d.StartsWith(prefix)).ToList();
            Assert.Equal([prefix + "1", prefix + "2", prefix + "3"], mine);
        }
        finally
        {
            DeleteRows(c, AllTypesTable, ids);
        }
    }

    [Fact]
    public void GetFKeyOptions_DateTimeTypes_MapsTimeSpanDisplay()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, DateTimeTable);
        var row = DateTimeRow(false);
        var id = 0;
        try
        {
            id = AddRow(repo, row, DateTimeFields);

            var options = repo.GetFKeyOptions(new FKeyInfo("FK", $"dbo.{DateTimeTable}", "Id", "TimeSpanNotNull"));

            var option = Assert.Single(options, o => (int)o.Id! == id);
            Assert.Equal(new TimeSpan(1, 2, 3), option.Display);
        }
        finally
        {
            DeleteRows(c, DateTimeTable, [id]);
        }
    }

    [Fact]
    public void GetFKeyOptions_NullDisplayValue_IsReturnedAsNull()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, AllTypesTable);
        var id = 0;
        try
        {
            id = AddRow(repo, AllTypesRow(false), AllTypesFields);

            var options = repo.GetFKeyOptions(new FKeyInfo("FK", $"dbo.{AllTypesTable}", "Id", "StringNullable"));

            Assert.Null(Assert.Single(options, o => (int)o.Id! == id).Display);
        }
        finally
        {
            DeleteRows(c, AllTypesTable, [id]);
        }
    }

    [Fact]
    public void GetFKeyOptions_DeletedRowIsNoLongerListed()
    {
        using var c = OpenConnection();
        var repo = CreateRepo(c, AllTypesTable);
        var id = 0;
        try
        {
            id = AddRow(repo, AllTypesRow(false), AllTypesFields);
            var info = new FKeyInfo("FK", $"dbo.{AllTypesTable}", "Id", "StringNotNull");
            Assert.Contains(repo.GetFKeyOptions(info), o => (int)o.Id! == id);

            repo.Delete(id);

            Assert.DoesNotContain(repo.GetFKeyOptions(info), o => (int)o.Id! == id);
        }
        finally
        {
            DeleteRows(c, AllTypesTable, [id]);
        }
    }
}
