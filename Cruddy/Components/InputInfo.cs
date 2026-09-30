using System.Reflection;

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
    }
}
