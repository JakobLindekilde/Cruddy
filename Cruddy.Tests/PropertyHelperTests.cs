using System.ComponentModel.DataAnnotations;
using CruddyDemo.Helpers;

namespace Cruddy.Tests
{
    public class PropertyHelperTests
    {
        public class ClassWithKey
        {
            public ClassWithKey() { }

            [Key]
            public int Id { get; set; }

            public required string Name { get; set; }
        }

        public class ClassWithoutKey
        {
            public ClassWithoutKey() { }

            public int Id { get; set; }

            public required string Firstname { get; set; }

            public string? Lastname { get; set; }

            public int Age { get; set; }

            public int? Hight { get; set; }
        }

        [Fact]
        public void GetKey()
        {
            var hasKeyWithKey = PropertyHelper.GetKey(typeof(ClassWithKey));
            var hasKeyWithoutKey = PropertyHelper.GetKey(typeof(ClassWithoutKey));

            // Assert
            Assert.NotNull(hasKeyWithKey);
            Assert.Null(hasKeyWithoutKey);
        }

        [Fact]
        public void PropExists()
        {
            var hasIdProperty = PropertyHelper.PropExists(typeof(ClassWithoutKey), "Id");
            var hasFirstnameProperty = PropertyHelper.PropExists(typeof(ClassWithoutKey), "Firstname");
            var hasAgeProperty = PropertyHelper.PropExists(typeof(ClassWithoutKey), "Age");
            var hasNonExistentProperty = PropertyHelper.PropExists(typeof(ClassWithoutKey), "NonExistent");
            
            // Assert
            Assert.True(hasIdProperty);
            Assert.True(hasFirstnameProperty);
            Assert.True(hasAgeProperty);
            Assert.False(hasNonExistentProperty);       
        }
    }
}
