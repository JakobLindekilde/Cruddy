# Cruddy

## About
A simple ASP.NET Blazor component that can do CRUD operations 
on any database table without the need for a model class.

## Key Features
- Cruddy only requires a table name and a DbConnection (e.g. SqlConnection) to work with
- Cruddy uses Dapper - not Entity Framework

## How to Use
See CruddyDemo project for examples of how to use the Cruddy component:
https://github.com/JakobLindekilde/Cruddy

## CruddyModel - The version that uses a model class or record
See CruddyModelDemo project for examples of how to use the CruddyModel component:
https://github.com/JakobLindekilde/CruddyModel

## Main Types
The main types provided by this library are:
- `Lindekilde.Blazor.Cruddy.Components`
- `Lindekilde.Blazor.Cruddy.Helpers`
- `Lindekilde.Blazor.Cruddy.Repositories`