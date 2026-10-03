# Talukdar Sales

One ASP.NET Core (net6.0) project, `TalukdarSales.Web`, that hosts the API and the Angular app (`ClientApp/`).

## Configuration
- `ConnectionStrings:DefaultConnection` – SQL Server
- `Jwt:Key` – required, at least 32 characters (set as env var `Jwt__Key` in production)
- `Cors:AllowedOrigins` – only needed for local development

## Local development
1. Run the API: `dotnet run --project TalukdarSales.Web` (https://localhost:7019)
2. Run the UI: `cd TalukdarSales.Web/ClientApp && npm ci && npm start` (http://localhost:4200, proxies `/api`, `/images`, `/pdf` to the API via `proxy.conf.json`)

## Publish to IIS
Requires the .NET SDK and Node.js on the build machine, and the ASP.NET Core Hosting Bundle (net6) on the IIS server.

    dotnet publish TalukdarSales.Web -c Release -o publish

The publish step runs `npm ci` and `ng build --configuration production` and copies the result to `publish/ClientApp/dist`.
Pass `-p:SkipClientBuild=true` to skip the Angular build. Point an IIS site at the `publish` folder; keep `wwwroot/images` and `wwwroot/pdf` (uploads) between deployments.
