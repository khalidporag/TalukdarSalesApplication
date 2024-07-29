using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Project.Run.Repositories;
using System.Text;
using TalukdarSalesAPI.Context;
using TalukdarSalesAPI.Interfaces;
using TalukdarSalesAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyPolicy",
        //builder => builder.WithOrigins("http://localhost:4200", "https://localhost:4200")
        builder => builder.WithOrigins("http://localhost:8001/")
        .SetIsOriginAllowed(host => true)
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("veryverysceret.....")),
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

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseStaticFiles(); // This line enables serving static files


app.UseCors("MyPolicy");


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
