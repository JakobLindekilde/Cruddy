# Copilot instructions for Cruddy

These repository instructions guide GitHub Copilot (and other AI assistants) when working in Cruddy component library.

## Project overview

`Cruddy` is a simple ASP.NET Blazor component that does database table CRUD.

The solution (`CruddyDemo.slnx`) contains three projects:

- `CruddyDemo` — The Cruddy component library.
- `CruddyDemo.Tests` — A unit test project for the component library, using xunit.v3 and `Microsoft.Testing.Platform` runner.
- `CruddyDemo` — A Blazor Web App project that uses the `Cruddy` component to demonstrate its functionality.

## Target framework & dependencies

- **.NET 10** for all projects.
- Library depends on `Microsoft.AspNetCore.Components.QuickGrid` and `Microsoft.AspNetCore.Components.Web`. It also uses `Dapper`.
- Frontend: Blazor with minimal JS interop. Bootstrap 5 + Bootstrap Icons are expected in the host app.

## Coding style & conventions

- Prefer modern C#: file-scoped namespaces, pattern matching, collection expressions (`[]`), primary constructors where suitable.
- Follow existing code style.
- One public type per file; avoid `#region`; write self-explanatory code.
- Async-first: `async`/`await`; pass `CancellationToken` for I/O; avoid sync-over-async.
- Components: `.razor` markup with code-behind `.razor.cs` partial classes for anything non-trivial.
- Use QuickGrid for data grids; do not introduce other grid libraries. Keep JS interop minimal and prefer native Blazor/.NET patterns.
- Prefer constructor injection; register new services via `IServiceCollection` (use extension methods for cross-cutting concerns). 

## Best practices

- Avoid premature abstraction; wait for proven duplication.
- Favor explicitness over reflection-heavy patterns unless already established.

## Change scope (important)

When asked for a refactor or improvement:

- Help implement small, focused changes with minimal surface area.
- If you spot minor related cleanups nearby, list them as suggestions with rationale and effort — do not apply them unasked.
- Only proceed with broader refactors after explicit confirmation.
- Keep unrelated formatting or XML-doc churn out of focused changes.
- If a fix reduces risk (obvious bug, disposal, async correctness), flag its severity clearly.

## Maintenance

Keep this document accurate. Update it when the solution structure changes, when target frameworks shift, or when new patterns are adopted; remove guidance that no longer matches the code.
