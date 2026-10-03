# Talukdar Sales

A single ASP.NET Core (net6.0) project, `TalukdarSales.Web`, serving the whole application:

- **UI:** Razor Pages + [htmx](https://htmx.org) + Bootstrap 5, server-rendered. Both libraries are vendored in `wwwroot/lib`, so there is no Node.js, npm or CDN at build or run time.
- **Services:** `Services/` holds the business logic (requisitions, invoices, collections, reports, users) used by the pages.
- **JSON API:** the controllers under `Controllers/` (JWT bearer, `/api/*`) are kept for external clients. The pages do not use them.

Sign-in for the pages uses a cookie; `/api/*` accepts only the JWT bearer token.

## Configuration
| Key | Purpose |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server |
| `Jwt:Key` | Required, at least 32 characters (env var `Jwt__Key`). Only used by the JSON API |
| `Cors:AllowedOrigins` | Only needed if a browser app on another origin calls the JSON API |

## Develop
    dotnet run --project TalukdarSales.Web        # https://localhost:7019
    dotnet test tests/TalukdarSales.Tests          # runs the app in-process on an in-memory database

## Publish to IIS
Needs the .NET SDK on the build machine and the ASP.NET Core Hosting Bundle (net6) on the server. No Node.js.

    dotnet publish TalukdarSales.Web -c Release -o publish

Point an IIS site at `publish`. Keep `wwwroot/images` and `wwwroot/pdf` (uploaded files) between deployments, and set the connection string and `Jwt__Key` on the server.

## Layout
    TalukdarSales.Web/
      Pages/            Razor Pages (one folder per screen; partials are the htmx fragments)
      Services/         business logic
      Infrastructure/   page helpers (toasts, paging, Excel export)
      Controllers/      JSON API
      wwwroot/          css, js, lib (htmx, bootstrap), img, uploads
    tests/TalukdarSales.Tests/
