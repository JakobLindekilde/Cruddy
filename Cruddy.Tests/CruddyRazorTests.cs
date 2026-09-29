using Xunit;
using Cruddy.Components;

namespace Cruddy.Tests
{
	public class CruddyRazorTests
	{
        [Fact]
		public void GetAddSimple_ReturnsMethod()
		{
			var method = Cruddy<object>.GetAddSimpleMethod();

			Assert.NotNull(method);
			Assert.Equal("AddSimple", method.Name);
		}	

	}
}
