using Cruddy.Components;
using Xunit;

namespace CruddyDemo.Tests;

public class CruddyBaseFillFKeyInfoTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void FillFKeyInfo_NullOrEmpty_ReturnsEmptyList(string? fKey)
    {
        var result = CruddyBase<object>.FillFKeyInfoList(fKey);
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void FillFKeyInfo_NoColon_ReturnsEmptyList()
    {
        var result = CruddyBase<object>.FillFKeyInfoList("CategoryId");
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Theory]
    [InlineData("CategoryId: Categories, Id")]
    [InlineData("CategoryId: Categories, Id, Name, Extra")]
    [InlineData("CategoryId:")]
    public void FillFKeyInfo_WrongNumberOfValues_ReturnsEmptyList(string fKey)
    {
        var result = CruddyBase<object>.FillFKeyInfoList(fKey);
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void FillFKeyInfo_ValidInput_ReturnsPopulatedInfo()
    {
        var result = CruddyBase<object>.FillFKeyInfoList("CategoryId1: Categories1, Id1, Name1; CategoryId2: Categories2, Id2, Name2");

        Assert.NotNull(result);
        
        Assert.Equal("CategoryId1", result[0].FKey);
        Assert.Equal("Categories1", result[0].PrimaryTable);
        Assert.Equal("Id1", result[0].PrimaryId);
        Assert.Equal("Name1", result[0].PrimaryDisplay);

        Assert.Equal("CategoryId2", result[1].FKey);
        Assert.Equal("Categories2", result[1].PrimaryTable);
        Assert.Equal("Id2", result[1].PrimaryId);
        Assert.Equal("Name2", result[1].PrimaryDisplay);
    }

    [Fact]
    public void FillFKeyInfo_ExtraWhitespace_TrimsValues()
    {
        var result = CruddyBase<object>.FillFKeyInfoList("  CategoryId  :  Categories ,  Id  ,  Name  ");

        Assert.NotNull(result);
        Assert.Equal("CategoryId", result[0].FKey);
        Assert.Equal("Categories", result[0].PrimaryTable);
        Assert.Equal("Id", result[0].PrimaryId);
        Assert.Equal("Name", result[0].PrimaryDisplay);
    }

    [Fact]
    public void FillFKeyInfo_NoSpaces_ParsesCorrectly()
    {
        var result = CruddyBase<object>.FillFKeyInfoList("CategoryId:Categories,Id,Name");

        Assert.NotNull(result);
        Assert.Equal("Categories", result[0].PrimaryTable);
    }
}