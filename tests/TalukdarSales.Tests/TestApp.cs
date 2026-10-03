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
        private readonly string _dbName = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, cfg) => cfg.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Jwt:Key"] = "test-key-test-key-test-key-test-key-1234"
            }));
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<ApplicationDbContext>();
                services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(_dbName));
            });
        }

        public void Seed(Action<ApplicationDbContext> action)
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            action(db);
            db.SaveChanges();
        }

        public int SeedUser(string username = "admin", string password = "secret123", string first = "Ada")
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
            });
            return id;
        }

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
