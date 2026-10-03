using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TalukdarSales.Web.Context;
using TalukdarSales.Web.Security;
using TalukdarSales.Web.Services;
using Xunit;
using Xunit.Abstractions;

namespace TalukdarSales.Tests
{
    // Guards against queries that only work on the in-memory/SQLite providers: the services run on the SQL Server
    // provider against an unreachable server. EF translates a query to SQL before it connects, so an untranslatable
    // expression throws InvalidOperationException ("could not be translated") while a healthy query fails later with a
    // connection error. Each service entry point is exercised (its first query is the one that is translated).
    public class SqlServerFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(s =>
            {
                s.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                s.RemoveAll<ApplicationDbContext>();
                s.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer("Server=127.0.0.1,1;Database=x;User Id=a;Password=b;Connect Timeout=1;Encrypt=false"));
            });
        }
    }

    public class SqlServerTranslationTests : IClassFixture<SqlServerFactory>
    {
        private readonly SqlServerFactory _f; private readonly ITestOutputHelper _o;
        public SqlServerTranslationTests(SqlServerFactory f, ITestOutputHelper o) { _f = f; _o = o; }

        private void Check(string name, Action<IServiceProvider> run)
        {
            using var scope = _f.Services.CreateScope();
            try { run(scope.ServiceProvider); throw new Xunit.Sdk.XunitException($"{name}: expected a connection error"); }
            catch (Xunit.Sdk.XunitException) { throw; }
            catch (Exception ex)
            {
                var all = new List<Exception>(); for (var e = ex; e != null; e = e.InnerException) all.Add(e);
                var translation = all.FirstOrDefault(e => e.Message.Contains("could not be translated"));
                if (translation != null)
                    throw new Xunit.Sdk.XunitException($"{name} cannot be translated to SQL: {translation.Message.Split('\n')[0]}");
            }
        }

        [Fact]
        public void The_check_itself_detects_untranslatable_queries()
        {
            var ex = Assert.Throws<Xunit.Sdk.XunitException>(() => Check("bad", sp => sp.GetRequiredService<ApplicationDbContext>().ApplicationRoles
                .Where(r => string.Equals(r.Name, "x", StringComparison.OrdinalIgnoreCase)).ToList()));
            Assert.Contains("cannot be translated", ex.Message);
        }

        [Fact]
        public void Queries_translate_on_sql_server()
        {
            var d = new DateTime(2026, 1, 1);
            Check("users search", sp => sp.GetRequiredService<UserService>().Search(1, "ann", 1, 12));
            Check("users search (no filter)", sp => sp.GetRequiredService<UserService>().Search(null, null, 2, 12));
            Check("role names", sp => sp.GetRequiredService<UserService>().RoleNames(new[] { 1, 2 }));
            Check("requisition page", sp => sp.GetRequiredService<RequisitionService>().Page(true, null, 1, "req", d, d, 1, 25));
            Check("requisition page by user", sp => sp.GetRequiredService<RequisitionService>().Page(false, 3, null, null, null, null, 1, 25));
            Check("requisition list", sp => sp.GetRequiredService<RequisitionService>().List(true, 3, "x", d, d));
            Check("requisition get", sp => sp.GetRequiredService<RequisitionService>().Get(1));
            Check("production totals", sp => sp.GetRequiredService<RequisitionService>().ProductTotalsForDay(d));
            Check("invoice list", sp => sp.GetRequiredService<InvoiceService>().List(2, d, d));
            Check("invoice list all", sp => sp.GetRequiredService<InvoiceService>().List(null, null, null));
            Check("invoice get", sp => sp.GetRequiredService<InvoiceService>().Get(1));
            Check("outstanding", sp => sp.GetRequiredService<InvoiceService>().OutstandingFor(1));
            Check("collect", sp => sp.GetRequiredService<InvoiceService>().Collect(1, 10, "Cash"));
            Check("collections", sp => sp.GetRequiredService<InvoiceService>().CollectionHistory(1, 2, d, d));
            Check("approve", sp => sp.GetRequiredService<InvoiceService>().CreateFromRequisitions(new[] { 1, 2 }));
            Check("top sellers", sp => sp.GetRequiredService<ReportService>().TopSellers(null, null));
            Check("top sellers range", sp => sp.GetRequiredService<ReportService>().TopSellers(d, d));
            Check("best products", sp => sp.GetRequiredService<ReportService>().Products(null, null, true));
            Check("worst products", sp => sp.GetRequiredService<ReportService>().Products(d, d, false));
            Check("top due", sp => sp.GetRequiredService<ReportService>().TopDue(null, null));
            Check("daily sales", sp => sp.GetRequiredService<ReportService>().DailySales(null, null));
            Check("daily orders", sp => sp.GetRequiredService<ReportService>().DailyOrders(d, d));
            Check("access", sp => sp.GetRequiredService<AccessService>().For(1));
            Check("admin ids", sp => sp.GetRequiredService<AccessService>().AdministratorUserIds());
            Check("seeder", sp => sp.GetRequiredService<AccessSeeder>().Run());
            Check("create user (next number)", sp => sp.GetRequiredService<UserService>().CreateAsync(new TalukdarSales.Web.Models.Dto.CreateUserDto { FirstName = "a", PhoneNumber = "1" }).GetAwaiter().GetResult());
            Check("assign role", sp => sp.GetRequiredService<UserService>().AssignRole(1, 1, new UserAccess(true, true, new HashSet<string>())));
            Check("requisition create", sp => sp.GetRequiredService<RequisitionService>().Create(1, new[] { new RequisitionLine(1, 2) }));
        }
    }
}
