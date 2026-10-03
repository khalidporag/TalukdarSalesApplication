using System.Net;
using Xunit;

namespace TalukdarSales.Tests
{
    public class SalesPagesTests
    {
        private static async Task<(TestApp app, HttpClient c, int userId, int goodId)> Setup()
        {
            var app = new TestApp();
            var userId = app.SeedUser("admin", "secret123", "Ada");
            var goodId = app.SeedGood("Chicken Noodles", 25);
            app.SeedOpenWindow();
            var c = app.NewClient();
            await TestApp.Login(c, "admin", "secret123");
            return (app, c, userId, goodId);
        }

        private static Task<HttpResponseMessage> Form(HttpClient c, string pageUrl, string postUrl, Dictionary<string, string> fields) =>
            c.FormPost(pageUrl, postUrl, fields);

        [Fact]
        public async Task Full_flow_order_invoice_collect_history_print()
        {
            var (_, c, userId, goodId) = await Setup();

            // 1. new order: the page lists customers and products; sending redirects to the board with a toast
            var create = await c.GetStringAsync("/Requisitions/Create");
            Assert.Contains("Chicken Noodles", create);
            Assert.Contains("Ada Lovelace", create);
            var created = await Form(c, "/Requisitions/Create", "/Requisitions/Create", new() { ["UserId"] = userId.ToString(), [$"Qty[{goodId}]"] = "4" });
            Assert.Equal(HttpStatusCode.Redirect, created.StatusCode);
            Assert.Contains("REQ - 000001 sent", await c.FlashAfter(created));
            var board = await c.GetStringAsync("/Requisitions");
            Assert.Contains("REQ - 000001", board);
            Assert.Contains("100", board);

            // the detail panel shows the catalogue price and total
            var panel = await c.GetStringAsync("/Requisitions?handler=Panel&id=1");
            Assert.Contains("Chicken Noodles", panel);
            Assert.Contains("100", panel);

            // 2. one-tap invoice
            var invoiced = await Form(c, "/Requisitions", "/Requisitions?handler=Invoice&id=1", new());
            Assert.Contains("INV - 000001 created", await c.FlashAfter(invoiced));
            Assert.DoesNotContain("REQ - 000001", await c.GetStringAsync("/Requisitions"));          // no longer waiting
            Assert.Contains("REQ - 000001", await c.GetStringAsync("/Requisitions?tab=invoiced"));

            // 3. invoice list shows it with due 100
            var invoices = await c.GetStringAsync("/Invoices");
            Assert.Contains("INV - 000001", invoices);
            Assert.Contains("Unpaid", invoices);

            // 4. collect 40 from the side panel
            var form = await c.GetStringAsync("/Invoices?handler=Panel&id=1");
            Assert.Contains("Collect payment", form);
            var paid = await Form(c, "/Invoices", "/Invoices?handler=Collect", new() { ["invoiceId"] = "1", ["amount"] = "40", ["method"] = "Cash" });
            Assert.Contains("collected by Cash", await c.FlashAfter(paid));
            var after = await c.GetStringAsync("/Invoices");
            Assert.Contains("Partly paid", after);
            Assert.Contains("৳ 60", after);   // due

            // 5. overpayment is rejected
            var over = await Form(c, "/Invoices", "/Invoices?handler=Collect", new() { ["invoiceId"] = "1", ["amount"] = "61", ["method"] = "Cash" });
            Assert.Contains("exceeds the outstanding", await c.FlashAfter(over));

            // 6. history + print page + excel
            var history = await c.GetStringAsync("/Collections");
            Assert.Contains("INV - 000001", history);
            Assert.Contains("Cash", history);
            var print = await c.GetStringAsync("/Invoices/Details?id=1");
            Assert.Contains("Chicken Noodles", print);
            Assert.Contains("window.print()", print);

            var xlsx = await c.GetAsync("/Invoices?handler=Export&days=0");
            Assert.Equal(HttpStatusCode.OK, xlsx.StatusCode);
            Assert.Equal(TalukdarSales.Web.Infrastructure.Excel.ContentType, xlsx.Content.Headers.ContentType!.MediaType);
            var bytes = await xlsx.Content.ReadAsByteArrayAsync();
            Assert.Equal((byte)'P', bytes[0]);   // xlsx is a zip
            Assert.Equal((byte)'K', bytes[1]);
        }

        [Fact]
        public async Task Order_form_shows_error_when_nothing_entered_or_no_customer()
        {
            var (_, c, userId, goodId) = await Setup();
            var empty = await Form(c, "/Requisitions/Create", "/Requisitions/Create", new() { ["UserId"] = userId.ToString() });
            Assert.Equal(HttpStatusCode.OK, empty.StatusCode);
            Assert.Contains("at least one product", await empty.Content.ReadAsStringAsync());

            var noUser = await Form(c, "/Requisitions/Create", "/Requisitions/Create", new() { [$"Qty[{goodId}]"] = "1" });
            Assert.Contains("choose a customer", (await noUser.Content.ReadAsStringAsync()).ToLowerInvariant());
        }

        [Fact]
        public async Task Adjusted_invoice_uses_the_quantities_from_the_panel()
        {
            var (_, c, userId, goodId) = await Setup();
            await Form(c, "/Requisitions/Create", "/Requisitions/Create", new() { ["UserId"] = userId.ToString(), [$"Qty[{goodId}]"] = "10" });
            Assert.Contains("value=\"10\"", await c.GetStringAsync("/Requisitions?handler=Panel&id=1"));

            var res = await Form(c, "/Requisitions", "/Requisitions?handler=Create&id=1", new() { [$"Qty[{goodId}]"] = "6" });
            Assert.Contains("INV - 000001 created", await c.FlashAfter(res));
            var list = await c.GetStringAsync("/Invoices");
            Assert.Contains("৳ 150", list);   // 6 x 25
        }

        [Fact]
        public async Task Bulk_invoice_creates_one_invoice_per_ticked_order()
        {
            var (_, c, userId, goodId) = await Setup();
            for (var i = 0; i < 2; i++)
                await Form(c, "/Requisitions/Create", "/Requisitions/Create", new() { ["UserId"] = userId.ToString(), [$"Qty[{goodId}]"] = "1" });
            var res = await c.PostAsync("/Requisitions?handler=Bulk", new FormUrlEncodedContent(new[]
            {
                KeyValuePair.Create("ids", "1"), KeyValuePair.Create("ids", "2"),
                KeyValuePair.Create("__RequestVerificationToken", await TestApp.Antiforgery(c, "/Requisitions"))
            }));
            Assert.Contains("2 invoices created", await c.FlashAfter(res));
        }

        [Fact]
        public async Task Production_page_groups_by_category_and_escapes_html()
        {
            var (app, c, userId, goodId) = await Setup();
            await Form(c, "/Requisitions/Create", "/Requisitions/Create", new() { ["UserId"] = userId.ToString(), [$"Qty[{goodId}]"] = "3" });
            var page = await c.GetStringAsync("/Production");
            Assert.Contains("Chicken Noodles", page);

            app.Seed(db => db.Users.Add(new TalukdarSales.Web.Models.User { FirstName = "<b>X</b>", LastName = "", Username = "x1", SequencialUserId = "1981-0002", UserTypeId = 1,
                PhoneNumber = "1", Password = "", ImageName = "", Token = "", RefreshToken = "", Address = "", ContactPersonName = "", ContactPersonPhone = "", CreatedOn = DateTime.Now }));
            var customers = await c.GetStringAsync("/Users");
            Assert.DoesNotContain("<b>X</b>", customers);
            Assert.Contains("&lt;b&gt;X&lt;/b&gt;", customers);
            var order = await c.GetStringAsync("/Requisitions/Create");
            Assert.DoesNotContain("<b>X</b>", order);
        }
    }
}
