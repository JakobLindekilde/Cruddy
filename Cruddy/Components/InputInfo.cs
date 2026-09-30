namespace Cruddy.Components
{
    public class InputInfo
    {
        public required string DisplayName { get; set; }
        public object? Value { get; set; }
        public required Type Type { get; set; }
        public bool Disabled { get; set; }
        public bool Required { get; set; }
    }
}
