using Cruddy.Components;
using Cruddy.Tests.Models;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

#pragma warning disable BL0005 // Component parameter should not be set outside of its component.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

namespace Cruddy.Tests.Components
{
    public class GetInputInfoTests
    {
        [Fact]
        public void GetInputInfo_ReturnsMetadataForAnnotatedProperty()
        {
            var dut = new Cruddy<InputInfoType>
            {
                DbConnection = null,
                DefaultDateFormat = "date-default",
                DefaultDateTimeFormat = "datetime-default",
                DefaultTimeFormat = "time-default",
                DefaultDecimalFormat = "decimal-default",
                DefaultNumberFormat = "number-default"
            };

            var info = dut.GetInputInfo(Property(nameof(InputInfoType.Name)), null, CrudOperation.Read);

            Assert.Equal("Component Display Name", info.DisplayName);
            Assert.Null(info.DisplayFormat);
            Assert.Equal(typeof(string), info.Type);
            Assert.Null(info.Value);
            Assert.Null(info.FormattedValue);
            Assert.False(info.Disabled);
            Assert.True(info.Required);
            Assert.False(info.HideKeyColumn);
            Assert.False(info.IsKeyColumn);
            Assert.Null(info.DataType);
            Assert.True(info.Visible);
        }

        [Fact]
        public void GetInputInfo_PrefersDisplayNameAttributeOverDisplayAttribute()
        {
            var dut = new Cruddy<InputInfoType> { DbConnection = null };

            var info = dut.GetInputInfo(Property(nameof(InputInfoType.Name)), null, CrudOperation.Read);

            Assert.Equal("Component Display Name", info.DisplayName);
        }

        [Fact]
        public void GetInputInfo_UsesUnderlyingTypeAndNullableRequiredFlag()
        {
            var dut = new Cruddy<InputInfoType> { DbConnection = null };

            var nullableNumber = dut.GetInputInfo(Property(nameof(InputInfoType.NullableNumber)), null, CrudOperation.Read);
            var nullableName = dut.GetInputInfo(Property(nameof(InputInfoType.NullableName)), null, CrudOperation.Read);
            var number = dut.GetInputInfo(Property(nameof(InputInfoType.Number)), null, CrudOperation.Read);

            Assert.Equal(typeof(int), nullableNumber.Type);
            Assert.False(nullableNumber.Required);
            Assert.Equal(typeof(string), nullableName.Type);
            Assert.False(nullableName.Required);
            Assert.Equal(typeof(int), number.Type);
            Assert.True(number.Required);
        }

        [Theory]
        [InlineData(nameof(InputInfoType.Date), "datetime-default", "Date")]
        [InlineData(nameof(InputInfoType.Time), "datetime-default", "Time")]
        [InlineData(nameof(InputInfoType.DateOnly), "date-only-default", "DateOnly")]
        [InlineData(nameof(InputInfoType.TimeOnly), "time-only-default", "TimeOnly")]
        [InlineData(nameof(InputInfoType.Decimal), "decimal-default", "Decimal")]
        [InlineData(nameof(InputInfoType.Number), "number-default", "Number")]
        public void GetInputInfo_UsesDefaultDisplayFormatForPropertyType(string propertyName, string expectedFormat, string _)
        {
            var dut = new Cruddy<InputInfoType>
            {
                DbConnection = null,
                DefaultDateFormat = "date-only-default",
                DefaultDateTimeFormat = "datetime-default",
                DefaultTimeFormat = "time-only-default",
                DefaultDecimalFormat = "decimal-default",
                DefaultNumberFormat = "number-default"
            };

            var info = dut.GetInputInfo(Property(propertyName), null, CrudOperation.Read);

            Assert.Equal(expectedFormat, info.DisplayFormat);
        }

        [Fact]
        public void GetInputInfo_UsesDisplayFormatAttributeOverDefault()
        {
            var dut = new Cruddy<InputInfoType>
            {
                DbConnection = null,
                DefaultDateTimeFormat = "datetime-default"
            };

            var info = dut.GetInputInfo(Property(nameof(InputInfoType.FormattedDate)), null, CrudOperation.Read);

            Assert.Equal("yyyy-MM-dd", info.DisplayFormat);
        }

        [Theory]
        [InlineData(nameof(InputInfoType.Date), DataType.Date)]
        [InlineData(nameof(InputInfoType.Time), DataType.Time)]
        public void GetInputInfo_ReturnsDataTypeAttribute(string propertyName, DataType expectedDataType)
        {
            var dut = new Cruddy<InputInfoType> { DbConnection = null };

            var info = dut.GetInputInfo(Property(propertyName), null, CrudOperation.Read);

            Assert.Equal(expectedDataType, info.DataType);
        }

        [Fact]
        public void GetInputInfo_ReturnsItemValueAndFormattedValue()
        {
            var value = new InputInfoType
            {
                Id = 42,
                Date = new DateTime(2025, 6, 7, 8, 9, 10, DateTimeKind.Unspecified),
                Name = "Alice",
                FormattedDate = new DateTime(2025, 6, 7, 0, 0, 0, DateTimeKind.Unspecified)
            };
            var dut = new Cruddy<InputInfoType>
            {
                DbConnection = null,
                DefaultDateTimeFormat = "yyyy-MM-dd"
            };

            var nameInfo = dut.GetInputInfo(Property(nameof(InputInfoType.Name)), value, CrudOperation.Read);
            var dateInfo = dut.GetInputInfo(Property(nameof(InputInfoType.Date)), value, CrudOperation.Read);
            var formattedDateInfo = dut.GetInputInfo(Property(nameof(InputInfoType.FormattedDate)), value, CrudOperation.Read);

            Assert.Equal("Alice", nameInfo.Value);
            Assert.Equal("Alice", nameInfo.FormattedValue);
            Assert.Equal(value.Date, dateInfo.Value);
            Assert.Equal(value.Date.ToString(), dateInfo.FormattedValue);
            Assert.Equal(value.FormattedDate, formattedDateInfo.Value);
            Assert.Equal("2025-06-07", formattedDateInfo.FormattedValue);
        }

        [Fact]
        public void GetInputInfo_ReturnsNullAndEmptyFormattedValueForNullPropertyValue()
        {
            var value = new InputInfoType { Name = "Alice" };
            var dut = new Cruddy<InputInfoType> { DbConnection = null };

            var info = dut.GetInputInfo(Property(nameof(InputInfoType.NullableName)), value, CrudOperation.Read);

            Assert.Null(info.Value);
            Assert.Equal(string.Empty, info.FormattedValue);
        }

        [Fact]
        public void GetInputInfo_LeavesValuesUnsetWhenItemIsNull()
        {
            var dut = new Cruddy<InputInfoType> { DbConnection = null };

            var info = dut.GetInputInfo(Property(nameof(InputInfoType.Number)), null, CrudOperation.Create);

            Assert.Null(info.Value);
            Assert.Null(info.FormattedValue);
        }

        [Theory]
        [InlineData(CrudOperation.Create, false, true)]
        [InlineData(CrudOperation.Create, true, false)]
        [InlineData(CrudOperation.Update, false, true)]
        [InlineData(CrudOperation.Read, false, true)]
        public void GetInputInfo_DisablesKeyPropertyAccordingToOperation(
            CrudOperation operation,
            bool allowKeyColumnEditOnCreate,
            bool expectedDisabled)
        {
            var dut = new Cruddy<InputInfoType>
            {
                DbConnection = null,
                AllowKeyColumnEditOnCreate = allowKeyColumnEditOnCreate
            };

            var info = dut.GetInputInfo(Property(nameof(InputInfoType.Id)), null, operation);

            Assert.True(info.IsKeyColumn);
            Assert.Equal(expectedDisabled, info.Disabled);
        }

        [Fact]
        public void GetInputInfo_RecognizesKeyColumnCaseInsensitively()
        {
            var dut = new Cruddy<InputInfoType> { DbConnection = null, KeyColumn = "id" };

            var info = dut.GetInputInfo(Property(nameof(InputInfoType.Id)), null, CrudOperation.Read);

            Assert.True(info.IsKeyColumn);
            Assert.True(info.Disabled);
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void GetInputInfo_UsesHideKeyColumnToDetermineKeyVisibility(bool hideKeyColumn, bool expectedVisible)
        {
            var dut = new Cruddy<InputInfoType> { DbConnection = null, HideKeyColumn = hideKeyColumn };

            var info = dut.GetInputInfo(Property(nameof(InputInfoType.Id)), null, CrudOperation.Read);

            Assert.Equal(hideKeyColumn, info.HideKeyColumn);
            Assert.Equal(expectedVisible, info.Visible);
        }

        [Fact]
        public void GetInputInfo_NonKeyPropertyRemainsVisibleWhenKeyColumnIsHidden()
        {
            var dut = new Cruddy<InputInfoType> { DbConnection = null, HideKeyColumn = true };

            var info = dut.GetInputInfo(Property(nameof(InputInfoType.Name)), null, CrudOperation.Read);

            Assert.False(info.IsKeyColumn);
            Assert.True(info.Visible);
            Assert.False(info.Disabled);
        }

        private static PropertyInfo Property(string name) =>
            typeof(InputInfoType).GetProperty(name)!;
    }
}
