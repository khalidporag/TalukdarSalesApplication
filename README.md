# Talukdar Sales

A single ASP.NET Core (net6.0) project, `TalukdarSales.Web`, serving the whole application:

- **UI:** Razor Pages + [htmx](https://htmx.org), server-rendered, with a hand-written design system in `wwwroot/css/app.css` (no CSS framework). htmx is vendored in `wwwroot/lib` and fonts in `wwwroot/fonts`, so there is no Node.js, npm or CDN at build or run time.
- **Services:** `Services/` holds the business logic (requisitions, invoices, collections, KPI analytics, users) used by the pages.

Sign-in uses a cookie. There is no separate JSON API.

## Configuration
| Key | Purpose |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server |
| `Kpi:MonthlySalesTarget` | Sales target per month for the KPI report gauge and target bars (default 1500000) |
| `Kpi:CollectionRateTarget` | Target collected-as-%-of-billed (default 85) |
| `Kpi:DailyOrdersTarget` | Optional orders-per-day target; 0 hides the target on the Orders KPI (default 0) |

## Design system
Colours and type come from the Talukder Foods logo: red `#D7261F` (one primary action per screen), maroon `#7F1628` for headings, plum `#4A0A6B` for the navigation, butter yellow `#FBEA8F` for highlights. Rubik for headings and figures, Figtree for text, Noto Sans Bengali for the taka sign. Charts use four validated series colours (`#E5412D`, `#7B4BB8`, `#C98A12`, `#1F9E96`), are drawn as server-rendered SVG (`Infrastructure/Charts.cs`, `Pages/Shared/_LineChart.cshtml`) with a small hover script in `wwwroot/js/app.js`, and every chart has its numbers in a table or label. Amounts use Indian digit grouping (`Fmt.Money`). Controls are at least 44px tall for phone use.

Main flows: **New order** (`/Requisitions/Create`, live cart in `wwwroot/js/order.js`, credit warning, no page reloads) then **Orders to invoice** (`/Requisitions`: tick and bulk-invoice, one-tap invoice, or open the side panel to adjust quantities) then **Invoices** (collect panel with allocation preview, print layout) and **Collections**. The **Dashboard** is the bird's-eye view (KPIs, trend, attention list, ageing, categories, production, notices) and **KPI report** adds targets, previous-period comparison, weekly charts and Excel export.

## Access control
- Permissions are a fixed list in `Security/Permissions.cs` (view/manage a screen, plus separate approve and collect actions). At startup they are synced into the `ApplicationModules` table; roles are granted permissions on **Roles** and users get a role on **Customers**.
- **Deny by default:** `PermissionPageFilter` checks every page request (and the named handler, e.g. `?handler=Collect`) against the rule table before the page runs. A page without a rule is admin-only; a test fails if a new page is added without one.
- The **Administrator** role is built in and always has full access. On the first start with access control enabled, if nobody is an administrator, the user named in `Security:InitialAdmin` (or else the oldest user) is promoted and a warning is logged. Review it on the Roles page.
- Permissions are read from the database on every request, so role changes and deleted users take effect immediately without signing in again.
- Guards: only administrators can grant or remove the Administrator role; the last administrator cannot be demoted; a non-administrator can only change permissions they hold themselves.
- To add a permission: add it to `Perm.Catalog`, add a rule in `Perm.Rules`, and (optionally) hide the link in `_Layout.cshtml`.

## Develop
    dotnet run --project TalukdarSales.Web        # https://localhost:7019
    dotnet test tests/TalukdarSales.Tests          # runs the app in-process on an in-memory SQLite database

## Publish to IIS
Needs the .NET SDK on the build machine and the ASP.NET Core Hosting Bundle (net6) on the server. No Node.js.

    dotnet publish TalukdarSales.Web -c Release -o publish

Point an IIS site at `publish`. Keep `wwwroot/images` and `wwwroot/pdf` (uploaded files) between deployments, and set the connection string on the server.

## Data access
Repositories return `IQueryable<T>` (`GetAll()` = not-deleted rows, `FindBy(predicate)`), so filtering, grouping, counting and paging run in SQL. Rules of thumb: compose the query first and materialise last (`ToList()`, `Paged<T>.Create(query, page, size)`); use `ToLower().Contains(...)` for case-insensitive search (not `StringComparison`); look names up with `ids.Contains(x.Id)` instead of loading whole tables.
Tests run on SQLite and `SqlServerTranslationTests` runs every service entry point on the SQL Server provider (against an unreachable server: EF translates before it connects), so a query EF cannot translate fails the build, not production.

## Money
All money (prices, invoice totals, collections, customer balance and credit limit) is `double`. `Helpers/Money.Round` rounds to 2 decimals at every write so floating-point noise never reaches a balance; keep using it for any new money arithmetic.

## Database changes
Migrations are not applied automatically. After deploying, run `dotnet ef database update` (or generate a script with `dotnet ef migrations script`) against the production database; back it up first.

## Layout
    TalukdarSales.Web/
      Pages/            Razor Pages (one folder per screen; partials are the htmx fragments and side panels)
      Services/         business logic
      Infrastructure/   page helpers (toasts, paging, Excel export)
      wwwroot/          css, js, fonts, lib (htmx), img, uploads
    tests/TalukdarSales.Tests/
