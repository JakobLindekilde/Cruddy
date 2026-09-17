# Cruddy - BETA

- Cruddy is a ASP.NET Blazor componet that does CRUD operations on any table in any database.
- Cruddy is based on the Microsoft QuickGrid component
- Cruddy only requires a model class
- Cruddy uses Dapper - not Entity Framework.
- Cruddy uses ColumnManager from QuickGrid.Toolkit

## QuickGrid.Toolkit

**▶ Live demo: <https://vaclavelias.github.io/QuickGrid.Toolkit/>**

## Features
TODO

## Requirements

- .NET 10
- Bootstrap 5
- QuickGrid.Toolkit (is included in the beta)

## Examples

Get started:
- Create database CruddyDB
- Run script CruddyDB test database.sql
- Compile and run

Note how attribute Key, DisplayName and DisplayFormat are used in the model classes.

## Known issues
- It's an early beta! Needs beautification...
- Class as property in model class not supported (yet)
- Current version can not Create or Edit
