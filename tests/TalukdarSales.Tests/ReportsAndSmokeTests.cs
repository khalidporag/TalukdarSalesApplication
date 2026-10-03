using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TalukdarSales.Web.Services;
using Xunit;

namespace TalukdarSales.Tests
{
    public class ReportsAndSmokeTests
    {
        private static async Task<(TestApp app, HttpClient c)> Setup()
        {
            var app = new TestApp();
            app.SeedUser();
            app.SeedOpenWindow();
            var c = app.NewClient();
            await TestApp.Login(c, "admin", "secret123");
            return (app, c);
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/Dashboard")]
        [InlineData("/Users")]
        [InlineData("/Roles")]
        [InlineData("/Requisitions")]
        [InlineData("/Requisitions/Create")]
        [InlineData("/Invoices")]
        [InlineData("/Collections")]
        [InlineData("/Production")]
        [InlineData("/Reports")]
        [InlineData("/Products")]
        [InlineData("/TimeSetting")]
        public async Task Every_menu_page_renders_when_logged_in(string url)
        {
            var (_, c) = await Setup();
            var res = await c.GetAsync(url);
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            var html = await res.Content.ReadAsStringAsync();
            Assert.Contains("Talukder Foods", html);
        }

        [Fact]
        public async Task Missing_records_return_404_not_500()
        {
            var (_, c) = await Setup();
            Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync("/Invoices/Details?id=999")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync("/Requisitions?handler=Panel&id=999")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync("/Invoices?handler=Panel&id=999")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync("/Users?editId=999")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync("/Products?editId=999")).StatusCode);
        }

        [Fact]
        public async Task Dashboard_figures_aggregate_per_customer_and_period()
        {
            var app = new TestApp();
            var u1 = app.SeedUser("a", "x", "Ann");
            app.Seed(db => db.Users.Add(new TalukdarSales.Web.Models.User { FirstName = "Bob", LastName = "", Username = "b", SequencialUserId = "1981-0002", UserTypeId = 1, PhoneNumber = "2",
                Password = "", ImageName = "", Token = "", RefreshToken = "", Address = "", ContactPersonName = "", ContactPersonPhone = "", CreatedOn = DateTime.Now }));
            var g = app.SeedGood("Soap", 10);
            app.SeedOpenWindow();

            int Make(int user, double qty)
            {
                var r = app.Run(sp => sp.GetRequiredService<RequisitionService>().Create(user, new[] { new RequisitionLine(g, qty) })).Requisition.Id;
                return app.Run(sp => sp.GetRequiredService<InvoiceService>().CreateFromRequisitions(new[] { r })).Invoices[0].Id;
            }
            Make(u1, 3);   // 30
            Make(u1, 2);   // 20  -> Ann total 50
            Make(2, 1);    // 10  -> Bob
            app.Run(sp => sp.GetRequiredService<InvoiceService>().Collect(1, 30, "Cash"));

            var d = app.Run(sp => sp.GetRequiredService<AnalyticsService>().Build(Period.Parse("30")));
            Assert.Equal(60, d.Billed);
            Assert.Equal(30, d.Collected);
            Assert.Equal(30, d.Outstanding);
            Assert.Equal(2, d.OutstandingCustomers);
            Assert.Equal(3, d.Orders);
            Assert.Equal(2, d.ActiveCustomers);
            Assert.Equal("Ann Lovelace", d.TopCustomers[0].Name);
            Assert.Equal(50, d.TopCustomers[0].Value);          // two invoices summed per customer
            Assert.Equal(6, d.Products.Single().Quantity);
            Assert.Equal(60, d.Days.Sum(x => x.Billed));
            Assert.Equal(0, d.Waiting);

            // a window in the past has nothing in it, and today's figures sit in the previous-period comparison tomorrow
            Assert.Equal(0, app.Run(sp => sp.GetRequiredService<AnalyticsService>().Build(new Period("x", "x", DateTime.Today.AddDays(-20), DateTime.Today.AddDays(-10)))).Billed);
            Assert.Equal(60, app.Run(sp => sp.GetRequiredService<AnalyticsService>().Build(new Period("x", "x", DateTime.Today.AddDays(1), DateTime.Today.AddDays(2)))).PrevBilled);
        }

        [Fact]
        public async Task Dashboard_and_report_pages_and_exports_work()
        {
            var (_, c) = await Setup();
            foreach (var p in new[] { "today", "7", "30", "month", "90" })
            {
                Assert.Contains("Sales billed", await c.GetStringAsync($"/Dashboard?p={p}"));
                Assert.Contains("KPI report", await c.GetStringAsync($"/Reports?p={p}&compare=false"));
            }
            foreach (var url in new[] { "/Reports?handler=Export", "/Invoices?handler=Export", "/Collections?handler=Export", "/Production?handler=Export" })
                Assert.Equal(HttpStatusCode.OK, (await c.GetAsync(url)).StatusCode);
        }
    }
}
