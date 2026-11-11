using Lindekilde.Blazor.Cruddy.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Lindekilde.Blazor.Cruddy.Components;

/// <summary>
/// Lists the rows of a database table in a QuickGrid component, with columns 
/// generated from the table columns specified in <see cref="CruddyBase.TableColumns"/>.
/// No model is needed.
/// </summary>
public partial class Cruddy : CruddyBase
{
    /// <summary>
    /// Gets or sets the application configuration.
    /// </summary>
    /// <remarks>Provided by dependency injection.</remarks>
    [Inject]
    protected IConfiguration Configuration { get; set; } = default!;

    #region Parameters

    /// <summary>
    /// The default date format to use when displaying date values. Default format is d (aka 'Short date pattern').
    /// </summary>
    [Parameter]
    public string? DefaultDateFormat { get; set; } = "d";

    /// <summary>
    /// The default date format to use when displaying datetime values.
    /// Default format is g (aka 'general date/time pattern (short time)').
    /// </summary>
    [Parameter]
    public string? DefaultDateTimeFormat { get; set; } = "g";

    /// <summary>
    /// The default date format to use when displaying time values. Default format is t (aka 'short time pattern').
    /// </summary>
    [Parameter]
    public string? DefaultTimeFormat { get; set; } = "t";

    /// <summary>
    /// The default decimal, double and float format to use when displaying decimal values. Default format is 0.00.
    /// </summary>
    [Parameter]
    public string? DefaultDecimalFormat { get; set; } = "0.00";

    /// <summary>
    /// The default format to use when displaying numbers. Default format is 0.
    /// </summary>
    [Parameter]
    public string? DefaultNumberFormat { get; set; } = "0";

    /// <summary>
    /// The title to use for the actions column.
    /// </summary>
    [Parameter]
    public string? ActionsTitle { get; set; } = "Actions";

    /// <summary>
    /// Same as setting AllowCreate, AllowDetails, AllowEdit and AllowDelete to true.
    /// </summary>
    [Parameter]
    public bool AllowCrud { get; set; } = false;

    /// <summary>
    /// Whether to enable the create functionality.
    /// </summary>
    [Parameter]
    public bool AllowCreate { get; set; } = false;

    /// <summary>
    /// Whether to enable the details functionality for each row.
    /// </summary>
    [Parameter]
    public bool AllowDetails { get; set; } = false;

    /// <summary>
    /// List of table columns, separated by commas, to display in the Details modal. 
    /// If "*" is specified, all table columns will be displayed.
    /// If empty string TableColumns is used.
    /// </summary>
    [Parameter]
    public string DetailsColumns { get; set; } = "*";

    /// <summary>
    /// Whether to enable the edit functionality for each row.
    /// </summary>
    [Parameter]
    public bool AllowEdit { get; set; } = false;

    /// <summary>
    /// Whether to enable the delete functionality for each row.
    /// </summary>
    [Parameter]
    public bool AllowDelete { get; set; } = false;

    /// <summary>
    /// Whether to update the whole page after a change (create, edit, delete) is made, 
    /// or just the component. If true, the page will be refreshed after a change is made. 
    /// If false, the in-memory list of rows will be updated and the UI will be refreshed 
    /// without reloading the page.
    /// </summary>
    [Parameter]
    public bool ReloadPageAfterChange { get; set; } = false;

    /// <summary>
    /// The name of the column/field, used for display purposes in the UX. 
    /// Used in messages like "Are you sure you want to delete this {NameUx}?".
    /// </summary>
    [Parameter]
    public string? NameUx { get; set; }

    /// <summary>
    /// Whether to hide the key column in the UX.
    /// </summary>
    [Parameter]
    public bool HideKeyColumn { get; set; } = false;

    /// <summary>
    /// Theme applied to the QuickGrid. Default is "twentyAI".
    /// </summary>
    [Parameter]
    public string CruddyGridTheme { get; set; } = "twentyAI";

    /// <summary>
    /// CSS class applied to the QuickGrid.
    /// </summary>
    [Parameter]
    public string CruddyGridClass { get; set; } = "table table-sm table-index table-striped small table-fit table-thead-sticky mb-0";

    /// <summary>
    /// CSS class applied to the modal dialog table.
    /// </summary>
    [Parameter]
    public string ModalTableClass { get; set; } = "table table-sm table-striped";

    #endregion

    #region Properties

    /// <summary>
    /// The QuickGrid component that displays the rows retrieved from the database.
    /// </summary>
    protected QuickGrid<CruddyRow>? MyGrid;

    /// <summary>
    /// The ColumnManager that manages the columns of the QuickGrid component.
    /// </summary>
    protected readonly List<RenderFragment> MyColumns = [];

    /// <summary>
    /// Holds a user visible error message when e.g. delete fails.
    /// </summary>
    protected string? ErrorMessage { get; set; }

    /// <summary>
    /// Holds a user visible error message when e.g. Edit or Create fails.
    /// </summary>
    protected string? EditErrorMessage { get; set; }

    /// <summary>
    /// Navigation manager used to reload the page when ReloadPageAfterChange is true.
    /// </summary>
    [Inject]
    protected NavigationManager? NavigationManager { get; set; }

    /// <summary>
    /// The currently selected row being crud'ed (a copy).
    /// </summary>
    protected CruddyRow? ActionItem { get; set; }

    /// <summary>
    /// The currently active crud operation.
    /// </summary>
    protected CrudOperation ActionCrud { get; set; } = CrudOperation.None;

    #endregion

    #region Lifecycle methods

    /// <summary>
    /// Parameters not supplied in the Cruddy razor tag are read from the "Cruddy" section in appsettings.json. 
    /// If a parameter is in neither, the default value is used.
    /// </summary>
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Configuration.GetSection("Cruddy").Bind(this);
        return base.SetParametersAsync(parameters);
    }

    /// <summary>
    /// During component initialization, this method adds columns to the grid.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        AddColumnsToGrid();
    }

    #endregion

    #region Details

    /// <summary>
    /// Whether the details modal is visible.
    /// </summary>
    protected bool ShowDetailsModal { get; set; }

    /// <summary>
    /// The fields shown in the details modal.
    /// </summary>
    protected List<CruddyField> DetailsFields =>
        string.IsNullOrEmpty(DetailsColumns) ? Fields : GetFields(DetailsColumns);

    private void ShowDetails(CruddyRow item)
    {
        LoadFKeyOptions();

        if (string.IsNullOrEmpty(DetailsColumns))
        {
            ActionItem = item;
        }
        else
        {
            var keyValue = GetKeyValue(item);
            ActionItem = keyValue is null ? item : GetTableRow(keyValue, DetailsColumns) ?? item;
        }

        ActionCrud = CrudOperation.Read;
    }

    #endregion

    #region Edit

    /// <summary>
    /// EditContext used for validation.
    /// </summary>
    protected EditContext? EditCtx { get; set; }

    /// <summary>
    /// The fields shown in the create and edit modal.
    /// </summary>
    protected List<CruddyField> EditFields => Fields.Where(f => !f.IsReadOnly).ToList();

    private Task ShowEdit(CrudOperation actionCrud, CruddyRow? item = null)
    {
        LoadFKeyOptions();

        ActionItem = actionCrud == CrudOperation.Create
            ? NewRow()
            : new CruddyRow(item!, StringComparer.OrdinalIgnoreCase);

        EditCtx = new EditContext(ActionItem);
        ActionCrud = actionCrud;
        return Task.CompletedTask;
    }

    private CruddyRow NewRow()
    {
        var row = new CruddyRow(StringComparer.OrdinalIgnoreCase);
        foreach (var field in Fields)
        {
            row[field.Name] = field.IsNullable || field.Type == typeof(string)
                ? null
                : Activator.CreateInstance(field.Type);
        }
        return row;
    }

    private void CancelAction()
    {
        ActionCrud = CrudOperation.None;
        ActionItem = null;
        EditCtx = null;
    }

    private void PersistCreated(CruddyRow item)
    {
        var result = Create(item);

        // If a scalar id was returned, attempt to set the key field
        var keyField = Fields.FirstOrDefault(f => string.Equals(f.Column, KeyColumn, StringComparison.OrdinalIgnoreCase));
        if (keyField != null && result != null && result is not DBNull)
        {
            try
            {
                item[keyField.Name] = Convert.ChangeType(result, keyField.Type);
            }
            catch
            {
                // ignore conversion errors
            }
        }

        Rows?.Insert(0, item);
    }

    private void PersistUpdated(CruddyRow item)
    {
        var keyValue = GetKeyValue(item);

        var rowsAffected = Update(item, keyValue!);

        if (rowsAffected > 0 && Rows != null)
        {
            var original = Rows.FirstOrDefault(r =>
                string.Equals(GetKeyValue(r)?.ToString(), keyValue?.ToString(), StringComparison.OrdinalIgnoreCase));
            if (original != null)
            {
                var idx = Rows.IndexOf(original);
                if (idx >= 0) Rows[idx] = item;
            }
        }
    }

    private async Task ConfirmActionAsync()
    {
        if (ActionItem == null)
        {
            return;
        }

        try
        {
            if (ActionCrud == CrudOperation.Create)
            {
                PersistCreated(ActionItem);
            }
            else if (ActionCrud == CrudOperation.Update)
            {
                PersistUpdated(ActionItem);
            }

            ActionCrud = CrudOperation.None;
            ActionItem = null;
            EditCtx = null;
            if (ReloadPageAfterChange)
            {
                NavigationManager?.NavigateTo(NavigationManager.Uri, forceLoad: true);
            }
            else
            {
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (Exception ex)
        {
            if (ActionCrud == CrudOperation.Create || ActionCrud == CrudOperation.Update)
            {
                EditErrorMessage = $"Update failed: {ex.Message}";
            }
            else
            {
                ErrorMessage = $"Action failed: {ex.Message}";
            }
            await ShowErrorMessage();
        }
    }

    /// <summary>
    /// Called when a single field is changed in the edit form. Updates the field in the row being edited
    /// and notifies the EditContext.
    /// </summary>
    /// <param name="field">Field being changed.</param>
    /// <param name="value">New value from the input event.</param>
    protected void OnFieldChanged(CruddyField field, object? value)
    {
        if (ActionItem == null) return;

        try
        {
            ActionItem[field.Name] = TypeHelper.TryParseValue(value, field.Type);
            EditCtx?.NotifyFieldChanged(new FieldIdentifier(ActionItem, field.Name));
        }
        catch
        {
            // ignore conversion errors here
        }
    }

    #endregion

    #region Delete

    private Task ConfirmAndDeleteAsync(CruddyRow item)
    {
        ActionItem = item;
        ActionCrud = CrudOperation.Delete;
        return Task.CompletedTask;
    }

    private async Task ConfirmDeleteAsync()
    {
        var item = ActionItem;
        ActionCrud = CrudOperation.None;
        ActionItem = null;

        if (item == null) return;

        try
        {
            Delete(GetKeyValue(item)!);

            Rows?.Remove(item);
            if (ReloadPageAfterChange)
            {
                NavigationManager?.NavigateTo(NavigationManager.Uri, forceLoad: true);
            }
            else
            {
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Delete failed: {ex.Message}";
            await ShowErrorMessage();
        }
    }

    private string DeleteMessage(CruddyRow item)
    {
        var sureToDelete = "Sure you want to delete " + TableName.ToLower() + " row ";
        var keyInfo = HideKeyColumn ? "" : $"({KeyColumn}={GetKeyValue(item)})";
        var nameUxValue = string.IsNullOrEmpty(NameUx) ? "" : GetCell(item, NameUx)?.ToString() ?? "";
        if (!string.IsNullOrEmpty(nameUxValue))
        {
            nameUxValue += " ";
        }

        return string.IsNullOrEmpty(nameUxValue)
            ? $"{sureToDelete}{keyInfo}?"
            : $"{sureToDelete}{nameUxValue}{keyInfo}?";
    }

    #endregion

    #region AddColumns 

    /// <summary>
    /// Add a simple column for each CRUD field.
    /// </summary>
    protected virtual void AddColumnsToGrid()
    {
        foreach (var field in Fields)
        {
            AddColumn(field);
        }

        if (AllowCrud || AllowDetails || AllowEdit || AllowDelete)
        {
            AddActionColumn();
        }
    }

    /// <summary>
    /// Adds an action column to the QuickGrid, with buttons for each row 
    /// that prompts the user for confirmation and performs the actions if confirmed.
    /// </summary>
    private void AddActionColumn()
    {
        RenderFragment<CruddyRow> actionTemplate = item => builder =>
        {
            if (AllowCrud || AllowDetails)
            {
                builder.OpenElement(0, "button");
                builder.AddAttribute(1, "class", "btn btn-sm btn-primary me-1");
                builder.AddAttribute(2, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => ShowDetails(item)));
                builder.AddContent(3, "Details");
                builder.CloseElement();
            }

            if (AllowCrud || AllowEdit)
            {
                builder.OpenElement(4, "button");
                builder.AddAttribute(5, "class", "btn btn-sm btn-secondary me-1");
                builder.AddAttribute(6, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, async () => await ShowEdit(CrudOperation.Update, item)));
                builder.AddContent(7, "Edit");
                builder.CloseElement();
            }

            if (AllowCrud || AllowDelete)
            {
                builder.OpenElement(8, "button");
                builder.AddAttribute(9, "class", "btn btn-sm btn-danger");
                builder.AddAttribute(10, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, async () => await ConfirmAndDeleteAsync(item)));
                builder.AddContent(11, "Delete");
                builder.CloseElement();
            }
        };

        MyColumns.Add(builder =>
        {
            builder.OpenComponent<TemplateColumn<CruddyRow>>(0);
            builder.AddAttribute(1, nameof(TemplateColumn<>.ChildContent), actionTemplate);
            builder.AddAttribute(2, nameof(TemplateColumn<>.Title), ActionsTitle);
            builder.AddAttribute(3, nameof(TemplateColumn<>.Class), "text-left");
            builder.CloseComponent();
        });
    }

    /// <summary>
    /// Adds a column to the QuickGrid for the specified field.
    /// </summary>
    /// <param name="field">The field to add a column for.</param>
    private void AddColumn(CruddyField field)
    {
        var ii = GetInputInfo(field, null, CrudOperation.Read);
        if (!ii.Visible)
        {
            return;
        }

        if (HandleFKey(field, ii.DisplayName))
        {
            return;
        }

        RenderFragment<CruddyRow> template = item => builder =>
        {
            builder.AddContent(0, FormatValue(GetCell(item, field.Name), ii.DisplayFormat));
        };

        MyColumns.Add(builder =>
        {
            builder.OpenComponent<TemplateColumn<CruddyRow>>(0);
            builder.AddAttribute(1, nameof(TemplateColumn<>.ChildContent), template);
            builder.AddAttribute(2, nameof(TemplateColumn<>.Title), ii.DisplayName);
            builder.AddAttribute(3, nameof(TemplateColumn<>.Align), Align.Left);
            builder.AddAttribute(4, nameof(TemplateColumn<>.Sortable), true);
            builder.AddAttribute(5, nameof(TemplateColumn<>.SortBy),
                GridSort<CruddyRow>.ByAscending(item => GetCell(item, field.Name)));
            builder.CloseComponent();
        });
    }

    /// <summary>
    /// Handles the rendering of a (possible) foreign key column in the QuickGrid. If the field is 
    /// a foreign key, it retrieves the options for the foreign key and creates a template column 
    /// that displays the corresponding display value instead of the raw foreign key value.
    /// </summary>
    /// <param name="field">The field that might be a foreign key.</param>
    /// <param name="columnTitle">The title of the column to be displayed in the QuickGrid.</param>
    /// <returns>True if the field is a foreign key and the column was added; otherwise, false.</returns>
    public bool HandleFKey(CruddyField field, string columnTitle)
    {
        LoadFKeyOptions();
        var fkOptions = GetFKeyOptions(field);
        if (fkOptions is not null)
        {
            RenderFragment<CruddyRow> fkTemplate = item => builder =>
            {
                builder.AddContent(0, GetForeignKeyDisplayValue(field, item, fkOptions));
            };

            MyColumns.Add(builder =>
            {
                builder.OpenComponent<TemplateColumn<CruddyRow>>(0);
                builder.AddAttribute(1, nameof(TemplateColumn<>.ChildContent), fkTemplate);
                builder.AddAttribute(2, nameof(TemplateColumn<>.Title), columnTitle);
                builder.AddAttribute(3, nameof(TemplateColumn<>.Align), Align.Left);
                builder.AddAttribute(4, nameof(TemplateColumn<>.Sortable), true);
                builder.AddAttribute(5, nameof(TemplateColumn<>.SortBy),
                    GridSort<CruddyRow>.ByAscending(item => GetForeignKeyDisplayText(field, item, fkOptions)));
                builder.CloseComponent();
            });
            return true;
        }

        return false;
    }

    /// <summary>
    /// Resolves the display value for a foreign key field from the provided option list.
    /// </summary>
    /// <remarks>Matching first uses object equality on option identifiers and then compares string
    /// representations.</remarks>
    /// <param name="field">The foreign key field.</param>
    /// <param name="item">The row that contains the foreign key value.</param>
    /// <param name="fkOptions">Available foreign key options to match by identifier.</param>
    /// <returns>The matched option display value when a corresponding identifier is found; otherwise, the original 
    /// value.</returns>
    public static object? GetForeignKeyDisplayValue(CruddyField field, CruddyRow item, List<FKeyOption> fkOptions)
    {
        var value = GetCell(item, field.Name);
        var option = value is null
            ? null
            : fkOptions.FirstOrDefault(o => Equals(o.Id, value) || o.Id?.ToString() == value.ToString());
        return option is not null ? option.Display : value;
    }

    /// <summary>
    /// Gets the display text for a foreign key field value in the specified row.
    /// </summary>
    /// <param name="field">The field that represents the foreign key.</param>
    /// <param name="item">The row that contains the foreign key value.</param>
    /// <param name="fkOptions">The available foreign key options used to resolve the display value.</param>
    /// <returns>The string representation of the resolved foreign key display value, or <see langword="null"/> when no value is
    /// resolved.</returns>
    public static string? GetForeignKeyDisplayText(CruddyField field, CruddyRow item, List<FKeyOption> fkOptions) =>
        GetForeignKeyDisplayValue(field, item, fkOptions)?.ToString();

    #endregion

    #region Misc methods

    /// <summary>
    /// Gets the InputInfo for a given field, row and operation. This is used to 
    /// generate the columns in the QuickGrid and the input fields in the modals.
    /// </summary>
    /// <param name="field">The field for which to get the InputInfo.</param>
    /// <param name="item">The row from which to get the field value.</param>
    /// <param name="operation">The CRUD operation being performed.</param>
    /// <returns>An InputInfo object containing metadata about the field.</returns>
    public InputInfo GetInputInfo(CruddyField field, CruddyRow? item, CrudOperation operation)
    {
        var ii = new InputInfo
        {
            DisplayName = field.Name,
            DisplayFormat = GetDisplayFormat(field),
            Type = field.Type,
            Disabled = !IncludeColumn(operation, field.Column),
            Required = !field.IsNullable,
            HideKeyColumn = HideKeyColumn,
            IsKeyColumn = string.Equals(field.Column, KeyColumn, StringComparison.OrdinalIgnoreCase),
            DataType = field.IsDate ? DataType.Date : null,
        };

        if (item is not null)
        {
            ii.Value = GetCell(item, field.Name);
            ii.FormattedValue = FormatValue(ii.Value, ii.DisplayFormat);
        }

        return ii;
    }

    /// <summary>
    /// Gets the foreign key option for the specified field that matches the current input value.
    /// </summary>
    /// <param name="field">The field whose foreign key options are evaluated.</param>
    /// <param name="ii">The input data containing the value used to find a matching option.</param>
    /// <returns>The matching <c>FKeyOption</c> when a value is present and a match is found; otherwise, <see langword="null"/>.</returns>
    public FKeyOption? GetFkeyOption(CruddyField field, InputInfo ii)
    {
        var fkeyOptions = GetFKeyOptions(field);
        var fkeyOption = ii.Value is null ? null :
            fkeyOptions?.FirstOrDefault(o => Equals(o.Id, ii.Value) || o.Id?.ToString() == ii.Value.ToString());
        return fkeyOption;
    }

    private async Task ShowErrorMessage(int showDuration = 8000)
    {
        await InvokeAsync(StateHasChanged);

        // Clear the message after a short delay
        _ = Task.Run(async () =>
        {
            await Task.Delay(showDuration);
            ErrorMessage = null;
            EditErrorMessage = null;
            await InvokeAsync(StateHasChanged);
        });
    }

    /// <summary>
    /// Formats a value using the format if the value is formattable, otherwise uses ToString().
    /// </summary>
    protected static string FormatValue(object? value, string? format)
    {
        if (value == null)
        {
            return string.Empty;
        }

        if (!string.IsNullOrEmpty(format) && value is IFormattable f)
        {
            try
            {
                return f.ToString(format, CultureInfo.CurrentCulture);
            }
            catch (FormatException)
            {
                // the format does not fit the type, so fall through to ToString()
            }
        }

        return value.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Gets the default display format for the specified field.
    /// </summary>
    /// <param name="field">The field to get the display format for.</param>
    /// <returns>The display format string, or null if none is specified.</returns>
    protected string? GetDisplayFormat(CruddyField field)
    {
        if (TypeHelper.IsDecimal(field.Type))
        {
            return string.IsNullOrEmpty(DefaultDecimalFormat) ? null : DefaultDecimalFormat;
        }

        if (TypeHelper.IsNumber(field.Type))
        {
            return string.IsNullOrEmpty(DefaultNumberFormat) ? null : DefaultNumberFormat;
        }

        if (field.IsDate)
        {
            return string.IsNullOrEmpty(DefaultDateFormat) ? null : DefaultDateFormat;
        }

        if (field.Type == typeof(DateTime))
        {
            return string.IsNullOrEmpty(DefaultDateTimeFormat) ? null : DefaultDateTimeFormat;
        }

        return null;
    }

    #endregion

}
