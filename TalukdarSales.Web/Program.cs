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

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
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

// SPA fallback: deep links such as /dashboard/invoice-list serve index.html; unknown /api/* paths stay 404
if (clientAppFiles != null)
    app.MapFallbackToFile("{*path:regex(^(?!api/).*$)}", "index.html", new StaticFileOptions { FileProvider = clientAppFiles });

app.Run();
