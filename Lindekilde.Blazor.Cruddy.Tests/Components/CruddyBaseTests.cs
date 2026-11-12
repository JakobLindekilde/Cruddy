#pragma warning disable BL0005 // Component parameter should not be set outside of its component.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

using Lindekilde.Blazor.Cruddy.Components;
using Lindekilde.Blazor.Cruddy.Repositories;

namespace Lindekilde.Blazor.Cruddy.Tests.Components;

public class CruddyBaseTests
{
    private sealed class TestableCruddy : CruddyBase
    {
        public TestableCruddy() => RepositoryFactory = new DapperRepositoryFactory();

        public Task InitializeAsync() => OnInitializedAsync();
        public List<CruddyRow> LoadedRows => Rows;
        public void SetFields(params CruddyField[] fields)
        {
            AllColumns = [.. fields];
            Fields = [.. fields];
        }
    }

    private static CruddyField Field(string column, Type? type = null, bool nullable = true, bool pk = false, bool readOnly = false) =>
        new(column, column, type ?? typeof(string), nullable, pk, readOnly, false);

    private static CruddyBase Create(string tableColumns = "*") =>
        new() { TableName = "Persons", TableColumns = tableColumns, DbConnection = null };

    #region BuildSql

    [Theory]
    [InlineData("", "Id, Name", "Persons", "SELECT TOP 10000  Id, Name FROM dbo.Persons")]
    [InlineData("Any sql", "Id, Name", "Persons", "Any sql")]
    public void BuildSql_ReturnsSelectSql(string select, string tableColumns, string tableName, string expected)
    {
        var dut = new CruddyBase { Select = select, TableColumns = tableColumns, TableName = tableName, DbConnection = null };
        Assert.Equal(expected, dut.BuildSql());
    }

    [Theory]
    [InlineData(false, 0, "*", "dbo", "Persons", "", "SELECT  * FROM dbo.Persons")]
    [InlineData(false, 0, "Id, Name", "dbo", "Persons", "", "SELECT  Id, Name FROM dbo.Persons")]
    [InlineData(false, 100, "Id, Name", "dbo", "Persons", "", "SELECT TOP 100  Id, Name FROM dbo.Persons")]
    [InlineData(false, 100, "Id, Name", "dbo", "Persons", "Name = 'Bob'", "SELECT TOP 100  Id, Name FROM dbo.Persons WHERE Name = 'Bob'")]
    [InlineData(true, 0, "Name", "dbo", "Persons", "", "SELECT DISTINCT  Name FROM dbo.Persons")]
    [InlineData(true, 3, "Name", "dbo", "Persons", "", "SELECT DISTINCT TOP 3  Name FROM dbo.Persons")]
    [InlineData(false, 0, "Id, Name", "sales", "Persons", "", "SELECT  Id, Name FROM sales.Persons")]
    public void BuildSql_ReturnsExpectedSql(bool distinct, int top, string tableColumns, string defaultSchema,
        string tableName, string where, string expected)
    {
        var dut = new CruddyBase
        {
            Distinct = distinct,
            Top = top,
            TableColumns = tableColumns,
            DefaultSchema = defaultSchema,
            TableName = tableName,
            Where = where,
            DbConnection = null
        };
        Assert.Equal(expected, dut.BuildSql());
    }

    [Theory]
    [InlineData("", SortOrder.None, "SELECT TOP 10000  Id, Name FROM dbo.Persons")]
    [InlineData("Name", SortOrder.None, "SELECT TOP 10000  Id, Name FROM dbo.Persons ORDER BY Name")]
    [InlineData("Name", SortOrder.Ascending, "SELECT TOP 10000  Id, Name FROM dbo.Persons ORDER BY Name ASC")]
    [InlineData("Name", SortOrder.Descending, "SELECT TOP 10000  Id, Name FROM dbo.Persons ORDER BY Name DESC")]
    [InlineData("Name DESC", SortOrder.Ascending, "SELECT TOP 10000  Id, Name FROM dbo.Persons ORDER BY Name DESC")]
    public void BuildSql_ReturnsExpectedSqlWithOrderBy(string orderBy, SortOrder sortOrder, string expected)
    {
        var dut = new CruddyBase
        {
            TableColumns = "Id, Name",
            TableName = "Persons",
            OrderBy = orderBy,
            SortOrder = sortOrder,
            DbConnection = null
        };
        Assert.Equal(expected, dut.BuildSql());
    }

    #endregion

    #region ColumnAliasDict and GetFields

    [Theory]
    [InlineData("Id, Name, Email", "Id:Id|Name:Name|Email:Email")]
    [InlineData("Id AS PersonId, Name AS PersonName, Email", "Id:PersonId|Name:PersonName|Email:Email")]
    [InlineData("Id, Name, Name AS Name2", "Id:Id|Name:Name")]
    [InlineData("Id, Name AS Name1, Name", "Id:Id|Name:Name1")]
    [InlineData("Id, Name AS [PersonName]", "Id:Id|Name:PersonName")]
    public void FillColumnAliasDict_FillsDictionary(string tableColumns, string expected)
    {
        var dut = Create(tableColumns);

        dut.FillColumnAliasDict();

        var actual = string.Join("|", dut.ColumnAliasDict.Select(kv => $"{kv.Key}:{kv.Value}"));
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetFields_Star_ReturnsAllColumns()
    {
        var dut = new TestableCruddy { TableName = "Persons", DbConnection = null };
        dut.SetFields(Field("Id", typeof(int), false, true), Field("Name"), Field("Email"));

        var fields = dut.GetFields("*");

        Assert.Equal(["Id", "Name", "Email"], fields.Select(f => f.AliasName));
    }

    [Fact]
    public void GetFields_ExplicitColumns_ReturnsSubsetInSpecifiedOrderWithAliases()
    {
        var dut = new TestableCruddy { TableName = "Persons", DbConnection = null };
        dut.SetFields(Field("Id", typeof(int), false, true), Field("Name"), Field("Email"));

        var fields = dut.GetFields("Email AS Mail, id, Unknown");

        Assert.Equal(["Mail", "id"], fields.Select(f => f.AliasName));
        Assert.Equal(["Email", "Id"], fields.Select(f => f.ColumnName));
    }

    #endregion

    #region Defaults

    [Fact]
    public void Defaults_AreExpected()
    {
        var dut = new CruddyBase { DbConnection = null };
        Assert.Equal("dbo", dut.DefaultSchema);
        Assert.True(dut.PluralizeTableName);
        Assert.Equal("*", dut.TableColumns);
        Assert.Equal(string.Empty, dut.TableName);
        Assert.Null(dut.KeyColumn);
        Assert.False(dut.AllowKeyColumnEditOnCreate);
        Assert.Equal(10000, dut.Top);
        Assert.False(dut.Distinct);
        Assert.Null(dut.OrderBy);
        Assert.Null(dut.Where);
        Assert.Null(dut.Select);
        Assert.Equal(SortOrder.Ascending, dut.SortOrder);
    }

    #endregion

    #region IncludeColumn and GetFieldsForCreateOrUpdate

    [Theory]
    [InlineData(CrudOperation.Create, false, false)]
    [InlineData(CrudOperation.Create, true, true)]
    [InlineData(CrudOperation.Update, false, false)]
    [InlineData(CrudOperation.Update, true, false)]
    public void IncludeColumn_KeyColumn(CrudOperation operation, bool allowKeyOnCreate, bool expected)
    {
        var dut = new CruddyBase { KeyColumn = "Id", AllowKeyColumnEditOnCreate = allowKeyOnCreate, DbConnection = null };
        Assert.Equal(expected, dut.IncludeColumn(operation, "id"));
    }

    [Theory]
    [InlineData(CrudOperation.Create)]
    [InlineData(CrudOperation.Update)]
    public void IncludeColumn_NonKeyColumn_IsIncluded(CrudOperation operation)
    {
        var dut = new CruddyBase { KeyColumn = "Id", DbConnection = null };
        Assert.True(dut.IncludeColumn(operation, "Name"));
    }

    [Fact]
    public void GetFieldsForCreateOrUpdate_ExcludesKeyAndReadOnlyFields()
    {
        var dut = new TestableCruddy { KeyColumn = "Id", DbConnection = null };
        dut.SetFields(Field("Id", typeof(int), false, true), Field("Name"), Field("RowVer", readOnly: true));

        Assert.Equal(["Name"], dut.GetFieldsForCreateOrUpdate(CrudOperation.Create).Select(f => f.ColumnName));
        Assert.Equal(["Name"], dut.GetFieldsForCreateOrUpdate(CrudOperation.Update).Select(f => f.ColumnName));
    }

    [Fact]
    public void GetFieldsForCreateOrUpdate_ReadOperation_Throws()
    {
        var dut = Create();
        Assert.Throws<ArgumentException>(() => dut.GetFieldsForCreateOrUpdate(CrudOperation.Read));
    }

    #endregion

    #region Rows and keys

    [Fact]
    public void GetCell_ReturnsValueOrNull()
    {
        var row = new CruddyRow { ["Id"] = 1, ["Name"] = DBNull.Value };

        Assert.Equal(1, CruddyBase.GetCell(row, "Id"));
        Assert.Null(CruddyBase.GetCell(row, "Name"));
        Assert.Null(CruddyBase.GetCell(row, "Missing"));
    }

    [Fact]
    public void GetKeyValue_UsesAliasOfKeyField()
    {
        var dut = new TestableCruddy { KeyColumn = "Id", DbConnection = null };
        dut.SetFields(Field("Id", typeof(int), false, true) with { AliasName = "PersonId" });

        Assert.Equal(5, dut.GetKeyValue(new CruddyRow { ["PersonId"] = 5 }));
    }

    #endregion

    #region Create, Update, Delete, GetTableRow

    [Fact]
    public void Create_InsertsWritableFields()
    {
        var db = new FakeDbConnection();
        var dut = new TestableCruddy { TableName = "Persons", KeyColumn = "Id", DbConnection = db };
        dut.SetFields(Field("Id", typeof(int), false, true), Field("Name"), Field("Email"));

        var result = dut.Create(new CruddyRow { ["Name"] = "Ann", ["Email"] = "a@x.dk" });

        Assert.Equal(42, result);
        var cmd = Assert.Single(db.Commands);
        Assert.StartsWith("INSERT INTO dbo.Persons ([Name], [Email]) VALUES (@p0, @p1)", cmd.Sql);
        Assert.Equal("Ann", cmd.Parameters["p0"]);
        Assert.Equal("a@x.dk", cmd.Parameters["p1"]);
    }

    [Fact]
    public void Create_NullRow_Throws()
    {
        var dut = Create();
        Assert.Throws<ArgumentNullException>(() => dut.Create(null));
    }

    [Fact]
    public void Update_UpdatesWritableFieldsByKey()
    {
        var db = new FakeDbConnection();
        var dut = new TestableCruddy { TableName = "Persons", KeyColumn = "Id", DbConnection = db };
        dut.SetFields(Field("Id", typeof(int), false, true), Field("Name"));

        var affected = dut.Update(new CruddyRow { ["Id"] = 3, ["Name"] = "Bob" }, 3);

        Assert.Equal(1, affected);
        var cmd = Assert.Single(db.Commands);
        Assert.Equal("UPDATE dbo.Persons SET [Name] = @p0 WHERE Id = @keyValue", cmd.Sql);
        Assert.Equal("Bob", cmd.Parameters["p0"]);
        Assert.Equal(3, cmd.Parameters["keyValue"]);
    }

    [Fact]
    public void Update_NullRow_Throws()
    {
        var dut = Create();
        Assert.Throws<ArgumentNullException>(() => dut.Update(null, 1));
    }

    [Fact]
    public void Delete_ExecutesDeleteSql()
    {
        var db = new FakeDbConnection { ScalarResult = null };
        var dut = new TestableCruddy { TableName = "Persons", KeyColumn = "Id", DbConnection = db };

        var result = dut.Delete(3);

        Assert.Null(result);
        var cmd = Assert.Single(db.Commands);
        Assert.Equal("DELETE FROM dbo.Persons WHERE Id = @keyValue", cmd.Sql);
        Assert.Equal(3, cmd.Parameters["keyValue"]);
    }

    [Fact]
    public void GetTableRow_ReturnsRow()
    {
        var db = new FakeDbConnection();
        db.Rows.Add(new Dictionary<string, object?> { ["Id"] = 4, ["Name"] = "Ann" });
        var dut = new TestableCruddy { TableName = "Persons", KeyColumn = "Id", DbConnection = db };

        var row = dut.GetTableRow(4);

        Assert.NotNull(row);
        Assert.Equal(4, row["Id"]);
        Assert.Equal("Ann", row["Name"]);
        var cmd = Assert.Single(db.Commands);
        Assert.Equal("SELECT * FROM dbo.Persons WHERE Id = @Id", cmd.Sql);
        Assert.Equal(4, cmd.Parameters["Id"]);
    }

    [Fact]
    public void GetTableRow_UsesColumnsParameter()
    {
        var db = new FakeDbConnection();
        var dut = new TestableCruddy { TableName = "Persons", KeyColumn = "Id", DbConnection = db };

        dut.GetTableRow(1, "Id, Name");

        Assert.Equal("SELECT Id, Name FROM dbo.Persons WHERE Id = @Id", db.Commands[0].Sql);
    }

    [Fact]
    public void GetTableRow_NotFound_ReturnsNull()
    {
        var dut = new TestableCruddy { TableName = "Persons", KeyColumn = "Id", DbConnection = new FakeDbConnection() };
        Assert.Null(dut.GetTableRow(123));
    }

    #endregion

    #region OnInitializedAsync

    [Fact]
    public async Task OnInitializedAsync_NoTableName_Throws()
    {
        var dut = new TestableCruddy { DbConnection = new FakeDbConnection() };
        await Assert.ThrowsAsync<InvalidOperationException>(dut.InitializeAsync);
    }

    [Fact]
    public async Task OnInitializedAsync_UnknownTable_Throws()
    {
        var dut = new TestableCruddy { TableName = "Nope", DbConnection = new FakeDbConnection() };
        await Assert.ThrowsAsync<InvalidOperationException>(dut.InitializeAsync);
    }

    [Fact]
    public async Task OnInitializedAsync_ReadsCatalogInfersKeyAndSelectsFields()
    {
        var db = new FakeDbConnection();
        db.Rows.Add(ColumnRow("Id", "int", false, 1));
        db.Rows.Add(ColumnRow("Name", "nvarchar", true, 0));
        db.Rows.Add(ColumnRow("Blob", "varbinary", true, 0));
        var dut = new TestableCruddy { TableName = "Persons", TableColumns = "*", DbConnection = db };

        await dut.InitializeAsync();

        Assert.Equal("Id", dut.KeyColumn);
        Assert.Equal(["Id", "Name"], dut.Fields.Select(f => f.ColumnName));
        Assert.Equal(typeof(int), dut.Fields[0].Type);
        Assert.False(dut.Fields[0].IsNullable);
        Assert.True(dut.Fields[0].IsPrimaryKey);
        Assert.Equal(typeof(string), dut.Fields[1].Type);
        Assert.Equal(2, db.Commands.Count);
        Assert.Contains("sys.columns", db.Commands[0].Sql);
        Assert.Equal("SELECT TOP 10000  * FROM dbo.Persons", db.Commands[1].Sql);
    }

    [Fact]
    public async Task OnInitializedAsync_ExplicitColumns_SelectsSubset()
    {
        var db = new FakeDbConnection();
        db.Rows.Add(ColumnRow("Id", "int", false, 1));
        db.Rows.Add(ColumnRow("Name", "nvarchar", true, 0));
        db.Rows.Add(ColumnRow("Email", "nvarchar", true, 0));
        var dut = new TestableCruddy { TableName = "Persons", TableColumns = "Name AS Who, Id", DbConnection = db };

        await dut.InitializeAsync();

        Assert.Equal(["Who", "Id"], dut.Fields.Select(f => f.AliasName));
        Assert.Equal(2, dut.ColumnAliasDict.Count);
    }

    private static Dictionary<string, object?> ColumnRow(string name, string typeName, bool nullable, int pk) =>
        new()
        {
            ["Name"] = name,
            ["TypeName"] = typeName,
            ["IsNullable"] = nullable,
            ["IsPrimaryKey"] = pk,
            ["IsComputed"] = false
        };

    #endregion
}
#pragma warning restore BL0005
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
