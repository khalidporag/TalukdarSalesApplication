using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TalukdarSales.Web.Context;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserTypeRepository, UserTypeRepository>();
builder.Services.AddScoped<IApplicationRoleRepository, ApplicationRoleRepository>();
builder.Services.AddScoped<IApplicationModuleRepository, ApplicationModuleRepository>();
builder.Services.AddScoped<IRoleWisePermissionRepository, RoleWisePermissionRepository>();
builder.Services.AddScoped<IUserRoleMappingRepository, UserRoleMappingRepository>();
builder.Services.AddScoped<IFinishedGoodTypeRepository, FinishedGoodTypeRepository>();
builder.Services.AddScoped<IFinishedGoodsRepository, FinishedGoodsRepository>();
builder.Services.AddScoped<ISalesRequisitionRepository, SalesRequisitionRepository>();
builder.Services.AddScoped<ISalesRequisitionDetailRepository, SalesRequisitionDetailRepository>();
builder.Services.AddScoped<ICollectionLedgerRepository, CollectionLedgerRepository>();
builder.Services.AddScoped<ISalesInvoiceRepository, SalesInvoiceRepository>();
builder.Services.AddScoped<ISalesInvoiceDetailsRepository, SalesInvoiceDetailsRepository>();
builder.Services.AddScoped<ITimeSettingRepository, TimeSettingRepository>();
builder.Services.AddScoped<INoticeRepository, NoticeRepository>();
builder.Services.AddScoped<TalukdarSales.Web.Services.UserService>();
builder.Services.AddScoped<TalukdarSales.Web.Services.ImageStore>();
builder.Services.AddScoped<TalukdarSales.Web.Services.RequisitionService>();
builder.Services.AddScoped<TalukdarSales.Web.Services.InvoiceService>();
builder.Services.AddScoped<TalukdarSales.Web.Services.ReportService>();
builder.Services.AddScoped<TalukdarSales.Web.Services.AnalyticsService>();
builder.Services.AddScoped<TalukdarSales.Web.Security.AccessService>();
builder.Services.AddScoped<TalukdarSales.Web.Security.AccessSeeder>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.Cookie.Name = "TSA.Auth";
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
        o.SlidingExpiration = true;
        o.LoginPath = "/Login";
        o.LogoutPath = "/Logout";
        o.AccessDeniedPath = "/AccessDenied";
        o.Events.OnRedirectToLogin = ctx =>
        {
            // htmx requests: ask the browser to do a full-page redirect instead of swapping the login page into a fragment
            if (ctx.Request.Headers.ContainsKey("HX-Request"))
            {
                ctx.Response.Headers["HX-Redirect"] = ctx.RedirectUri;
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            }
            else
                ctx.Response.Redirect(ctx.RedirectUri);
            return Task.CompletedTask;
        };
    });

builder.Services.AddAntiforgery(o => o.HeaderName = "RequestVerificationToken");
builder.Services
    .AddRazorPages(o =>
    {
        o.Conventions.AuthorizeFolder("/");
        o.Conventions.AllowAnonymousToPage("/Login");
    })
    .AddMvcOptions(o => o.Filters.Add<TalukdarSales.Web.Security.PermissionPageFilter>());

var app = builder.Build();

// Sync the permission catalog, the Administrator role and (first start only) an initial administrator.
using (var scope = app.Services.CreateScope())
{
    try { scope.ServiceProvider.GetRequiredService<TalukdarSales.Web.Security.AccessSeeder>().Run(); }
    catch (Exception ex) { app.Logger.LogError(ex, "Access control seeding failed."); }
}

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");

app.UseHttpsRedirection();
app.UseStaticFiles(); // uploads, fonts, generated PDFs

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

public partial class Program { }
