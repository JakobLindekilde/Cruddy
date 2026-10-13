using Cruddy.Components;
using Xunit;

namespace CruddyDemo.Tests;

public class CruddyBaseFillFKeyInfoTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void FillFKeyInfo_NullOrEmpty_ReturnsNull(string? fKey)
    {
        Assert.Null(CruddyBase<object>.FillFKeyInfo(fKey));
    }

    [Fact]
    public void FillFKeyInfo_NoColon_ReturnsNull()
    {
        Assert.Null(CruddyBase<object>.FillFKeyInfo("CategoryId"));
    }

    [Theory]
    [InlineData("CategoryId: Categories, Id")]
    [InlineData("CategoryId: Categories, Id, Name, Extra")]
    [InlineData("CategoryId:")]
    public void FillFKeyInfo_WrongNumberOfValues_ReturnsNull(string fKey)
    {
        Assert.Null(CruddyBase<object>.FillFKeyInfo(fKey));
    }

    [Fact]
    public void FillFKeyInfo_ValidInput_ReturnsPopulatedInfo()
    {
        var result = CruddyBase<object>.FillFKeyInfo("CategoryId: Categories, Id, Name");

        Assert.NotNull(result);
        Assert.Equal("CategoryId", result.FKey);
        Assert.Equal("Categories", result.PrimaryTable);
        Assert.Equal("Id", result.PrimaryId);
        Assert.Equal("Name", result.PrimaryDisplay);
    }

    [Fact]
    public void FillFKeyInfo_ExtraWhitespace_TrimsValues()
    {
        var result = CruddyBase<object>.FillFKeyInfo("  CategoryId  :  Categories ,  Id  ,  Name  ");

        Assert.NotNull(result);
        Assert.Equal("CategoryId", result.FKey);
        Assert.Equal("Categories", result.PrimaryTable);
        Assert.Equal("Id", result.PrimaryId);
        Assert.Equal("Name", result.PrimaryDisplay);
    }

    [Fact]
    public void FillFKeyInfo_NoSpaces_ParsesCorrectly()
    {
        var result = CruddyBase<object>.FillFKeyInfo("CategoryId:Categories,Id,Name");

        Assert.NotNull(result);
        Assert.Equal("Categories", result.PrimaryTable);
    }
}