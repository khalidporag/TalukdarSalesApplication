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
        [InlineData("/Invoices/Create")]
        [InlineData("/Collections")]
        [InlineData("/Production")]
        [InlineData("/Notices")]
        [InlineData("/Reports")]
        [InlineData("/ProductTypes")]
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
            Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync("/Requisitions/Details?id=999")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync("/Invoices?handler=Collect&id=999")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync("/Users?handler=Edit&id=999")).StatusCode);
        }

        [Fact]
        public async Task Report_panels_aggregate_per_customer()
        {
            var app = new TestApp();
            var u1 = app.SeedUser("a", "x", "Ann");
            app.Seed(db => db.Users.Add(new TalukdarSales.Web.Models.User { FirstName = "Bob", LastName = "", Username = "b", SequencialUserId = "1981-0002", UserTypeId = 1, PhoneNumber = "2",
                Password = "", ImageName = "", Token = "", RefreshToken = "", Address = "", ContactPersonName = "", ContactPersonPhone = "", CreatedOn = DateTime.Now }));
            var g = app.SeedGood("Soap", 10);
            app.SeedOpenWindow();

            var req = app.Run(sp => sp.GetRequiredService<RequisitionService>());
            int Make(int user, double qty)
            {
                var r = app.Run(sp => sp.GetRequiredService<RequisitionService>().Create(user, new[] { new RequisitionLine(g, qty) })).Requisition.Id;
                return app.Run(sp => sp.GetRequiredService<InvoiceService>().CreateFromRequisitions(new[] { r })).Invoices[0].Id;
            }
            Make(u1, 3);   // 30
            Make(u1, 2);   // 20  -> Ann total 50 (two invoices must be summed, not listed separately)
            Make(2, 1);    // 10  -> Bob

            var top = app.Run(sp => sp.GetRequiredService<ReportService>().TopSellers(null, null));
            Assert.Equal(2, top.Count);
            Assert.Equal("Ann Lovelace", top[0].UserName);
            Assert.Equal(50, top[0].Amount);

            var due = app.Run(sp => sp.GetRequiredService<ReportService>().TopDue(null, null));
            Assert.Equal(2, due.Count);
            Assert.Equal(50, due[0].Due);

            app.Run(sp => sp.GetRequiredService<InvoiceService>().Collect(1, 30, "Cash"));
            due = app.Run(sp => sp.GetRequiredService<ReportService>().TopDue(null, null));
            Assert.Equal(20, due.Single(d => d.UserName == "Ann Lovelace").Due);   // 50 - 30, summed per customer

            var products = app.Run(sp => sp.GetRequiredService<ReportService>().Products(null, null, best: true));
            Assert.Equal(6, products.Single().Quantity);

            // explicit range: today only includes everything, a past range includes nothing
            Assert.Equal(2, app.Run(sp => sp.GetRequiredService<ReportService>().TopSellers(DateTime.Today, DateTime.Today)).Count);
            Assert.Empty(app.Run(sp => sp.GetRequiredService<ReportService>().TopSellers(DateTime.Today.AddDays(-10), DateTime.Today.AddDays(-5))));
            Assert.Equal(1, app.Run(sp => sp.GetRequiredService<ReportService>().DailySales(null, null)).Count);
            Assert.Equal(1, app.Run(sp => sp.GetRequiredService<ReportService>().DailyOrders(null, null)).Count);
        }

        [Fact]
        public async Task Report_panel_and_export_endpoints_work()
        {
            var (_, c) = await Setup();
            foreach (var kind in new[] { "sellers", "best", "worst", "due", "sales", "orders" })
            {
                Assert.Contains("table", await c.GetStringAsync($"/Reports?handler=Panel&kind={kind}"));
                var x = await c.GetAsync($"/Reports?handler=Export&kind={kind}");
                Assert.Equal(HttpStatusCode.OK, x.StatusCode);
            }
            Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync("/Reports?handler=Export&kind=nope")).StatusCode);
        }
    }
}
