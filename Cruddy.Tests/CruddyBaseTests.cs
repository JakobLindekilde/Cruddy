using CruddyDemo.Components;
using CruddyDemo.Helpers;

namespace Cruddy.Tests
{
    public class CruddyBaseTests
    {
        [Theory]
        [InlineData('a', true)]
        [InlineData('e', true)]
        [InlineData('i', true)]
        [InlineData('o', true)]
        [InlineData('u', true)]
        [InlineData('A', true)]
        [InlineData('E', true)]
        [InlineData('I', true)]
        [InlineData('O', true)]
        [InlineData('U', true)]
        [InlineData('b', false)]
        [InlineData('y', false)]   // 'y' is sometimes a vowel. It's handled in Pluralize()
        [InlineData('z', false)]
        [InlineData('1', false)]
        [InlineData('?', false)]
        [InlineData('_', false)]
        [InlineData(' ', false)]
        [InlineData('-', false)]
        [InlineData('æ', false)]   // Danish/Norwegian vowels
        [InlineData('ø', false)]
        [InlineData('å', false)]
        [InlineData('Æ', false)]
        [InlineData('Ø', false)]
        [InlineData('Å', false)]
        public void IsVowel_Works_ForVariousChars(char c, bool expected)
        {
            var result = CruddyBase<object>.IsVowel(c);
            Assert.Equal(expected, result);
        }

        [Theory]
        // y preceded by consonant -> ies
        [InlineData("Category", "Categories")]
        [InlineData("city", "cities")]
        [InlineData("baby", "babies")]
        [InlineData("by", "bies")]

        // y preceded by vowel -> just add s
        [InlineData("key", "keys")]
        [InlineData("day", "days")]

        // endings that add es
        [InlineData("bus", "buses")]
        [InlineData("box", "boxes")]
        [InlineData("buzz", "buzzes")]
        [InlineData("church", "churches")]
        [InlineData("dish", "dishes")]

        // regular plurals
        [InlineData("cat", "cats")]
        [InlineData("dog", "dogs")]

        // case preservation tests
        [InlineData("Bus", "Buses")]
        [InlineData("BUS", "BUSES")]
        [InlineData("CHurCh", "CHurChes")]
        public void Pluralize_Returns_Expected(string singular, string expectedPlural)
        {
            var plural = CruddyBase<object>.Pluralize(singular);
            Assert.Equal(expectedPlural, plural);
        }

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
            var dut = new CruddyBase<Person> {
                Distinct = distinct,
                Top = top,
                TableColumns = tableColumns,
                DefaultSchema = defaultSchema,
                TableName = tableName,
                Where = where,  
                DbConnection = null };
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

    }
}
