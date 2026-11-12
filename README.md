# Cruddy

Why Cruddy? -I needed a simple component that can do CRUD operations on any database table.
I wanted to be able to use it with any class or record. It had to use Dapper for database operations. 

NOTE: See also the twin component CruddyModel that uses a model class: 
https://github.com/JakobLindekilde/Cruddy

## Overview

- Cruddy is an ASP.NET Blazor component that does CRUD operations on any table in any database
- Cruddy only requires a table name and a DbConnection (e.g. SqlConnection) to work with
- Cruddy is based on the Microsoft QuickGrid component
- When editing or creating validation is done table columns properties
- Cruddy uses Dapper - not Entity Framework
- Cruddy will generate the appropriate CRUD SQL statements
- Cruddy does not use any non-Microsoft NuGet packages (except for Dapper)
- Foreign keys are handled by Cruddy - See the FKeys parameter in the table below

## How to contribute

You can contribute to Cruddy by creating a pull request on GitHub.
Please follow normal/typical coding standards and include tests for changes and new features.

Please run the tests locally: You need to set up a local database with the provided SQL script. 
Else the database tests will not be run. I have not set up a test database in GitHub Actions yet.

## Roadmap for version 1

- Date for version 1: Before end of 2026
- Get stable and usable for production
- Make final NuGet package (beta prerelease is available on nuget.org now)

## Examples 
Note: There are more examples in the CruddyDemo project.

### ConnectionString in appsettings.json:
```csharp
{
  "ConnectionStrings": {
    "CruddyDB": "Server=.;Database=CruddyDB;MultipleActiveResultSets=True;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Cruddy": {
    "DefaultDateFormat": "d",
    "DefaultDateTimeFormat": "g",
    "DefaultTimeFormat": "t",
    "DefaultDecimalFormat": "0.00",
    "DefaultNumberFormat": "0",
    "ActionsTitle": "Actions",
    "CruddyGridTheme": "twentyAI",
    "CruddyGridClass": "table table-sm table-index table-striped small table-fit table-thead-sticky mb-0",
    "ModalTableClass": "table table-sm table-striped"
  },
  ...
}
```

### First add SqlConnection in Program.cs:
```csharp
public static void Main(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSingleton<IRepositoryFactory, DapperRepositoryFactory>();
    builder.Services.AddScoped(serviceProvider =>
    {
        return new SqlConnection(builder.Configuration.GetConnectionString("CruddyDB"));
    });
    ...  
```

The factory is stateless; the component still uses the `DbConnection` passed to it. To use a different repository implementation, register your own `IRepositoryFactory` instead.

### Typical use - file Customers.razor:

```razor
@page "/Customers"
@inject Microsoft.Data.SqlClient.SqlConnection dbConnection

<h3>Table Customers</h3>

<h4>TableColumns="*" HideKeyColumn="true"</h4>
<Cruddy TableName="Customers" AllowCrud="true" ReloadPageAfterChange="true" DbConnection="dbConnection" HideKeyColumn="true" />

@code {
}
```

The razor file above will result in this QuickGrid:
<img src="Docs/TableCustomersCruddy.png" alt="Cruddy example" width="800" />

**Key**: The KeyColumn parameter is used to specify the primary key column name (default: "Id").

<img src="Docs/TableCustomers.png" alt="Cruddy example" width="400" />
<img src="Docs/Details.png" alt="Cruddy Details" width="800" />
<img src="Docs/Edit.png" alt="Cruddy Edit" width="800" />
<img src="Docs/Create.png" alt="Cruddy Create" width="800" />
<img src="Docs/Delete.png" alt="Cruddy Delete" width="800" />

### Most simple use (no CRUD):
```razor
@page "/Products"
@inject Microsoft.Data.SqlClient.SqlConnection dbConnection

<h3>Table Products</h3>

<h4>Simple as possible</h4>
<Cruddy TableName="Products" DbConnection="dbConnection" AllowDetails="true" />

@code {
}
```

### Foreign keys example - file Orders.razor:

```razor
@page "/Orders"
@inject Microsoft.Data.SqlClient.SqlConnection dbConnection

<h3>Table Orders - with foreign keys</h3>
<Cruddy TableName="Orders" TableColumns="Id, CId AS Customer, PId AS Product, EId AS Employee, Qty, Paid, OrderDate" AllowCrud="true" DbConnection="dbConnection"
        FKeys="Customer: Customers, Id, Name; Product: Products, Id, Name; Employee: Employees, Id, Name" />
 
<br />
<h3>Table Orders - without foreign keys</h3>
<Cruddy TableName="Orders" AllowCrud="true" DbConnection="dbConnection" />

@code {
}
```

## Database related parameters - CruddyBase.cs:

| Parameter | Description |
|-----------|-------------|
| DefaultSchema | The default schema for the database table (default: "dbo"). |
| TableName | Name of the database table to operate on. If not specified the TEntity class name is used. |
| PluralizeTableName | Whether to pluralize TableName when building SQL (adds an 's'). |
| TableColumns | The columns to retrieve from the table, separated by commas (default: "*"). |
| KeyColumn | The primary key column name. If not specified the component will try to infer it from attributes or common naming (Id / {Class}Id). |
| AllowKeyColumnEditOnCreate | Whether to allow editing of the key column when creating rows (not when updating). Default is false. |
| Top | Maximum number of rows to retrieve (default: 10000). |
| Distinct | Whether to retrieve only distinct rows. |
| OrderBy | Columns to order the result by (comma separated). Works together with SortOrder. |
| Where | SQL WHERE clause (without the "WHERE" keyword) to filter results. |
| SortOrder | Sorting order for OrderBy (Ascending / Descending). |
| Select | If provided, this full SQL SELECT statement will be used instead of the generated one. |
| DbConnection | An ADO.NET DbConnection (e.g. SqlConnection) used by Dapper to query and modify the database. |
| FKeys | Foreign key info in format "FKey: PrimaryTable, PrimaryId, PrimaryDisplay; FKey2: ...". Or just the foreign key column name(s) and Cruddy will try to guess the rest. |

## UI and formatting related parameters - Cruddy.razor.cs:

Note: All parameters below can be set in appsettings.json file. 
The value in appsettings.json will be used as default, but can be overridden by setting the parameter in the component.
If the parameter is not set in appsettings.json or in the component, the default value will be used.

| Parameter | Description |
|-----------|-------------|
| DefaultDateFormat | Default date format used when no DisplayFormatAttribute is present. Default: d (aka 'Short date pattern'). |
| DefaultDateTimeFormat | Default date and time format used when no DisplayFormatAttribute is present. Default: g (aka 'General date/time pattern'). |
| DefaultTimeFormat | Default time format used when no DisplayFormatAttribute is present. Default: t (aka 'Short time pattern'). |
| DefaultDecimalFormat | Default decimal/double/float format when no DisplayFormatAttribute is present. Default format is 0.00. |
| DefaultNumberFormat | Default integer number format when no DisplayFormatAttribute is present. Default format is 0. |
| ActionsTitle | Title to use for the actions column (default: "Actions"). |
| AllowCreate | Enable the create functionality. |
| AllowDetails | Enable per-row Details button that opens a modal showing all TEntity fields. |
| AllowEdit | Enable per-row edit button and edit functionality. |
| AllowDelete | Enable per-row delete button and delete behaviour. |
| AllowCrud | Same as setting AllowCreate, AllowDetails, AllowEdit and AllowDelete to true |
| DetailsColumns | List of table columns, separated by commas, to display in the Details modal. If "*" is specified, all public readable properties of TEntity will be displayed. If empty string TableColumns is used. |
| HideKeyColumn | Whether to hide the key column in the table. |
| ReloadPageAfterChange | Whether to update the whole page after a change (create, edit, delete) is made, or just the component. If true, the page will be refreshed after a change is made. If false, the in-memory list of rows will be updated and the UI will be refreshed without reloading the page. |
| NameUx | The property name used as the human-readable item label in UX prompts (e.g. in delete confirmation messages). |
| CruddyGridTheme | Theme applied to the QuickGrid. Default is "twentyAI". |
| CruddyGridClass | CSS class applied to the QuickGrid. Default is "table table-sm table-index table-striped small table-fit table-thead-sticky mb-0". |
| ModalTableClass | CSS class applied to the modal table. Default is "table table-sm table-striped". |

## Requirements

- .NET 10
- Bootstrap 5

## Get Started

- Create database CruddyDB
- Run script CruddyDB test database.sql
- Compile and run CruddyDemo project

## Known issues
- Only tested with MS SQL Server - but should work with any database that Dapper supports
- Method DapperRepository.Add() will need more work for other databases than MS SQL Server
- Cruddy does not support composite primary keys (no plans for this in version 1)
