using System.ComponentModel.DataAnnotations;

namespace Cruddy.Components;

/// <summary>
/// Represents information about an input field, including its display name, value, type, and other 
/// properties. This class is used to generate input fields in a UI based on the properties of a model.
/// </summary>
public class InputInfo
{
    /// <summary>
    /// Display name for a property, using the DisplayNameAttribute or DisplayAttribute. If neither is present, the property name is used.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Display format for a property, using the DisplayFormatAttribute if present.
    /// </summary>
    public required string? DisplayFormat { get; init; }

    /// <summary>
    /// The value of the property, which can be of any type. This is used to populate the input field in the UI.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// The formatted value of the property, which can be used to display the value in a specific format in the UI.
    /// </summary>
    public string? FormattedValue { get; set; }

    /// <summary>
    /// The type of the property. This is used to determine the appropriate input field type in the UI.
    /// </summary>
    public required Type Type { get; init; }

    /// <summary>
    /// Whether the input field is disabled. This is used to determine if the input field should be interactive in the UI.
    /// </summary>
    public required bool Disabled { get; init; }

    /// <summary>
    /// Whether the input field is required. This is used to determine if the input field must have a value in the UI.
    /// </summary>
    public required bool Required { get; init; }

    /// <summary>
    /// Whether the key column should be hidden. This is used to determine if the key column should be visible in the UI.
    /// </summary>
    public required bool HideKeyColumn { get; init; }

    /// <summary>
    /// Whether the property is a key column.
    /// </summary>
    public required bool IsKeyColumn { get; init; }

    /// <summary>
    /// The data type of the property. This is used to determine the appropriate input field type in the UI, especially for date and time inputs.
    /// </summary>
    public DataType? DataType { get; init; }

    /// <summary>
    /// Whether the input field is visible. This is used to determine if the input field should be rendered in the UI.
    /// </summary>
    public bool Visible
    {
        get
        {
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
