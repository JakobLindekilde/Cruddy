# Cruddy - BETA

- Cruddy is a ASP.NET Blazor componet that does CRUD operations on any table in any database
- Cruddy is based on the Microsoft QuickGrid component
- Cruddy requires a model class or record (TEntity)
- The model class/record properties does not need to match the database table columns
- Cruddy uses Dapper - not Entity Framework
- Cruddy uses ColumnManager from QuickGrid.Toolkit

## QuickGrid.Toolkit

**▶ Live demo: <https://vaclavelias.github.io/QuickGrid.Toolkit/>**

## Examples 

Most simple use (no CRUD):
```razor
@page "/Products"
@inject Microsoft.Data.SqlClient.SqlConnection dbConnection

<h3>Table Products</h3>

<h4>Simple as possible</h4>
<Cruddy TEntity="Models.Product" DbConnection="dbConnection" />

@code {}
```

Typical use (with CRUD):
```razor
@page "/Customers"
@inject Microsoft.Data.SqlClient.SqlConnection dbConnection

<h3>Table Customers</h3>

<Cruddy TEntity="Models.Customer" TableName="Customers" AllowCrud="true" DbConnection="dbConnection" />

@code {}
```

Advanced use example 1:
```razor
@page "/AnyPoco"
@inject Microsoft.Data.SqlClient.SqlConnection dbConnection

<h3>Select into any POCO class</h3>

<h4>SELECT Id, Name, LTRIM(STR(Price)) AS Info FROM Products</h4>
<Cruddy TEntity="Models.AnyPoco" Select="SELECT Id, Name, LTRIM(STR(Price)) AS Info FROM Products" DbConnection="dbConnection" />

@code {}
```

Advanced use example 2:
```razor
<Cruddy TEntity="Models.OrderCruddy" TableName="Orders" TableColumns="Id, CustomerName, ProductName, Qty, Paid, OrderDate" Select=@sql ShowColumnSelector="true" DbConnection="dbConnection" />

@code {
    string sql = @"
        SELECT o.Id, c.Name AS CustomerName, p.Name AS ProductName, Pid, o.Qty, o.Paid, o.OrderDate FROM Orders o
        INNER JOIN Customers c ON c.Id = o.Cid
        INNER JOIN Products p ON p.Id = o.PId
";
}
```

Note how attribute Key, DisplayName and DisplayFormat are used in the model classes.

## Database related parameters - CruddyBase.cs:

| Parameter | Description |
|-----------|-------------|
| DefaultSchema | The default schema for the database table (default: "dbo"). |
| TableName | Name of the database table to operate on. If not specified the TEntity class name is used. |
| PluralizeTableName | Whether to pluralize TableName when building SQL (adds an 's'). |
| TableColumns | The columns to retrieve from the table, separated by commas (default: "*"). |
| KeyColumn | The primary key column name. If not specified the component will try to infer it from attributes or common naming (Id / {Class}Id). |
| AllowKeyColumnEdit | Whether to allow editing of the key column in the grid when creating rows (not when updating). Default is false. |
| Top | Maximum number of rows to retrieve (default: 10000). |
| Distinct | Whether to retrieve only distinct rows. |
| OrderBy | Columns to order the result by (comma separated). Works together with SortOrder. |
| Where | SQL WHERE clause (without the "WHERE" keyword) to filter results. |
| SortOrder | Sorting order for OrderBy (Ascending / Descending). |
| Select | If provided, this full SQL SELECT statement will be used instead of the generated one. |
| DbConnection | A required ADO.NET DbConnection (e.g. SqlConnection) used by Dapper to query and modify the database. |

## UI and formatting related parameters - Cruddy.razor.cs:

| Parameter | Description |
|-----------|-------------|
| DefaultDateFormat | Default date format used when no DisplayFormatAttribute is present (default: "dd-MM-yyyy"). |
| DefaultDecimalFormat | Default decimal/double/float format when no DisplayFormatAttribute is present (default: "0.00"). |
| DefaultNumberFormat | Default integer number format when no DisplayFormatAttribute is present (default: "0"). |
| ActionsTitle | Title to use for the actions column (default: "Actions"). |
| AllowDetails | Enable per-row Details button that opens a modal showing all TEntity fields. |
| AllowEdit | Enable per-row edit button and edit functionality. |
| AllowDelete | Enable per-row delete button and delete behaviour. |
| AllowCrud | Same as setting AllowDetails, AllowEdit and AllowDelete to true |
| NameUx | The property name used as the human-readable item label in UX prompts (e.g. in delete confirmation messages). |

## Requirements

- .NET 10
- Bootstrap 5
- QuickGrid.Toolkit (is included in the beta)

## Get Started

- Create database CruddyDB
- Run script CruddyDB test database.sql
- Compile and run

## Known issues
- It's an early beta! Needs beautification...
- Class as property in a model class not supported (yet) (maybe partly supported in version 1)
- List and collection properties in a model class not supported (no plans for that to version 1)
