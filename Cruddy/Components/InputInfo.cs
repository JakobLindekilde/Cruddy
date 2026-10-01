using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace Cruddy.Components
{
    public class InputInfo
    {
        public required string DisplayName { get; init; }
        public required string? DisplayFormat { get; init; }
        public object? Value { get; set; }
        public string? FormattedValue { get; set; }
        public required Type Type { get; init; }
        public required bool Disabled { get; init; }
        public required bool Required { get; init; }
        public required bool HideKeyColumn { get; init; }
        public required bool IsKeyColumn { get; init; }
        public DataType? DataType { get; init; }

        public bool Visible 
        { 
            get { 
                if (IsKeyColumn)
                {
                    return !HideKeyColumn;
                }
                return true;
            }
        }

        /// <summary>
        /// Returns the appropriate HTML input type="data" and input type="datetime-local" format (i.e. not in localized format).
        /// </summary>
        /// <returns></returns>
        public string InputDateTimeValue()
        {
            if (Value is DateTime dt)
            {
                if (DataType == System.ComponentModel.DataAnnotations.DataType.Date)
                {
                    return dt.ToString("yyyy-MM-dd");
                }
                else if (DataType == System.ComponentModel.DataAnnotations.DataType.Time)
                {
                    return dt.ToString("HH:mm:ss");
                }
                return dt.ToString("yyyy-MM-ddTHH:mm:ss");
            }
            return string.Empty;
        }
    }
}
