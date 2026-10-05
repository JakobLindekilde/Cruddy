using Cruddy.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.AspNetCore.Components.Web;
using QuickGrid.Toolkit;
using QuickGrid.Toolkit.Columns;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

// More stuff to do:
// TODO: Display all column headers in bold (not just the Actions column)
// TODO: Make unittests for the Cruddy component: AddColumnsToGrid(). GetDisplayFormat() etc.


namespace Cruddy.Components;

/// <summary>
/// Lists the rows of a database table in a QuickGrid component, 
/// with columns automatically generated from the public properties of TEntity.
/// </summary>
public partial class Cruddy<TEntity> : CruddyBase<TEntity> //where TEntity : class, record, new()
{
    #region Parameters

    /// <summary>
    /// The default date format to use when displaying date values when not 
    /// using <seealso cref="DisplayFormatAttribute"/>. Default format is d (aka 'Short date pattern').
    /// </summary>
    [Parameter]
    public string? DefaultDateFormat { get; set; } = "d";

    /// <summary>
    /// The default date format to use when displaying datetime values when not using <seealso cref="DisplayFormatAttribute"/>.
    /// Default format is g (aka 'general date/time pattern (short time)').
    /// </summary>
    [Parameter]
    public string? DefaultDateTimeFormat { get; set; } = "g";

    /// <summary>
    /// The default date format to use when displaying time values when not 
    /// using <seealso cref="DisplayFormatAttribute"/>. Default format is t (aka 'short time pattern').
    /// </summary>
    [Parameter]
    public string? DefaultTimeFormat { get; set; } = "t";

    /// <summary>
    /// The default decimal, double and float format to use when displaying decimal values 
    /// when not using <seealso cref="DisplayFormatAttribute"/>. Default format is 0.00.
    /// </summary>
    [Parameter]
    public string? DefaultDecimalFormat { get; set; } = "0.00";

    /// <summary>
    /// The default format to use when displaying numbers when not 
    /// using <seealso cref="DisplayFormatAttribute"/>. Default format is 0.
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
    /// If "*" is specified, all public readable properties of TEntity will be displayed.
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
    protected QuickGrid<TEntity>? MyGrid;

    /// <summary>
    /// The ColumnManager that manages the columns of the QuickGrid component.
    /// </summary>
    protected readonly ColumnManager<TEntity> MyColumnManager = new();

    /// <summary>
    /// Holds a user visible error message when e.g. delete fails.
    /// </summary>
    protected string? ErrorMessage { get; set; }

    /// <summary>
    /// Navigation manager used to reload the page when ReloadPageAfterChange is true.
    /// </summary>
    [Inject]
    protected NavigationManager? NavigationManager { get; set; }

    /// <summary>
    /// The currently selected item being crud'ed (a copy).
    /// </summary>
    protected TEntity? ActionItem { get; set; }

    /// <summary>
    /// The currently selected item being crud'ed (a copy).
    /// </summary>
    protected CrudOperation ActionCrud { get; set; } = CrudOperation.None;

    #endregion

    #region Lifecycle methods

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

    private void ShowDetails(TEntity item)
    {
        if (string.IsNullOrEmpty(DetailsColumns))
        {
            ActionItem = item;
        }
        else
        {
            var keyValue = PropertyHelper.GetValue(item!, KeyColumn!);
            ActionItem = GetTableRow(DbConnection, keyValue, DetailsColumns);
        }

        ActionCrud = CrudOperation.Read;
    }

    #endregion

    #region Edit

    /// <summary>
    /// EditContext used for DataAnnotations validation.
    /// </summary>
    protected EditContext? EditCtx { get; set; }

    private Task ShowEdit(CrudOperation actionCrud, TEntity item = default!)
    {
        ActionItem = actionCrud == CrudOperation.Create ? 
                CruddyHelper.NewInstance<TEntity>() :
                CruddyHelper.DeepCopy(item);

        EditCtx = new EditContext(ActionItem!);
        ActionCrud = actionCrud;
        return Task.CompletedTask;
    }

    private void CancelAction()
    {
        ActionCrud = CrudOperation.None;
        ActionItem = default;

        if (ActionCrud == CrudOperation.Create || ActionCrud == CrudOperation.Update)
        {
            EditCtx = null;
        }
    }

    private void PersistCreated(TEntity item)
    {
        // Persist via base Create method
        var result = Create(DbConnection, item);

        // If a scalar id was returned, attempt to set the key property
        var keyProp = PropertyHelper.GetProperty(typeof(TEntity), KeyColumn!);
        if (keyProp != null && result != null)
        {
            try
            {
                var converted = Convert.ChangeType(result, TypeHelper.GetUnderlyingType(keyProp.PropertyType));
                keyProp.SetValue(item, converted);
            }
            catch
            {
                // ignore conversion errors
            }
        }

        if (Rows != null)
        {
            Rows.Insert(0, item!);
        }
    }

    private void PersistUpdated(TEntity item)
    {
        // Get key value from edited item
        var keyProp = PropertyHelper.GetProperty(typeof(TEntity), KeyColumn!);
        object? keyValue = null;
        if (keyProp != null) keyValue = keyProp.GetValue(item);

        // Call base Update method to persist changes
        var rowsAffected = Update(DbConnection, item, keyValue!);

        // Update in-memory list
        if (rowsAffected > 0 && Rows != null)
        {
            // find original item by key and replace
            var original = Rows.FirstOrDefault(r => string.Equals(PropertyHelper.GetValue(r!, KeyColumn!),
                PropertyHelper.GetValue(item!, KeyColumn!), StringComparison.OrdinalIgnoreCase));
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
            // Validate using DataAnnotations
            if (EditCtx != null && !EditCtx.Validate())
            {
                // validation failed - keep modal open and show messages (ValidationSummary will display)
                await InvokeAsync(StateHasChanged);
                return;
            }

            if (ActionCrud == CrudOperation.Create)
            {
                PersistCreated(ActionItem);
            }
            else if (ActionCrud == CrudOperation.Update)
            {
                PersistUpdated(ActionItem);
            }

            ActionCrud = CrudOperation.None;
            ActionItem = default;
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
            ErrorMessage = $"Update failed: {ex.Message}";
            await ShowErrorMessage();
        }
    }


    /// <summary>
    /// Called when a single field is changed in the edit form. Updates the EditItem property via reflection
    /// and notifies the EditContext for validation.
    /// </summary>
    /// <param name="prop">Property being changed.</param>
    /// <param name="value">New value (as string) from the input event.</param>
    protected void OnFieldChanged(PropertyInfo prop, object? value)
    {
        if (ActionItem == null) return;

        try
        {
            var targetType = TypeHelper.GetUnderlyingType(prop.PropertyType);
            object? converted = TypeHelper.TryParseValue(value, targetType);
            prop.SetValue(ActionItem, converted);

            if (EditCtx != null)
            {
                var field = new FieldIdentifier(ActionItem, prop.Name);
                EditCtx.NotifyFieldChanged(field);
            }
        }
        catch
        {
            // ignore conversion errors here; validation will catch invalid values on submit
        }
    }


    #endregion

    #region Delete

    private Task ConfirmAndDeleteAsync(TEntity item)
    {
        ActionItem = item;
        ActionCrud = CrudOperation.Delete;
        return Task.CompletedTask;
    }

    private async Task ConfirmDeleteAsync()
    {
        var item = ActionItem;
        ActionCrud = CrudOperation.None;
        ActionItem = default;
        //TODO: Can we just delete line "if (item == null) return;"?
#pragma warning disable S2955   // SonarQube: "null" should not be passed as an argument to a non-nullable parameter
        if (item == null) return;
#pragma warning restore S2955

        try
        {
            var keyValue = PropertyHelper.GetValue(item, KeyColumn!);
            Delete(DbConnection, keyValue);

            // Remove the item from the in-memory rows and refresh UI
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
            // Show an error message to the user and refresh the UI
            ErrorMessage = $"Delete failed: {ex.Message}";
            await ShowErrorMessage();
        }
    }

    private string DeleteMessage(TEntity item)
    {
        var sureToDelete = "Sure you want to delete " + typeof(TEntity).Name.ToLower();
        var keyInfo = HideKeyColumn ? "" : $" ({KeyColumn}={PropertyHelper.GetValue(item!, KeyColumn!)})";
        string nameUxValue = PropertyHelper.GetValue(item!, NameUx!);
        if (!string.IsNullOrEmpty(nameUxValue))
        {
            nameUxValue += " ";
        }

        var message = string.IsNullOrEmpty(nameUxValue)
            ? $"{sureToDelete}{keyInfo}?"
            : $"{sureToDelete}{nameUxValue}{keyInfo}?";

        return message;
    }

    #endregion

    #region AddColumns 

    /// <summary>
    /// Add a simple column, using AddSimple(), for each public readable property on TEntity.
    /// </summary>
    protected virtual void AddColumnsToGrid()
    {
        var props = PropertyHelper.GetReadProperties(typeof(TEntity));

        if (TableColumns == "*")
        {
            foreach (var prop in props)
            {
                AddColumn(prop);
            }
        }
        else
        {
            foreach (var col in ColumnAliasDict)
            {
                var prop = props.FirstOrDefault(p => p.Name.Equals(col.Value, StringComparison.OrdinalIgnoreCase));
                if (prop != null)
                {
                    AddColumn(prop);
                }
            }
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
        RenderFragment deleteTemplate(TEntity item) => (builder) =>
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
                builder.OpenElement(0, "button");
                builder.AddAttribute(1, "class", "btn btn-sm btn-secondary me-1");
                builder.AddAttribute(2, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, async () => await ShowEdit(CrudOperation.Update, item)));
                builder.AddContent(3, "Edit");
                builder.CloseElement();
            }

            if (AllowCrud || AllowDelete)
            {
                builder.OpenElement(0, "button");
                builder.AddAttribute(1, "class", "btn btn-sm btn-danger");
                builder.AddAttribute(2, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, async () => await ConfirmAndDeleteAsync(item)));
                builder.AddContent(3, "Delete");
                builder.CloseElement();
            }
        };

        MyColumnManager.AddTemplateColumn(deleteTemplate, title: ActionsTitle, cssClass: "text-center");
    }

    /// <summary>
    /// Adds a column to the QuicGrid for the specified property
    /// </summary>
    protected virtual void AddColumn(PropertyInfo prop)
    {
        if (!TypeHelper.IsSupported(prop.PropertyType))
        {
            return;
        }

        AddSimpleColumn(prop);
    }

    /// <summary>
    /// Adds a Toolkit.AddSimple column to the QuickGrid for the specified property.
    /// </summary>
    /// <param name="prop">The property to add a column for.</param>
    private void AddSimpleColumn(PropertyInfo prop)
    {
        var method = CruddyHelper.GetAddSimpleMethod<TEntity>();

        if (method != null)
        {
            // AddSimple is a generic method, so we need to make it generic with the correct type argument. In this case,
            // we can use object as the type argument, since we don't know the actual type of the property at compile time.
            var genericMethod = method.MakeGenericMethod(typeof(object));

            try
            {
                Type entityType = typeof(TEntity);
                var param = Expression.Parameter(entityType, "p");
                var access = Expression.PropertyOrField(param, prop.Name);

                var delegateType = typeof(Func<,>).MakeGenericType(entityType, typeof(object));
                var returnType = delegateType.GetMethod("Invoke")!.ReturnType;

                // convert access to the expected return type if needed
                Expression body = access;
                if (access.Type != returnType)
                {
                    body = Expression.Convert(access, returnType);
                }

                // create a strongly-typed lambda matching the overload
                var lambda = Expression.Lambda(delegateType, body, param);

                var ii = GetInputInfo(prop, default(TEntity), CrudOperation.Read);
                var columnInfo = new ColumnInfo(ii.DisplayName, ii.DisplayName, null);
                genericMethod.Invoke(MyColumnManager, [lambda, columnInfo, ii.DisplayFormat, Align.Left, null, null, ii.Visible, null]);
                // Would be nice if the code below worked, but it doesn't because of the generic type parameter. So we have to use reflection to invoke the method.
                //MyColumnManager.AddSimple(           lambda, columnInfo, ii.DisplayFormat, Align.Left, null, null, ii.Visible, null);
            }
            catch
            {
                // ignore any failures adding a specific column
                // TODO: What should we do here? Log the error? Show a message in the UI?
            }
        }
    }

    #endregion

    #region Misc methods

    /// <summary>
    /// Gets the InputInfo for a given property, item and operation. This is used to 
    /// generate the columns in the QuickGrid and the input fields in the modals.
    /// </summary>
    /// <param name="prop">The property for which to get the InputInfo.</param>
    /// <param name="item">The item from which to get the property value.</param>
    /// <param name="operation">The CRUD operation being performed.</param>
    /// <returns>An InputInfo object containing metadata about the property.</returns>
    public InputInfo GetInputInfo(PropertyInfo prop, TEntity? item, CrudOperation operation)
    {
        var ii = new InputInfo
        {
            DisplayName = PropertyHelper.GetDisplayName(prop),
            DisplayFormat = GetDisplayFormat(prop),
            Type = TypeHelper.GetUnderlyingType(prop.PropertyType),
            Disabled = !IncludeColumn(operation, prop.Name),
            Required = !PropertyHelper.IsNullable(prop),
            HideKeyColumn = HideKeyColumn,
            IsKeyColumn = string.Equals(prop.Name, KeyColumn, StringComparison.OrdinalIgnoreCase),
            DataType = prop.GetCustomAttribute<DataTypeAttribute>()?.DataType,
        };

        if (!object.Equals(item, default(TEntity)))
        {
            ii.Value = prop.GetValue(item);
            ii.FormattedValue = PropertyHelper.GetFormattedValue(prop, item);
        }

        return ii;
    }
    private async Task ShowErrorMessage(int showDuration = 8000)
    {
        await InvokeAsync(StateHasChanged);

        // Clear the message after a short delay
        _ = Task.Run(async () =>
        {
            await Task.Delay(showDuration);
            ErrorMessage = null;
            await InvokeAsync(StateHasChanged);
        });
    }

    /// <summary>
    /// Gets the display format for the specified property either 
    /// from the DisplayFormatAttribute, or from the default formats.
    /// </summary>
    /// <param name="prop">The property to get the display format for.</param>
    /// <returns>The display format string, or null if none is specified.</returns>
    protected string? GetDisplayFormat(PropertyInfo prop)
    {
        var displayFormat = PropertyHelper.GetDisplayFormat(prop);
        if (!string.IsNullOrEmpty(displayFormat))
        {
            return displayFormat;
        }

        var propertyType = TypeHelper.GetUnderlyingType(prop.PropertyType);

        if (TypeHelper.IsDecimal(propertyType))
        {
            return string.IsNullOrEmpty(DefaultDecimalFormat) ? null : DefaultDecimalFormat;
        }

        if (TypeHelper.IsNumber(propertyType))
        {
            return string.IsNullOrEmpty(DefaultNumberFormat) ? null : DefaultNumberFormat;
        }

        switch (propertyType.Name)
        {
            case "TimeOnly": return string.IsNullOrEmpty(DefaultTimeFormat) ? null : DefaultTimeFormat;
            case "DateOnly": return string.IsNullOrEmpty(DefaultDateFormat) ? null : DefaultDateFormat;
            case "DateTime": return string.IsNullOrEmpty(DefaultDateTimeFormat) ? null : DefaultDateTimeFormat;
            default: return null;
        }
    }

    #endregion

}
