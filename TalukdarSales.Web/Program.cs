using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Project.Run.Repositories;
using System.Text;
using TalukdarSales.Web.Context;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Repositories;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
    throw new InvalidOperationException("Configuration value 'Jwt:Key' is required and must be at least 32 characters.");

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:4200" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyPolicy",
        //builder => builder.WithOrigins("http://localhost:4200", "https://localhost:4200")
        builder => builder.WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        //.WithHeaders(HeaderNames.ContentType,"Access-Control-Allow-Origin")
        );
});
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

// Pages use a cookie; /api/* keeps JWT bearer. The policy scheme picks one per request so a
// browser cookie is never accepted by the API (no CSRF exposure there).
const string SmartScheme = "Smart";
builder.Services.AddAuthentication(x =>
{
    x.DefaultScheme = SmartScheme;
    x.DefaultAuthenticateScheme = SmartScheme;
    x.DefaultChallengeScheme = SmartScheme;
})
.AddPolicyScheme(SmartScheme, SmartScheme, o =>
{
    o.ForwardDefaultSelector = ctx =>
        ctx.Request.Path.StartsWithSegments("/api")
            ? JwtBearerDefaults.AuthenticationScheme
            : CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
{
    o.Cookie.Name = "TSA.Auth";
    o.Cookie.HttpOnly = true;
    o.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    o.ExpireTimeSpan = TimeSpan.FromHours(8);
    o.SlidingExpiration = true;
    o.LoginPath = "/Login";
    o.LogoutPath = "/Logout";
    o.AccessDeniedPath = "/Login";
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
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateAudience = false,
        ValidateIssuer = false,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAntiforgery(o => o.HeaderName = "RequestVerificationToken");
builder.Services
    .AddRazorPages(o =>
    {
        o.Conventions.AuthorizeFolder("/");
        o.Conventions.AllowAnonymousToPage("/Login");
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.UseStaticFiles(); // uploads, fonts, generated PDFs

// Angular build output (ClientApp/dist/talukdar-sales-ui), served from the same origin as the API
var clientAppPath = Path.Combine(builder.Environment.ContentRootPath, "ClientApp", "dist", "talukdar-sales-ui");
var clientAppFiles = Directory.Exists(clientAppPath) ? new PhysicalFileProvider(clientAppPath) : null;
if (clientAppFiles != null)
    app.UseStaticFiles(new StaticFileOptions { FileProvider = clientAppFiles });


app.UseCors("MyPolicy");


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

// SPA fallback: deep links such as /dashboard/invoice-list serve index.html; unknown /api/* paths stay 404
if (clientAppFiles != null)
    app.MapFallbackToFile("{*path:regex(^(?!api/).*$)}", "index.html", new StaticFileOptions { FileProvider = clientAppFiles });

app.Run();

public partial class Program { }
