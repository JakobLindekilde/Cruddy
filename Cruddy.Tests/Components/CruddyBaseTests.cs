#pragma warning disable BL0005 // Component parameter should not be set outside of its component.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

using Cruddy.Components;
using Cruddy.Tests.Models;

namespace Cruddy.Tests.Components;

public class CruddyBaseTests
{
    #region BuildSql

    [Theory]
    [InlineData("", "Id, Name", "Persons", "SELECT TOP 10000  Id, Name FROM dbo.Persons")]
    [InlineData("Any sql", "Id, Name", "Persons", "Any sql")]
    [InlineData("Any sql", "", "", "Any sql")]
    public void BuildSql_ReturnsSelectSql(
        string select,
        string tableColumns,
        string tableName,
        string expected)
    {
        var dut = new CruddyBase<Person>
        {
            Select = select,
            TableColumns = tableColumns,
            TableName = tableName,
            DbConnection = null
        };
        var sql = dut.BuildSql();
        Assert.Equal(expected, sql);
    }

    [Theory]
    [InlineData(false, 0, "*", "dbo", "Persons", "", "SELECT  * FROM dbo.Persons")]
    [InlineData(false, 0, "Id, Name", "dbo", "Persons", "", "SELECT  Id, Name FROM dbo.Persons")]
    [InlineData(false, 100, "Id, Name", "dbo", "Persons", "", "SELECT TOP 100  Id, Name FROM dbo.Persons")]
    [InlineData(false, 100, "Id, Name", "dbo", "Persons", "Name = 'Bob'", "SELECT TOP 100  Id, Name FROM dbo.Persons WHERE Name = 'Bob'")]
    [InlineData(false, 0, "Id, Name", "dbo", "Persons", "Name = 'Bob'", "SELECT  Id, Name FROM dbo.Persons WHERE Name = 'Bob'")]
    [InlineData(true, 0, "Name", "dbo", "Persons", "", "SELECT DISTINCT  Name FROM dbo.Persons")]
    [InlineData(true, 3, "Name", "dbo", "Persons", "", "SELECT DISTINCT TOP 3  Name FROM dbo.Persons")]
    [InlineData(false, 0, "Id, Name", "sales", "Persons", "", "SELECT  Id, Name FROM sales.Persons")]
    public void BuildSql_ReturnsExpectedSql(
        bool distinct,
        int top,
        string tableColumns,
        string defaultSchema,
        string tableName,
        string where,
        string expected)
    {
        var dut = new CruddyBase<Person>
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
    [InlineData("Id, Name", "Persons", "", "", SortOrder.None, "SELECT TOP 10000  Id, Name FROM dbo.Persons")]
    [InlineData("Id, Name", "Persons", "", "", SortOrder.Ascending, "SELECT TOP 10000  Id, Name FROM dbo.Persons")]
    [InlineData("Id, Name", "Persons", "", "", SortOrder.Descending, "SELECT TOP 10000  Id, Name FROM dbo.Persons")]
    [InlineData("Id, Name", "Persons", "Name = 'Bob'", "", SortOrder.None, "SELECT TOP 10000  Id, Name FROM dbo.Persons WHERE Name = 'Bob'")]
    [InlineData("Id, Name", "Persons", "Name = 'Bob'", "", SortOrder.Ascending, "SELECT TOP 10000  Id, Name FROM dbo.Persons WHERE Name = 'Bob'")]
    [InlineData("Id, Name", "Persons", "Name = 'Bob'", "", SortOrder.Descending, "SELECT TOP 10000  Id, Name FROM dbo.Persons WHERE Name = 'Bob'")]
    [InlineData("Id, Name", "Persons", "", "Name", SortOrder.None, "SELECT TOP 10000  Id, Name FROM dbo.Persons ORDER BY Name")]
    [InlineData("Id, Name", "Persons", "", "Name", SortOrder.Ascending, "SELECT TOP 10000  Id, Name FROM dbo.Persons ORDER BY Name ASC")]
    [InlineData("Id, Name", "Persons", "", "Name", SortOrder.Descending, "SELECT TOP 10000  Id, Name FROM dbo.Persons ORDER BY Name DESC")]
    [InlineData("Id, Name", "Persons", "Name = 'Bob'", "Name", SortOrder.None, "SELECT TOP 10000  Id, Name FROM dbo.Persons WHERE Name = 'Bob' ORDER BY Name")]
    [InlineData("Id, Name", "Persons", "Name = 'Bob'", "Name", SortOrder.Ascending, "SELECT TOP 10000  Id, Name FROM dbo.Persons WHERE Name = 'Bob' ORDER BY Name ASC")]
    [InlineData("Id, Name", "Persons", "Name = 'Bob'", "Name", SortOrder.Descending, "SELECT TOP 10000  Id, Name FROM dbo.Persons WHERE Name = 'Bob' ORDER BY Name DESC")]
    public void BuildSql_ReturnsExpectedSqlWithOrderBy(
        string tableColumns,
        string tableName,
        string where,
        string orderBy,
        SortOrder sortOrder,
        string expected)
    {
        var dut = new CruddyBase<Person>
        {
            TableColumns = tableColumns,
            TableName = tableName,
            Where = where,
            OrderBy = orderBy,
            SortOrder = sortOrder,
            DbConnection = null
        };
        var sql = dut.BuildSql();
        Assert.Equal(expected, sql);
    }

    #endregion

    #region ColumnAliasDict

    [Fact]
    public void FillColumnAliasDict_FillsDictionaryCorrectlyNoAS()
    {
        var dut = new CruddyBase<Person>
        {
            TableColumns = "Id, Name, Email",
            DbConnection = null
        };

        dut.FillColumnAliasDict();

        var expectedDict = new Dictionary<string, string>
        {
            { "Id", "Id" },
            { "Name", "Name" },
            { "Email", "Email" }
        };
        Assert.Equal(expectedDict, dut.ColumnAliasDict);
    }

    [Fact]
    public void FillColumnAliasDict_FillsDictionaryCorrectlyWithAS()
    {
        var dut = new CruddyBase<Person>
        {
            TableColumns = "Id AS PersonId, Name AS PersonName, Email",
            DbConnection = null
        };

        dut.FillColumnAliasDict();

        var expectedDict = new Dictionary<string, string>
        {
            { "Id", "PersonId" },
            { "Name", "PersonName" },
            { "Email", "Email" }
        };
        Assert.Equal(expectedDict, dut.ColumnAliasDict);
    }

    [Fact]
    public void FillColumnAliasDict_FillsDictionaryCorrectlyIgnoringDuplicates1()
    {
        var dut = new CruddyBase<Person>
        {
            TableColumns = "Id, Name, Name AS Name2",
            DbConnection = null
        };

        dut.FillColumnAliasDict();

        var expectedDict = new Dictionary<string, string>
        {
            { "Id", "Id" },
            { "Name", "Name" }
        };
        Assert.Equal(expectedDict, dut.ColumnAliasDict);
    }

    [Fact]
    public void FillColumnAliasDict_FillsDictionaryCorrectlyIgnoringDuplicates2()
    {
        var dut = new CruddyBase<Person>
        {
            TableColumns = "Id, Name AS Name1, Name",
            DbConnection = null
        };

        dut.FillColumnAliasDict();

        var expectedDict = new Dictionary<string, string>
        {
            { "Id", "Id" },
            { "Name", "Name1" }
        };
        Assert.Equal(expectedDict, dut.ColumnAliasDict);
    }

    [Fact]
    public void FillColumnAliasDict_FillsDictionaryCorrectlyRemovingBrackets()
    {
        var dut = new CruddyBase<Person>
        {
            TableColumns = "Id, Name AS [PersonName]",
            DbConnection = null
        };

        dut.FillColumnAliasDict();

        var expectedDict = new Dictionary<string, string>
        {
            { "Id", "Id" },
            { "Name", "PersonName" }
        };
        Assert.Equal(expectedDict, dut.ColumnAliasDict);
    }

    #endregion

    #region Map

    private class MapTarget
    {
#pragma warning disable S1144   // Unused private types or members should be removed
        public int Id { get; set; }
        public string? Name { get; set; }
        public DayOfWeek Day { get; set; }
        public decimal? Amount { get; set; }
#pragma warning restore S1144
    }

    [Fact]
    public void Map_Returns_Empty_List_When_Null()
    {
        IEnumerable<dynamic>? dyn = null;
        var list = CruddyBase<object>.Map<MapTarget>(dyn!);
        Assert.NotNull(list);
        Assert.Empty(list);
    }

    [Fact]
    public void Map_Maps_Anonymous_Object_To_StrongType()
    {
        var dynList = new List<dynamic>
        {
            new { Id = 7, Name = "Zoe", Day = 1, Amount = 12.34m }
        };

        var result = CruddyBase<object>.Map<MapTarget>(dynList);
        Assert.Single(result);
        var item = result[0];
        Assert.Equal(7, item.Id);
        Assert.Equal("Zoe", item.Name);
        // Day provided as numeric -> maps to enum
        Assert.Equal(DayOfWeek.Monday, item.Day);
        Assert.Equal(12.34m, item.Amount);
    }

    [Fact]
    public void Map_Is_CaseInsensitive_On_PropertyNames()
    {
        dynamic d = new System.Dynamic.ExpandoObject();
        var dict = (IDictionary<string, object?>)d;
        dict["id"] = 3;
        dict["name"] = "Case";

        var list = new List<dynamic> { d };
        var result = CruddyBase<object>.Map<MapTarget>(list);
        Assert.Single(result);
        Assert.Equal(3, result[0].Id);
        Assert.Equal("Case", result[0].Name);
    }

    [Fact]
    public void Map_Handles_Nullable_Values()
    {
        var dynList = new List<dynamic>
        {
            new { Id = 1, Name = (string?)null, Amount = (decimal?)null }
        };

        var result = CruddyBase<object>.Map<MapTarget>(dynList);
        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
        Assert.Null(result[0].Name);
        Assert.Null(result[0].Amount);
    }
    private static readonly int[] itemArray = [1, 2, 3];

    [Fact]
    public void Map_Maps_Array_Property()
    {
        var dynList = new List<dynamic>
        {
            new { Id = 5, Name = "Arr", Numbers = itemArray }
        };

        var result = CruddyBase<object>.Map<DynamicArrayTarget>(dynList);
        Assert.Single(result);
        var item = result[0];
        Assert.Equal(5, item.Id);
        Assert.Equal([1, 2, 3], item.Numbers!);
    }

    [Fact]
    public void Map_Maps_Enum_ByName()
    {
        var dynList = new List<dynamic>
        {
            new { Id = 8, Name = "EnumName", Day = "Friday" }
        };

        var result = CruddyBase<object>.Map<MapTarget>(dynList);
        Assert.Single(result);
        Assert.Equal(DayOfWeek.Friday, result[0].Day);
    }

    [Fact]
    public void Map_Throws_On_Invalid_Type_For_Property()
    {
        var dynList = new List<dynamic>
        {
            // Amount is expected to be a decimal; provide an unparsable string
            new { Id = 9, Name = "Bad", Amount = "not-a-decimal" }
        };

        Assert.Throws<System.Text.Json.JsonException>(() =>
        {
            _ = CruddyBase<object>.Map<MapTarget>(dynList);
        });
    }

    // Helper target for array mapping
    private class DynamicArrayTarget
    {
#pragma warning disable S1144   // Unused private types or members should be removed
        public int Id { get; set; }
        public int[]? Numbers { get; set; }
#pragma warning restore S1144
    }

    #endregion

    #region Constructor and defaults

    [Fact]
    public void Constructor_SetsPluralizedTableNameAndKeyColumn()
    {
        var dut = new CruddyBase<Person> { DbConnection = null };
        Assert.Equal("Persons", dut.TableName);
        Assert.Equal("Id", dut.KeyColumn);
    }

    [Fact]
    public void Constructor_SetsKeyColumnForClassWithoutKeyAttribute()
    {
        var dut = new CruddyBase<ClassWithoutKey> { DbConnection = null };
        Assert.Equal("ClassWithoutKeys", dut.TableName);
        Assert.Equal("Id", dut.KeyColumn);
    }

    [Fact]
    public void Defaults_AreExpected()
    {
        var dut = new CruddyBase<Person> { DbConnection = null };
        Assert.Equal("dbo", dut.DefaultSchema);
        Assert.True(dut.PluralizeTableName);
        Assert.Equal("*", dut.TableColumns);
        Assert.False(dut.AllowKeyColumnEditOnCreate);
        Assert.Equal(10000, dut.Top);
        Assert.False(dut.Distinct);
        Assert.Null(dut.OrderBy);
        Assert.Null(dut.Where);
        Assert.Null(dut.Select);
        Assert.Equal(SortOrder.Ascending, dut.SortOrder);
    }

    #endregion

    #region IncludeColumn

    [Theory]
    [InlineData(CrudOperation.Update, "Id", false, false)]
    [InlineData(CrudOperation.Update, "id", false, false)]
    [InlineData(CrudOperation.Update, "Id", true, false)]
    [InlineData(CrudOperation.Create, "Id", false, false)]
    [InlineData(CrudOperation.Create, "Id", true, true)]
    [InlineData(CrudOperation.Create, "ID", true, true)]
    [InlineData(CrudOperation.Create, "Name", false, true)]
    [InlineData(CrudOperation.Update, "Name", false, true)]
    public void IncludeColumn_ReturnsExpected(CrudOperation operation, string propName, bool allowKeyOnCreate, bool expected)
    {
        var dut = new CruddyBase<Person> { DbConnection = null, AllowKeyColumnEditOnCreate = allowKeyOnCreate };
        Assert.Equal(expected, dut.IncludeColumn(operation, propName));
    }

    #endregion

    #region GetPropertiesForTableColumns

    [Fact]
    public void GetPropertiesForTableColumns_NullEntity_Throws()
    {
        var dut = new CruddyBase<Person> { DbConnection = null };
        Assert.Throws<ArgumentNullException>(() => dut.GetPropertiesForTableColumns(null, CrudOperation.Update));
    }

    [Fact]
    public void GetPropertiesForTableColumns_Star_ReturnsAllPublicProperties()
    {
        var dut = new CruddyBase<Person> { DbConnection = null };
        var names = dut.GetPropertiesForTableColumns(new Person { Name = "A" }, CrudOperation.Update)
            .Select(p => p.Name).ToList();
        Assert.Contains("Id", names);
        Assert.Contains("Name", names);
        Assert.Contains("Description", names);
        Assert.Contains("Email", names);
        Assert.DoesNotContain("PrivateProperty", names);
        Assert.DoesNotContain("ProtectedProperty", names);
    }

    [Fact]
    public void GetPropertiesForTableColumns_UsesTableColumns()
    {
        var dut = new CruddyBase<Person> { DbConnection = null, TableColumns = "Id, Name" };
        var names = dut.GetPropertiesForTableColumns(new Person { Name = "A" }, CrudOperation.Update)
            .Select(p => p.Name).ToList();
        Assert.Equal(2, names.Count);
        Assert.Contains("Id", names);
        Assert.Contains("Name", names);
    }

    [Fact]
    public void GetPropertiesForTableColumns_ColumnsAltOverridesTableColumns()
    {
        var dut = new CruddyBase<Person> { DbConnection = null, TableColumns = "Id, Name" };
        var names = dut.GetPropertiesForTableColumns(new Person { Name = "A" }, CrudOperation.Update, "Email")
            .Select(p => p.Name).ToList();
        Assert.Equal(["Email"], names);
    }

    [Fact]
    public void GetPropertiesForTableColumns_Create_IgnoresTableColumns()
    {
        var dut = new CruddyBase<Person> { DbConnection = null, TableColumns = "Id" };
        var names = dut.GetPropertiesForTableColumns(new Person { Name = "A" }, CrudOperation.Create)
            .Select(p => p.Name).ToList();
        Assert.Contains("Name", names);
        Assert.Contains("Email", names);
    }

    [Fact]
    public void GetPropertiesForTableColumns_UnknownColumn_ReturnsEmpty()
    {
        var dut = new CruddyBase<Person> { DbConnection = null, TableColumns = "DoesNotExist" };
        Assert.Empty(dut.GetPropertiesForTableColumns(new Person { Name = "A" }, CrudOperation.Update));
    }

    #endregion

    #region GetPropertiesForCreateOrUpdate

    [Theory]
    [InlineData(CrudOperation.None)]
    [InlineData((CrudOperation)99)]
    public void GetPropertiesForCreateOrUpdate_InvalidOperation_Throws(CrudOperation operation)
    {
        var dut = new CruddyBase<Person> { DbConnection = null };
        var ex = Assert.Throws<ArgumentException>(() =>
            dut.GetPropertiesForCreateOrUpdate(new Person { Name = "A" }, operation));
        Assert.Equal("operation", ex.ParamName);
    }

    [Fact]
    public void GetPropertiesForCreateOrUpdate_Create_ExcludesKeyByDefault()
    {
        var dut = new CruddyBase<Person> { DbConnection = null };
        var names = dut.GetPropertiesForCreateOrUpdate(new Person { Name = "A" }, CrudOperation.Create)
            .Select(p => p.Name).ToList();
        Assert.DoesNotContain("Id", names);
        Assert.Contains("Name", names);
        Assert.Contains("Description", names);
        Assert.Contains("Email", names);
    }

    [Fact]
    public void GetPropertiesForCreateOrUpdate_Create_IncludesKeyWhenAllowed()
    {
        var dut = new CruddyBase<Person> { DbConnection = null, AllowKeyColumnEditOnCreate = true };
        var names = dut.GetPropertiesForCreateOrUpdate(new Person { Name = "A" }, CrudOperation.Create)
            .Select(p => p.Name).ToList();
        Assert.Contains("Id", names);
    }

    [Fact]
    public void GetPropertiesForCreateOrUpdate_Update_StarExcludesKey()
    {
        var dut = new CruddyBase<Person> { DbConnection = null };
        var names = dut.GetPropertiesForCreateOrUpdate(new Person { Name = "A" }, CrudOperation.Update)
            .Select(p => p.Name).ToList();
        Assert.DoesNotContain("Id", names);
        Assert.Contains("Name", names);
    }

    [Fact]
    public void GetPropertiesForCreateOrUpdate_Update_RespectsTableColumns()
    {
        var dut = new CruddyBase<Person> { DbConnection = null, TableColumns = "Id, Name" };
        var names = dut.GetPropertiesForCreateOrUpdate(new Person { Name = "A" }, CrudOperation.Update)
            .Select(p => p.Name).ToList();
        Assert.Equal(["Name"], names);
    }

    [Fact]
    public void GetPropertiesForCreateOrUpdate_Create_IgnoresTableColumns()
    {
        var dut = new CruddyBase<Person> { DbConnection = null, TableColumns = "Name" };
        var names = dut.GetPropertiesForCreateOrUpdate(new Person { Name = "A" }, CrudOperation.Create)
            .Select(p => p.Name).ToList();
        Assert.Contains("Email", names);
    }

    #endregion

    #region Update

    [Fact]
    public void Update_NullEntity_Throws()
    {
        var dut = new CruddyBase<Person> { DbConnection = new FakeDbConnection() };
        Assert.Throws<ArgumentNullException>(() => dut.Update(null, 1));
    }

    [Fact]
    public void Update_ExecutesUpdateSqlWithParameters()
    {
        var db = new FakeDbConnection { NonQueryResult = 1 };
        var dut = new CruddyBase<Person> { DbConnection = db };

        var result = dut.Update(new Person { Id = 5, Name = "Bob", Description = "D", Email = "b@x.dk" }, 5);

        Assert.Equal(1, result);
        var cmd = Assert.Single(db.Commands);
        Assert.Equal("NonQuery", cmd.Kind);
        Assert.StartsWith("UPDATE dbo.Persons SET ", cmd.Sql);
        Assert.EndsWith("WHERE Id = @keyValue", cmd.Sql);
        Assert.Contains("Name = @Name", cmd.Sql);
        Assert.Contains("Email = @Email", cmd.Sql);
        Assert.DoesNotContain("Id = @Id", cmd.Sql);
        Assert.Equal("Bob", cmd.Parameters["Name"]);
        Assert.Equal("b@x.dk", cmd.Parameters["Email"]);
        Assert.Equal(5, cmd.Parameters["keyValue"]);
    }

    [Fact]
    public void Update_UsesSchemaTableAndTableColumns()
    {
        var db = new FakeDbConnection();
        var dut = new CruddyBase<Person>
        {
            DbConnection = db,
            DefaultSchema = "sales",
            TableName = "People",
            TableColumns = "Id, Name"
        };

        dut.Update(new Person { Id = 2, Name = "Eve" }, 2);

        Assert.Equal("UPDATE sales.People SET Name = @Name WHERE Id = @keyValue", db.Commands[0].Sql);
    }

    [Fact]
    public void Update_NoMatchingProperties_ReturnsZeroAndExecutesNothing()
    {
        var db = new FakeDbConnection();
        var dut = new CruddyBase<Person> { DbConnection = db, TableColumns = "Id" };

        var result = dut.Update(new Person { Id = 2, Name = "Eve" }, 2);

        Assert.Equal(0, result);
        Assert.Empty(db.Commands);
    }

    #endregion

    #region Create

    [Fact]
    public void Create_NullEntity_Throws()
    {
        var dut = new CruddyBase<Person> { DbConnection = new FakeDbConnection() };
        Assert.Throws<ArgumentNullException>(() => dut.Create(null));
    }

    [Fact]
    public void Create_ReturnsScalarIdentity()
    {
        var db = new FakeDbConnection { ScalarResult = 77 };
        var dut = new CruddyBase<Person> { DbConnection = db };

        var result = dut.Create(new Person { Name = "Bob", Email = "b@x.dk" });

        Assert.Equal(77, result);
        var cmd = Assert.Single(db.Commands);
        Assert.Equal("Scalar", cmd.Kind);
        Assert.StartsWith("INSERT INTO dbo.Persons (", cmd.Sql);
        Assert.EndsWith("; SELECT SCOPE_IDENTITY();", cmd.Sql);
        Assert.DoesNotContain("@Id", cmd.Sql);
        Assert.Equal("Bob", cmd.Parameters["Name"]);
    }

    [Fact]
    public void Create_IncludesKeyWhenAllowed()
    {
        var db = new FakeDbConnection();
        var dut = new CruddyBase<Person> { DbConnection = db, AllowKeyColumnEditOnCreate = true };

        dut.Create(new Person { Id = 9, Name = "Bob" });

        Assert.Contains("@Id", db.Commands[0].Sql);
        Assert.Equal(9, db.Commands[0].Parameters["Id"]);
    }

    [Fact]
    public void Create_WhenScalarFails_FallsBackToRowsAffected()
    {
        var db = new FakeDbConnection { ThrowOnScalar = true, NonQueryResult = 1 };
        var dut = new CruddyBase<Person> { DbConnection = db };

        var result = dut.Create(new Person { Name = "Bob" });

        Assert.Equal(1, result);
        Assert.Equal(["Scalar", "NonQuery"], db.Commands.Select(c => c.Kind));
        Assert.DoesNotContain("SCOPE_IDENTITY", db.Commands[1].Sql);
    }

    #endregion

    #region Delete

    [Fact]
    public void Delete_ExecutesDeleteSql()
    {
        var db = new FakeDbConnection { ScalarResult = null };
        var dut = new CruddyBase<Person> { DbConnection = db };

        var result = dut.Delete(3);

        Assert.Null(result);
        var cmd = Assert.Single(db.Commands);
        Assert.Equal("DELETE FROM dbo.Persons WHERE Id = @keyValue", cmd.Sql);
        Assert.Equal(3, cmd.Parameters["keyValue"]);
    }

    #endregion

    #region GetTableRow

    [Fact]
    public void GetTableRow_ReturnsMappedEntity()
    {
        var db = new FakeDbConnection();
        db.Rows.Add(new Dictionary<string, object?> { ["Id"] = 4, ["Name"] = "Ann", ["Description"] = null, ["Email"] = "a@x.dk" });
        var dut = new CruddyBase<Person> { DbConnection = db };

        var row = dut.GetTableRow(4);

        Assert.NotNull(row);
        Assert.Equal(4, row.Id);
        Assert.Equal("Ann", row.Name);
        Assert.Null(row.Description);
        Assert.Equal("a@x.dk", row.Email);
        var cmd = Assert.Single(db.Commands);
        Assert.Equal("SELECT * FROM dbo.Persons WHERE Id = @Id", cmd.Sql);
        Assert.Equal(4, cmd.Parameters["Id"]);
    }

    [Fact]
    public void GetTableRow_UsesColumnsParameter()
    {
        var db = new FakeDbConnection();
        var dut = new CruddyBase<Person> { DbConnection = db };

        dut.GetTableRow(1, "Id, Name");

        Assert.Equal("SELECT Id, Name FROM dbo.Persons WHERE Id = @Id", db.Commands[0].Sql);
    }

    [Fact]
    public void GetTableRow_NotFound_ReturnsNull()
    {
        var dut = new CruddyBase<Person> { DbConnection = new FakeDbConnection() };
        Assert.Null(dut.GetTableRow(123));
    }

    #endregion

    #region OnInitializedAsync

    private sealed class TestableCruddy<T> : CruddyBase<T> where T : class
    {
        public Task InitializeAsync() => OnInitializedAsync();
        public List<T>? LoadedRows => Rows;
    }

    [Fact]
    public async Task OnInitializedAsync_LoadsRowsAndFillsAliasDict()
    {
        var db = new FakeDbConnection();
        db.Rows.Add(new Dictionary<string, object?> { ["Id"] = 1, ["Name"] = "A" });
        db.Rows.Add(new Dictionary<string, object?> { ["Id"] = 2, ["Name"] = "B" });
        var dut = new TestableCruddy<Person> { DbConnection = db, TableColumns = "Id, Name", Top = 5 };

        await dut.InitializeAsync();

        Assert.NotNull(dut.LoadedRows);
        Assert.Equal(["A", "B"], dut.LoadedRows.Select(r => r.Name));
        Assert.Equal(2, dut.ColumnAliasDict.Count);
        var cmd = Assert.Single(db.Commands);
        Assert.Equal("SELECT TOP 5  Id, Name FROM dbo.Persons", cmd.Sql);
    }

    [Fact]
    public async Task OnInitializedAsync_NoRows_LoadsEmptyList()
    {
        var dut = new TestableCruddy<Person> { DbConnection = new FakeDbConnection() };

        await dut.InitializeAsync();

        Assert.NotNull(dut.LoadedRows);
        Assert.Empty(dut.LoadedRows);
    }

    #endregion
}
#pragma warning restore BL0005
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
