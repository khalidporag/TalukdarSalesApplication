using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TalukdarSales.Web.Helpers;
using TalukdarSales.Web.Models;
using TalukdarSales.Web.Services;
using Xunit;

namespace TalukdarSales.Tests
{
    public class PaginationAndFilterTests
    {
        private static async Task<(TestApp app, HttpClient c)> Setup()
        {
            var app = new TestApp();
            app.SeedUser("admin", "pw", "Ada");
            app.SeedOpenWindow();
            var c = app.NewClient();
            await TestApp.Login(c, "admin", "pw");
            return (app, c);
        }

        private static void Customers(TestApp app, int n, double due = 0, double limit = 0) =>
            app.Seed(db =>
            {
                for (var i = 0; i < n; i++)
                    db.Users.Add(new User { FirstName = "Cust" + i.ToString("D3"), LastName = "Shop", Username = "c" + i, SequencialUserId = "1981-" + (500 + i), UserTypeId = 1, PhoneNumber = "017" + i,
                        Password = "", ImageName = "", Token = "", RefreshToken = "", Address = "", ContactPersonName = "", ContactPersonPhone = "", DueAmount = due, MaxCreditLimit = limit, CreatedOn = DateTime.Now.AddMinutes(-i) });
            });

        // ---- the shared pager --------------------------------------------------------------------------------

        [Fact]
        public async Task Pager_shows_the_range_rows_per_page_and_ellipses()
        {
            var (app, c) = await Setup();
            Customers(app, 119);   // 120 customers with the admin

            var first = await c.GetStringAsync("/Users?size=10");
            Assert.Contains("Showing <b>1</b> to <b>10</b> of <b>120</b>", first);
            Assert.Contains("aria-label=\"Last page\"", first);
            Assert.Contains("class=\"off\" href=\"#\" aria-label=\"First page\"", first);      // no first/previous on page 1
            Assert.Contains("<option value=\"/Users?name=&amp;status=&amp;sort=&amp;typeId=&amp;size=25&amp;p=1\"", first);

            var middle = await c.GetStringAsync("/Users?size=10&p=7");
            Assert.Contains("Showing <b>61</b> to <b>70</b> of <b>120</b>", middle);
            Assert.Contains("…", middle);                                                      // gaps between 1 ... 5 6 7 8 9 ... 12
            Assert.Contains("aria-current=\"page\" aria-label=\"Page 7\"", middle);

            var last = await c.GetStringAsync("/Users?size=10&p=12");
            Assert.Contains("Showing <b>111</b> to <b>120</b> of <b>120</b>", last);

            // only the offered sizes are accepted; a page past the end shows the last page rather than nothing
            Assert.Contains("to <b>10</b> of <b>120</b>", await c.GetStringAsync("/Users?size=7"));
            Assert.Contains("Showing <b>111</b>", await c.GetStringAsync("/Users?size=10&p=99"));
        }

        [Fact]
        public async Task Pager_keeps_the_active_filters_in_its_links()
        {
            var (app, c) = await Setup();
            Customers(app, 40, due: 100, limit: 1000);
            var html = await c.GetStringAsync("/Users?status=owes&sort=due&size=10");
            Assert.Contains("status=owes&amp;sort=due", html.Replace("&amp;amp;", "&amp;"));
            Assert.Contains("Showing <b>1</b> to <b>10</b> of <b>40</b>", html);   // the admin owes nothing so is filtered out
        }

        // ---- customers ---------------------------------------------------------------------------------------

        [Fact]
        public async Task Customers_filter_by_balance_and_sort_by_what_they_owe()
        {
            var (app, c) = await Setup();
            app.Seed(db =>
            {
                User U(string n, double due, double limit) => new() { FirstName = n, LastName = "", Username = n, SequencialUserId = n, UserTypeId = 1, PhoneNumber = "1", Password = "", ImageName = "", Token = "", RefreshToken = "", Address = "", ContactPersonName = "", ContactPersonPhone = "", DueAmount = due, MaxCreditLimit = limit, CreatedOn = DateTime.Now };
                db.Users.AddRange(U("Clear", 0, 1000), U("Owing", 300, 1000), U("Maxed", 1000, 1000), U("Bigowe", 5000, 0));
            });
            string Names(string html) => string.Join(",", System.Text.RegularExpressions.Regex.Matches(html, "<b>(Clear|Owing|Maxed|Bigowe)</b>").Select(m => m.Groups[1].Value));

            Assert.Equal(4, Names(await c.GetStringAsync("/Users")).Split(',').Length);
            Assert.Equal("Bigowe,Maxed,Owing", Names(await c.GetStringAsync("/Users?status=owes&sort=due")));
            Assert.Equal("Maxed", Names(await c.GetStringAsync("/Users?status=over")));              // limit 0 means no limit, so Bigowe is not "over"
            Assert.Equal("Bigowe,Clear,Maxed,Owing", Names(await c.GetStringAsync("/Users?sort=name")));
        }

        // ---- products ----------------------------------------------------------------------------------------

        [Fact]
        public async Task Products_filter_by_availability_and_sort_by_price()
        {
            var (app, c) = await Setup();
            app.SeedGood("Cheap", 5); app.SeedGood("Mid", 50); app.SeedGood("Dear", 500, active: false);
            string Names(string html) => string.Join(",", System.Text.RegularExpressions.Regex.Matches(html, "<b>(Cheap|Mid|Dear)</b>").Select(m => m.Groups[1].Value));

            Assert.Equal("Cheap,Dear,Mid", Names(await c.GetStringAsync("/Products")));
            Assert.Equal("Cheap,Mid", Names(await c.GetStringAsync("/Products?status=available")));
            Assert.Equal("Dear", Names(await c.GetStringAsync("/Products?status=hidden")));
            Assert.Equal("Dear,Mid,Cheap", Names(await c.GetStringAsync("/Products?sort=priceDesc")));
            Assert.Equal("Cheap,Mid", Names(await c.GetStringAsync("/Products?sort=price&status=available")));
        }

        // ---- collections -------------------------------------------------------------------------------------

        private static void Payments(TestApp app, int n)
        {
            app.Seed(db =>
            {
                var u = db.Users.First();
                var inv = new SalesInvoice { InvoiceSerialNo = "INV - 000900", UserId = u.Id, TotalPrice = 100000, CreatedDateTime = DateTime.Now, CreatedOn = DateTime.Now };
                db.SalesInvoices.Add(inv); db.SaveChanges();
                for (var i = 0; i < n; i++)
                    db.CollectionLedgers.Add(new CollectionLedger { UserId = u.Id, SalesInvoiceId = inv.Id, CollectionAmount = 10, PaymentMethod = i % 3 == 0 ? "Cash" : "bKash", CreatedOn = DateTime.Today.AddHours(8).AddMinutes(i).AddDays(-(i % 20)) });
            });
        }

        [Fact]
        public async Task Collections_are_paged_in_sql_and_totals_cover_the_whole_filter()
        {
            var (app, c) = await Setup();
            Payments(app, 60);   // 20 cash (i%3==0), 40 bKash, spread over 20 days

            var all = app.Run(sp => sp.GetRequiredService<InvoiceService>().Collections(null, null, null, null, 1, 10));
            Assert.Equal(10, all.Page.Items.Count);
            Assert.Equal(60, all.Page.Total);
            Assert.Equal(6, all.Page.TotalPages);
            Assert.Equal(600, all.Total);              // the total is not just this page
            Assert.Equal(60, all.Payments);
            Assert.Equal(new[] { "bKash", "Cash" }, all.ByMethod.Select(m => m.Method));
            Assert.Equal(new[] { 400d, 200d }, all.ByMethod.Select(m => m.Amount));

            var cash = app.Run(sp => sp.GetRequiredService<InvoiceService>().Collections(null, null, "cash", null, 1, 50));
            Assert.Equal(20, cash.Page.Total);
            Assert.Equal(200, cash.Total);

            var ranged = app.Run(sp => sp.GetRequiredService<InvoiceService>().Collections(DateTime.Today.AddDays(-1), DateTime.Today, null, null, 1, 50));
            Assert.Equal(ranged.Page.Total, ranged.Page.Items.Count);
            Assert.All(ranged.Page.Items, r => Assert.True(r.Time.Date >= DateTime.Today.AddDays(-1)));

            Assert.Equal(60, app.Run(sp => sp.GetRequiredService<InvoiceService>().Collections(null, null, null, "lovelace", 1, 10)).Page.Total);   // by customer name
            Assert.Equal(60, app.Run(sp => sp.GetRequiredService<InvoiceService>().Collections(null, null, null, "inv - 0009", 1, 10)).Page.Total);   // by invoice number
            Assert.Equal(0, app.Run(sp => sp.GetRequiredService<InvoiceService>().Collections(null, null, null, "nobody", 1, 10)).Page.Total);

            var page = await c.GetStringAsync("/Collections?days=0&size=10&p=2");
            Assert.Contains("Showing <b>11</b> to <b>20</b> of <b>60</b>", page);
            Assert.Contains("৳ 600", page);
            Assert.Contains("Showing <b>1</b> to <b>10</b> of <b>20</b>", await c.GetStringAsync("/Collections?days=0&size=10&method=Cash"));
            Assert.Equal(HttpStatusCode.OK, (await c.GetAsync("/Collections?handler=Export&days=0")).StatusCode);
        }

        [Fact]
        public async Task Collection_day_totals_cover_the_whole_day_even_when_it_spans_pages()
        {
            var (app, _) = await Setup();
            Payments(app, 60);
            var p1 = app.Run(sp => sp.GetRequiredService<InvoiceService>().Collections(null, null, null, null, 1, 5));
            var day = p1.Page.Items[0].Time.Date;
            var onThatDay = app.Run(sp => sp.GetRequiredService<InvoiceService>().Collections(day, day, null, null, 1, 100)).Page.Total * 10d;
            Assert.Equal(onThatDay, p1.DayTotals[day]);
        }

        // ---- notifications -----------------------------------------------------------------------------------

        [Fact]
        public async Task Notifications_page_and_filter_by_unread_and_kind()
        {
            var (app, c) = await Setup();
            var id = app.Run(sp => sp.GetRequiredService<TalukdarSales.Web.Context.ApplicationDbContext>().Users.Single(u => u.Username == "admin").Id);
            app.Seed(db =>
            {
                for (var i = 0; i < 30; i++)
                    db.Notifications.Add(new Notification { UserId = id, Kind = i % 3 == 0 ? "over-limit" : "new-order", Title = "Note " + i, Body = "b", Link = "/", IsRead = i < 10, CreatedOn = DateTime.Now.AddMinutes(-i) });
            });

            Assert.Contains("Showing <b>1</b> to <b>25</b> of <b>30</b>", await c.GetStringAsync("/Notifications"));
            Assert.Contains("Showing <b>26</b> to <b>30</b> of <b>30</b>", await c.GetStringAsync("/Notifications?p=2"));
            Assert.Contains("Showing <b>1</b> to <b>20</b> of <b>20</b>", await c.GetStringAsync("/Notifications?unread=true"));
            Assert.Contains("of <b>10</b>", await c.GetStringAsync("/Notifications?kind=over-limit"));
            Assert.Contains("of <b>6</b>", await c.GetStringAsync("/Notifications?kind=over-limit&unread=true"));   // unread over-limit: i = 12, 15, 18, 21, 24, 27
            Assert.Contains("of <b>30</b>", await c.GetStringAsync("/Notifications?kind=bogus"));                    // unknown kinds are ignored
        }

        // ---- orders and invoices: custom date range ----------------------------------------------------------

        [Fact]
        public void Orders_and_invoices_take_a_custom_date_range_that_overrides_the_preset()
        {
            var app = new TestApp();
            var u = app.SeedUser("admin", "pw", "Ada");
            var good = app.SeedGood("Soap", 10);
            app.SeedOpenWindow();
            var req = app.Run(sp => sp.GetRequiredService<RequisitionService>().Create(u, new[] { new RequisitionLine(good, 1) })).Requisition.Id;
            app.Run(sp => sp.GetRequiredService<InvoiceService>().CreateFromRequisitions(new[] { req }));
            app.Seed(db => { db.SalesRequisitions.Single().CreatedDateTime = DateTime.Today.AddDays(-10); db.SalesInvoices.Single().CreatedDateTime = DateTime.Today.AddDays(-10); });

            Assert.Equal(0, app.Run(sp => sp.GetRequiredService<RequisitionService>().Board("all", null, null, 7, 1, 25)).Page.Total);                  // preset: last 7 days
            Assert.Equal(1, app.Run(sp => sp.GetRequiredService<RequisitionService>().Board("all", null, null, 7, 1, 25, DateTime.Today.AddDays(-12), DateTime.Today.AddDays(-9))).Page.Total);   // custom wins
            Assert.Equal(0, app.Run(sp => sp.GetRequiredService<RequisitionService>().Board("all", null, null, 0, 1, 25, DateTime.Today.AddDays(-5), null)).Page.Total);
            Assert.Equal(1, app.Run(sp => sp.GetRequiredService<InvoiceService>().Board("all", null, 7, 1, 25, DateTime.Today.AddDays(-12), DateTime.Today.AddDays(-9))).Page.Total);
            Assert.Equal(0, app.Run(sp => sp.GetRequiredService<InvoiceService>().Board("all", null, 30, 1, 25, null, DateTime.Today.AddDays(-11))).Page.Total);
        }

        [Fact]
        public async Task Order_and_invoice_pages_accept_the_range_and_page_size()
        {
            var (app, c) = await Setup();
            foreach (var url in new[] { "/Requisitions?from=2026-01-01&to=2026-12-31&size=50", "/Invoices?from=2026-01-01&to=2026-12-31&size=10&status=paid", "/Audit?size=100", "/Products?size=25", "/Users?size=50" })
                Assert.Equal(HttpStatusCode.OK, (await c.GetAsync(url)).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await c.GetAsync("/Invoices?handler=Export&from=2026-01-01&to=2026-12-31")).StatusCode);
        }

        // ---- statement ---------------------------------------------------------------------------------------

        [Fact]
        public async Task Statement_pages_its_lines_keeps_the_running_balance_and_can_show_everything_for_printing()
        {
            var (app, c) = await Setup();
            var uid = app.Run(sp => sp.GetRequiredService<TalukdarSales.Web.Context.ApplicationDbContext>().Users.Single(u => u.Username == "admin").Id);
            var good = app.SeedGood("Soap", 10);
            for (var i = 0; i < 12; i++)
            {
                var req = app.Run(sp => sp.GetRequiredService<RequisitionService>().Create(uid, new[] { new RequisitionLine(good, 1) })).Requisition.Id;
                var inv = app.Run(sp => sp.GetRequiredService<InvoiceService>().CreateFromRequisitions(new[] { req })).Invoices[0].Id;
                app.Run(sp => sp.GetRequiredService<InvoiceService>().Collect(inv, 4, "Cash"));
            }   // 24 lines: 12 invoices + 12 payments

            var p1 = await c.GetStringAsync($"/Users/Statement?id={uid}&size=10");
            var n = app.Run(sp => sp.GetRequiredService<InvoiceService>().StatementFor(uid, DateTime.Today.AddDays(-89), DateTime.Today)).Entries.Count;   // a payment that spans invoices is one line per invoice
            Assert.InRange(n, 24, 40);
            var payments = n - 12;
            Assert.Contains($"Showing <b>1</b> to <b>10</b> of <b>{n}</b>", p1);
            Assert.Contains("Opening balance", p1);
            var lastPage = (int)Math.Ceiling(n / 10.0);
            var p3 = await c.GetStringAsync($"/Users/Statement?id={uid}&size=10&p={lastPage}");
            Assert.Contains($"to <b>{n}</b> of <b>{n}</b>", p3);
            Assert.DoesNotContain("Opening balance", p3);          // only on the first page
            Assert.Contains("Closing balance", p3);
            Assert.Contains("৳ 72", p3);                           // 12 x (10 - 4) owed at the end, whichever page you are on

            Assert.Contains($"of <b>{payments}</b>", await c.GetStringAsync($"/Users/Statement?id={uid}&kind=Payment"));
            var all = await c.GetStringAsync($"/Users/Statement?id={uid}&all=true");
            Assert.Equal(n, System.Text.RegularExpressions.Regex.Matches(all, "<td>(Invoice|Payment)</td>").Count);
        }
    }
}
