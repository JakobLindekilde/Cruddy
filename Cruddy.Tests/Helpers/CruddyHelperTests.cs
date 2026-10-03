using Cruddy.Helpers;

namespace Cruddy.Tests.Helpers;

public class CruddyHelperTests
{
    [Fact]
    public void GetAddSimple_ReturnsMethod()
    {
        var method = CruddyHelper.GetAddSimpleMethod<object>();

        Assert.NotNull(method);
        Assert.Equal("AddSimple", method.Name);
    }

}
