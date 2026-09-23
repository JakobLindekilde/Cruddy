using CruddyDemo.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.AspNetCore.Components.Web;
using QuickGrid.Toolkit;
using QuickGrid.Toolkit.Columns;
using System.Linq.Expressions;
using System.Reflection;

// More stuff to do:
// TODO: Details: Get data from database, not from the item passed in
// TODO: Display all column headers in bold (not just the Actions column)
// TODO: Make unittests for the Cruddy component: AddColumnsToGrid(). GetDisplayFormat() etc.
// TODO: Make the AddSimple, AddNumber and AddSimpleDate methods more robust, so that they can
//       handle more types of properties (e.g. nullable types)
// TODO: Make kode to display "class in class" e.g. make a column for a property that is a class,
//       and display its Name property (e.g. Address.Customer.Name)
// TODO: Dont show ICollection properties (e.g. Employee.Orders)
// TODO: Write code that finds the correct generics like AddSimple and AddNumber in ColumnManager

namespace CruddyDemo.Components
{
    /// <summary>
    /// Lists the rows of a database table in a QuickGrid component, 
    /// with columns automatically generated from the public properties of TEntity.
    /// </summary>
    public partial class Cruddy<TEntity> : CruddyBase<TEntity> //where TEntity : class, record, new()
    {
        #region Parameters

        /// <summary>
        /// The default date format to use when displaying date values 
        /// when not using <seealso cref="DisplayFormatAttribute"/>.
        /// </summary>
        [Parameter]
        public string? DefaultDateFormat { get; set; } = "dd-MM-yyyy";

        /// <summary>
        /// The default decimal, double and float format to use when displaying decimal values 
        /// when not using <seealso cref="DisplayFormatAttribute"/>.
        /// </summary>
        [Parameter]
        public string? DefaultDecimalFormat { get; set; } = "0.00";

        /// <summary>
        /// The default format to use when displaying numbers 
        /// when not using <seealso cref="DisplayFormatAttribute"/>.
        /// </summary>
        [Parameter]
        public string? DefaultNumberFormat { get; set; } = "0";

        /// <summary>
        /// The title to use for the actions column.
        /// </summary>
        [Parameter]
        public string? ActionsTitle { get; set; } = "Actions";

        /// <summary>
        /// Same as setting AllowDetails, AllowEdit and AllowDelete to true.
        /// </summary>
        [Parameter]
        public bool AllowCrud { get; set; } = false;


        /// <summary>
        /// Whether to enable the details functionality for each row.
        /// </summary>
        [Parameter]
        public bool AllowDetails { get; set; } = false;

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
        /// The name of the column/field, used for display purposes in the UX. 
        /// Used in messages like "Are you sure you want to delete this {NameUx}?".
        /// </summary>
        [Parameter]
        public string? NameUx { get; set; }

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

        #endregion

        #region Lifecycle methods

        /// <summary>
        /// During component initialization, this method fills <see cref="ColumnAliasDict"/>, 
        /// adds columns to the grid and retrieves the rows from the database.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            AddColumnsToGrid();
        }

        #endregion

        #region Details

        /// <summary>
        /// The currently selected item shown in the details modal.
        /// </summary>
        protected TEntity? DetailsItem { get; set; }

        /// <summary>
        /// Whether the details modal is visible.
        /// </summary>
        protected bool ShowDetailsModal { get; set; }

        private void ShowDetails(TEntity item)
        {
            // TODO: Get the the item from the database. For now, we just use the item as is.

            // TODO: Consider making this modal a separate component, so that it can be reused and customized.
            // For example, we could have a CruddyDetails<TEntity> component that takes a TEntity parameter and displays
            // its properties in a table or form. Then we could use that component here instead of the inline modal.
            DetailsItem = item;
            ShowDetailsModal = true;
        }

        private void CloseDetails()
        {
            ShowDetailsModal = false;
            DetailsItem = default;
        }

        #endregion

        #region Edit

        /// <summary>
        /// The currently selected item being edited (a copy until saved).
        /// </summary>
        protected TEntity? EditItem { get; set; }

        /// <summary>
        /// Whether the edit modal is visible.
        /// </summary>
        protected bool ShowEditModal { get; set; }

        /// <summary>
        /// EditContext used for DataAnnotations validation.
        /// </summary>
        protected EditContext? EditCtx { get; set; }

        private Task ShowEdit(TEntity item)
        {
            // Create a deep copy of the item so edits are not applied until saved
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var json = System.Text.Json.JsonSerializer.Serialize(item, options);
            EditItem = System.Text.Json.JsonSerializer.Deserialize<TEntity>(json, options);
            EditCtx = new EditContext(EditItem!);
            ShowEditModal = true;
            return Task.CompletedTask;
        }

        private void CancelEdit()
        {
            ShowEditModal = false;
            EditItem = default;
            EditCtx = null;
        }

        private async Task ConfirmEditAsync()
        {
            if (EditItem == null)
            {
                return;
            }

            try
            {
                // Get key value from edited item
                var keyProp = typeof(TEntity).GetProperty(KeyColumn!, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                object? keyValue = null;
                if (keyProp != null) keyValue = keyProp.GetValue(EditItem);

                // Validate using DataAnnotations
                if (EditCtx != null && !EditCtx.Validate())
                {
                    // validation failed - keep modal open and show messages (ValidationSummary will display)
                    await InvokeAsync(StateHasChanged);
                    return;
                }

                // Call base Update method to persist changes
                var rowsAffected = Update(DbConnection, EditItem, keyValue!);

                // Update in-memory list
                if (rowsAffected > 0 && Rows != null)
                {
                    // find original item by key and replace
                    var original = Rows.FirstOrDefault(r => string.Equals(PropertyHelper.GetValue(r!, KeyColumn!), 
                        PropertyHelper.GetValue(EditItem!, KeyColumn!), StringComparison.OrdinalIgnoreCase));
                    if (original != null)
                    {
                        var idx = Rows.IndexOf(original);
                        if (idx >= 0) Rows[idx] = EditItem!;
                    }
                }

                ShowEditModal = false;
                EditItem = default;
                EditCtx = null;
                await InvokeAsync(StateHasChanged);
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
            if (EditItem == null) return;

            try
            {
                object? converted = null;
                if (value == null)
                {
                    converted = null;
                }
                else
                {
                    var targetType = PropertyHelper.GetUnderlyingType(prop.PropertyType);
                    if (targetType == typeof(string)) converted = value.ToString();
                    else if (targetType.IsEnum) converted = Enum.Parse(targetType, value.ToString()!);
                    else if (targetType == typeof(int)) converted = int.TryParse(value.ToString(), out var i) ? i : (int?)null;
                    else if (targetType == typeof(long)) converted = long.TryParse(value.ToString(), out var l) ? l : (long?)null;
                    else if (targetType == typeof(short)) converted = short.TryParse(value.ToString(), out var s) ? s : (short?)null;
                    else if (targetType == typeof(uint)) converted = uint.TryParse(value.ToString(), out var ui) ? ui : (uint?)null;
                    else if (targetType == typeof(ulong)) converted = ulong.TryParse(value.ToString(), out var ul) ? ul : (ulong?)null;
                    else if (targetType == typeof(ushort)) converted = ushort.TryParse(value.ToString(), out var us) ? us : (ushort?)null;
                    else if (targetType == typeof(byte)) converted = byte.TryParse(value.ToString(), out var by) ? by : (byte?)null;
                    else if (targetType == typeof(sbyte)) converted = sbyte.TryParse(value.ToString(), out var sby) ? sby : (sbyte?)null;
                    else if (targetType == typeof(decimal)) converted = decimal.TryParse(value.ToString(), out var d) ? d : (decimal?)null;
                    else if (targetType == typeof(double)) converted = double.TryParse(value.ToString(), out var dd) ? dd : (double?)null;
                    else if (targetType == typeof(float)) converted = float.TryParse(value.ToString(), out var f) ? f : (float?)null;
                    else if (targetType == typeof(bool)) converted = bool.TryParse(value.ToString(), out var b) ? b : (bool?)null;
                    else if (targetType == typeof(DateTime)) converted = DateTime.TryParse(value.ToString(), out var dt) ? dt : (DateTime?)null;
                    else if (targetType == typeof(TimeSpan)) converted = TimeSpan.TryParse(value.ToString(), out var ts) ? ts : (TimeSpan?)null;
                    else if (targetType == typeof(DateTimeOffset)) converted = DateTimeOffset.TryParse(value.ToString(), out var dto) ? dto : (DateTimeOffset?)null;
                    else if (targetType == typeof(Guid)) converted = Guid.TryParse(value.ToString(), out var g) ? g : (Guid?)null;
                    else converted = value;
                }

                prop.SetValue(EditItem, converted);

                if (EditCtx != null)
                {
                    var field = new FieldIdentifier(EditItem, prop.Name);
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

        /// <summary>
        /// The currently selected item pending delete confirmation.
        /// </summary>
        protected TEntity? DeletePendingItem { get; set; }

        /// <summary>
        /// Whether the delete confirmation modal is visible.
        /// </summary>
        protected bool ShowDeleteModal { get; set; }

        private Task ConfirmAndDeleteAsync(TEntity item)
        {
            DeletePendingItem = item;
            ShowDeleteModal = true;
            return Task.CompletedTask;
        }

        private void CancelDelete()
        {
            ShowDeleteModal = false;
            DeletePendingItem = default;
        }

        private async Task ConfirmDeleteAsync()
        {
            var item = DeletePendingItem;
            ShowDeleteModal = false;
            DeletePendingItem = default;
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
                await InvokeAsync(StateHasChanged);
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
            var keyInfo = $"({KeyColumn}={PropertyHelper.GetValue(item!, KeyColumn!)})";
            string nameUxValue = PropertyHelper.GetValue(item!, NameUx!);

            var message = string.IsNullOrEmpty(nameUxValue)
                ? $"{sureToDelete} {keyInfo}?"
                : $"{sureToDelete} {nameUxValue} {keyInfo}?";

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
                    builder.AddAttribute(2, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, async () => await ShowEdit(item)));
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
            if (!PropertyHelper.IsSupported(prop.PropertyType)) return;

            if (PropertyHelper.IsNumber(prop.PropertyType))
            {
                AddNumberColumn(prop);
            }
            else if (prop.PropertyType.Name == "DateTime")
            {
                AddSimpleDateColumn(prop);
            }
            else
            {
                AddSimpleColumn(prop);
            }
        }

        //public DynamicColumn<TGridItem> AddNumber(Expression<Func<TGridItem, decimal?>>        expression, string? title = null, string? fullTitle = null, string format = "N0", string? @class = null, Align align = Align.Right, bool visible = true, string? propertyName = null, bool? calculateTotal = null)
        //public DynamicColumn<TGridItem> AddNumber(Expression<Func<TGridItem, double?>>         expression, string? title = null, string? fullTitle = null, string format = "N0", string? @class = null, Align align = Align.Right, bool visible = true, string? propertyName = null, bool? calculateTotal = null)
        //public DynamicColumn<TGridItem> AddSimpleDate<TValue>(Expression<Func<TGridItem, TValue?>> expression, string? title = null, string? fullTitle = null, string? format = "dd/MM/yyyy", string? @class = null, Align align = Align.Center, CellStyleMap<TValue>? cellStyle = null, bool visible = true)
        //public DynamicColumn<TGridItem> AddSimple    <TValue>(Expression<Func<TGridItem, TValue?>> expression, ColumnInfo columnInfo, string? format = null, Align align = Align.Left, CellStyleMap<TValue>? cellStyle = null, GridSort<TGridItem>? sortBy = null,  bool visible = true, string? propertyName = null)

        /// <summary>
        /// Adds a Toolkit.AddSimple column to the QuickGrid for the specified property.
        /// </summary>
        /// <param name="prop">The property to add a column for.</param>
        private void AddSimpleColumn(PropertyInfo prop)
        {
            // TODO: Make sure to get the correct overload of AddSimple(). Count the parameters!
            var method = typeof(ColumnManager<TEntity>).GetMethods()
                .FirstOrDefault(m => m.Name == "AddSimple" && m.GetParameters()[0].ParameterType.Name.StartsWith("Expression"));

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

                    var displayName = PropertyHelper.GetDisplayName(prop);
                    var format = GetDisplayFormat(prop);
                    var columnInfo = new ColumnInfo(displayName, displayName, null);
                    genericMethod.Invoke(MyColumnManager, [lambda, columnInfo, format, Align.Left, null, null, true, null]);
                    // Would be nice if the code below worked, but it doesn't because of the generic type parameter. So we have to use reflection to invoke the method.
                    //MyColumnManager.AddSimple(           lambda, columnInfo, format, Align.Left, null, null, true, null);
                }
                catch
                {
                    // ignore any failures adding a specific column
                    // TODO: What should we do here? Log the error? Show a message in the UI?
                }
            }
        }

        /// <summary>
        /// Adds a Toolkit.AddNumber column to the QuickGrid for the specified property.
        /// </summary>
        /// <param name="prop">The property to add a column for.</param>
        private void AddNumberColumn(PropertyInfo prop)
        {
            var method = typeof(ColumnManager<TEntity>).GetMethods()
                .FirstOrDefault(m => m.Name == "AddNumber" && m.GetParameters()[0].ParameterType.Name.StartsWith("Expression"));

            if (method != null)
            {
                try
                {
                    Type entityType = typeof(TEntity);
                    var param = Expression.Parameter(entityType, "p");
                    var access = Expression.PropertyOrField(param, prop.Name);

                    var firstParamType = method.GetParameters()[0].ParameterType; // Expression<TDelegate>
                    var delegateType = firstParamType.GetGenericArguments()[0];          // TDelegate (e.g. Func<Customer, Nullable<decimal>>)
                    var returnType = delegateType.GetMethod("Invoke")!.ReturnType;        // Nullable<decimal> (or decimal/other)

                    // convert access to the expected return type if needed
                    Expression body = access;
                    if (access.Type != returnType)
                    {
                        body = Expression.Convert(access, returnType);
                    }

                    // create a strongly-typed lambda matching the overload
                    var lambda = Expression.Lambda(delegateType, body, param);

                    var displayName = PropertyHelper.GetDisplayName(prop);
                    var format = GetDisplayFormat(prop);
                    method.Invoke(MyColumnManager, [lambda, displayName, displayName, format, null, Align.Left, true, null, null]);
                    // Would be nice if the code below worked, but it doesn't because of the generic type parameter. So we have to use reflection to invoke the method.
                    //MyColumnManager.AddNumber(    lambda, displayName, displayName, format, null, Align.Left, true, null, null);
                }
                catch
                {
                    // ignore any failures adding a specific column
                    // TODO: What should we do here? Log the error? Show a message in the UI?
                }
            }
        }

        /// <summary>
        /// Adds a Toolkit.AddSimpleDate column to the QuickGrid for the specified property.
        /// </summary>
        /// <param name="prop">The property to add a column for.</param>
        private void AddSimpleDateColumn(PropertyInfo prop)
        {
            var method = typeof(ColumnManager<TEntity>).GetMethods()
                .FirstOrDefault(m => m.Name == "AddSimpleDate" && m.GetParameters()[0].ParameterType.Name.StartsWith("Expression"));

            if (method != null)
            {
                // AddSimpleDate is a generic method, so we need to make it generic with the correct type argument. In this case,
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

                    var displayName = PropertyHelper.GetDisplayName(prop);
                    var format = GetDisplayFormat(prop);
                    genericMethod.Invoke(MyColumnManager, [lambda, displayName, displayName, format, null, Align.Left, null, true]);
                    // Would be nice if the code below worked, but it doesn't because of the generic type parameter. So we have to use reflection to invoke the method.
                    //MyColumnManager.AddSimpleDate(       lambda, displayName, displayName, format, null, Align.Left, null, true); 
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

            var propertyType = PropertyHelper.GetUnderlyingType(prop.PropertyType);

            if (PropertyHelper.IsDecimal(propertyType))
            {
                return string.IsNullOrEmpty(DefaultDecimalFormat) ? null : DefaultDecimalFormat;
            }

            if (PropertyHelper.IsNumber(propertyType))
            {
                return string.IsNullOrEmpty(DefaultNumberFormat) ? null : DefaultNumberFormat;
            }

            // TODO: Should we handle DateTimeOffset like DateTime?
            if (propertyType == typeof(DateTime))
            {
                return string.IsNullOrEmpty(DefaultDateFormat) ? null : DefaultDateFormat;
            }

            return null;
        }

        #endregion

    }
}
