using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TalukdarSales.Web.Context;
using TalukdarSales.Web.Helpers;
using TalukdarSales.Web.Models;

namespace TalukdarSales.Tests
{
    /// <summary>Runs the real app in-process against an in-memory database.</summary>
    public class TestApp : WebApplicationFactory<Program>
    {
        // A relational provider (not EF's InMemory) so queries that EF cannot translate to SQL fail in the tests too.
        private readonly Microsoft.Data.Sqlite.SqliteConnection _connection = new("DataSource=:memory:");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            _connection.Open();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options;
            using (var schema = new ApplicationDbContext(options))
                schema.Database.EnsureCreated();

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<ApplicationDbContext>();
                services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(_connection));
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _connection.Dispose();
            base.Dispose(disposing);
        }

        public void Seed(Action<ApplicationDbContext> action)
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            action(db);
            db.SaveChanges();
        }

        /// <summary>Creates a user. By default the user is an Administrator (full access); pass role to use another role instead.</summary>
        public int SeedUser(string username = "admin", string password = "secret123", string first = "Ada", string role = "Administrator")
        {
            var id = 0;
            Seed(db =>
            {
                var u = new User
                {
                    SequencialUserId = "1981-0001", Username = username, FirstName = first, LastName = "Lovelace",
                    PhoneNumber = "0100", Password = PasswordHasher.HashPassword(password), UserTypeId = 1,
                    ImageName = "", Token = "", RefreshToken = "", Address = "", ContactPersonName = "", ContactPersonPhone = "",
                    CreatedOn = DateTime.Now
                };
                db.Users.Add(u);
                db.SaveChanges();
                id = u.Id;
                if (role != null)
                {
                    var r = db.ApplicationRoles.AsEnumerable().FirstOrDefault(x => string.Equals(x.Name, role, StringComparison.OrdinalIgnoreCase));
                    if (r == null) { r = new ApplicationRole { Name = role, CreatedOn = DateTime.Now }; db.ApplicationRoles.Add(r); db.SaveChanges(); }
                    db.UserRoleMappings.Add(new UserRoleMapping { UserId = u.Id, RoleId = r.Id, CreatedOn = DateTime.Now });
                }
            });
            return id;
        }

        /// <summary>Creates (or reuses) a role and grants it the given permission keys (replacing earlier grants).</summary>
        public void SeedRole(string name, params string[] permissionKeys)
        {
            Seed(db =>
            {
                var r = db.ApplicationRoles.AsEnumerable().FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
                if (r == null) { r = new ApplicationRole { Name = name, CreatedOn = DateTime.Now }; db.ApplicationRoles.Add(r); db.SaveChanges(); }
                db.RoleWisePermissions.RemoveRange(db.RoleWisePermissions.Where(p => p.RoleId == r.Id));
                foreach (var key in permissionKeys)
                {
                    var m = db.ApplicationModules.First(x => x.Url == key);
                    db.RoleWisePermissions.Add(new RoleWisePermission { RoleId = r.Id, ModuleId = m.Id, CreatedOn = DateTime.Now });
                }
            });
        }

        public T Run<T>(Func<IServiceProvider, T> action)
        {
            using var scope = Services.CreateScope();
            return action(scope.ServiceProvider);
        }

        public async Task<T> RunAsync<T>(Func<IServiceProvider, Task<T>> action)
        {
            using var scope = Services.CreateScope();
            return await action(scope.ServiceProvider);
        }

        public int SeedGood(string name, double price, bool active = true)
        {
            var id = 0;
            Seed(db =>
            {
                var g = new FinishedGood { Name = name, UnitPrice = price, IsActive = active, UOM = "pc", Description = "", LogoName = "", CreatedOn = DateTime.Now };
                db.FinishedGoods.Add(g);
                db.SaveChanges();
                id = g.Id;
            });
            return id;
        }

        public void SeedOpenWindow(string from = "00:00", string to = "23:59") =>
            Seed(db => db.TimeSettings.Add(new TimeSetting { From = from, To = to, CreatedOn = DateTime.Now }));

        public HttpClient NewClient() => CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });

        public static async Task<string> Antiforgery(HttpClient client, string url)
        {
            var html = await client.GetStringAsync(url);
            var m = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
            if (!m.Success)
                m = Regex.Match(html, "RequestVerificationToken\": \"([^\"]+)\"");
            return m.Success ? m.Groups[1].Value : throw new InvalidOperationException("no antiforgery token on " + url);
        }

        public static async Task<HttpResponseMessage> Login(HttpClient client, string user, string pass)
        {
            var token = await Antiforgery(client, "/Login");
            return await client.PostAsync("/Login", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Username"] = user, ["Password"] = pass, ["__RequestVerificationToken"] = token
            }));
        }
    }
}

namespace TalukdarSales.Tests
{
    public static class HttpExtensions
    {
        /// <summary>POST an htmx form (multipart) with the antiforgery token scraped from <paramref name="pageUrl"/>.</summary>
        public static async Task<HttpResponseMessage> HtmxPost(this HttpClient client, string pageUrl, string handlerUrl, Dictionary<string, string> fields)
        {
            var form = new MultipartFormDataContent { { new StringContent(await TestApp.Antiforgery(client, pageUrl)), "__RequestVerificationToken" } };
            foreach (var kv in fields) form.Add(new StringContent(kv.Value), kv.Key);
            var req = new HttpRequestMessage(HttpMethod.Post, handlerUrl) { Content = form };
            req.Headers.Add("HX-Request", "true");
            return await client.SendAsync(req);
        }

        /// <summary>Plain (non-htmx) form post with the antiforgery token scraped from <paramref name="pageUrl"/>.</summary>
        public static async Task<HttpResponseMessage> FormPost(this HttpClient client, string pageUrl, string handlerUrl, Dictionary<string, string> fields)
        {
            var all = new Dictionary<string, string>(fields) { ["__RequestVerificationToken"] = await TestApp.Antiforgery(client, pageUrl) };
            return await client.PostAsync(handlerUrl, new FormUrlEncodedContent(all));
        }

        /// <summary>The toast text the next page shows after a redirect (the layout renders TempData Flash as data-flash).</summary>
        public static async Task<string> FlashAfter(this HttpClient client, HttpResponseMessage redirect)
        {
            var html = await client.GetStringAsync(redirect.Headers.Location!.OriginalString);
            var m = System.Text.RegularExpressions.Regex.Match(html, "data-flash=\"([^\"]*)\"");
            return m.Success ? System.Net.WebUtility.HtmlDecode(m.Groups[1].Value) : "";
        }

        public static string Trigger(this HttpResponseMessage res) =>
            res.Headers.TryGetValues("HX-Trigger", out var v) ? string.Join(",", v) : "";
    }
}
