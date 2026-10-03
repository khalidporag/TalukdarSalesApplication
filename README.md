# Talukdar Sales

A single ASP.NET Core (net6.0) project, `TalukdarSales.Web`, serving the whole application:

- **UI:** Razor Pages + [htmx](https://htmx.org) + Bootstrap 5, server-rendered. Both libraries are vendored in `wwwroot/lib`, so there is no Node.js, npm or CDN at build or run time.
- **Services:** `Services/` holds the business logic (requisitions, invoices, collections, reports, users) used by the pages.

Sign-in uses a cookie. There is no separate JSON API.

## Configuration
| Key | Purpose |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server |

## Access control
- Permissions are a fixed list in `Security/Permissions.cs` (view/manage a screen, plus separate approve and collect actions). At startup they are synced into the `ApplicationModules` table; roles are granted permissions on **Role Management** and users get a role on **User Management**.
- **Deny by default:** `PermissionPageFilter` checks every page request (and the named handler, e.g. `?handler=Collect`) against the rule table before the page runs. A page without a rule is admin-only; a test fails if a new page is added without one.
- The **Administrator** role is built in and always has full access. On the first start with access control enabled, if nobody is an administrator, the user named in `Security:InitialAdmin` (or else the oldest user) is promoted and a warning is logged. Review it in Role Management.
- Permissions are read from the database on every request, so role changes and deleted users take effect immediately without signing in again.
- Guards: only administrators can grant or remove the Administrator role; the last administrator cannot be demoted; a non-administrator can only change permissions they hold themselves.
- To add a permission: add it to `Perm.Catalog`, add a rule in `Perm.Rules`, and (optionally) hide the link in `_Layout.cshtml`.

## Develop
    dotnet run --project TalukdarSales.Web        # https://localhost:7019
    dotnet test tests/TalukdarSales.Tests          # runs the app in-process on an in-memory database

## Publish to IIS
Needs the .NET SDK on the build machine and the ASP.NET Core Hosting Bundle (net6) on the server. No Node.js.

    dotnet publish TalukdarSales.Web -c Release -o publish

Point an IIS site at `publish`. Keep `wwwroot/images` and `wwwroot/pdf` (uploaded files) between deployments, and set the connection string on the server.

## Layout
    TalukdarSales.Web/
      Pages/            Razor Pages (one folder per screen; partials are the htmx fragments)
      Services/         business logic
      Infrastructure/   page helpers (toasts, paging, Excel export)
      wwwroot/          css, js, lib (htmx, bootstrap), img, uploads
    tests/TalukdarSales.Tests/
