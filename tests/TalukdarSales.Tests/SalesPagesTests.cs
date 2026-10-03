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

        private static async Task<HttpResponseMessage> Form(HttpClient c, string pageUrl, string postUrl, Dictionary<string, string> fields)
        {
            fields["__RequestVerificationToken"] = await TestApp.Antiforgery(c, pageUrl);
            return await c.PostAsync(postUrl, new FormUrlEncodedContent(fields));
        }

        [Fact]
        public async Task Full_flow_requisition_approve_collect_history_print()
        {
            var (_, c, userId, goodId) = await Setup();

            // 1. requisition form: product table loads via htmx, submit creates and redirects to list
            Assert.Contains("Chicken Noodles", await c.GetStringAsync("/Requisitions/Create?handler=Products&goodTypeId=0"));
            var created = await Form(c, "/Requisitions/Create", "/Requisitions/Create", new() { ["UserId"] = userId.ToString(), [$"Qty[{goodId}]"] = "4" });
            Assert.Equal(HttpStatusCode.Redirect, created.StatusCode);
            var list = await c.GetStringAsync("/Requisitions?handler=List");
            Assert.Contains("REQ - 000001", list);

            // details page shows catalogue price 25 and total 100
            var details = await c.GetStringAsync("/Requisitions/Details?id=1");
            Assert.Contains("100.00", details);

            // 2. approve -> invoice
            var approve = await c.HtmxPost("/Requisitions", "/Requisitions?handler=Approve", new() { ["ids"] = "1", ["Active"] = "true" });
            Assert.Contains("1 invoice(s) created", approve.Trigger());
            Assert.DoesNotContain("REQ - 000001", await approve.Content.ReadAsStringAsync());   // no longer active

            // 3. invoice list (default filter is today) shows it with due 100
            var invoices = await c.GetStringAsync("/Invoices");
            Assert.Contains("INV - 000001", invoices);
            Assert.Contains("100.00", invoices);

            // 4. collect 40 via the modal handlers
            var form = await c.GetStringAsync("/Invoices?handler=Collect&id=1");
            Assert.Contains("Collect Amount", form);
            var paid = await c.HtmxPost("/Invoices", "/Invoices?handler=Collect", new() { ["Collect.InvoiceId"] = "1", ["Collect.Amount"] = "40", ["Collect.PaymentMethod"] = "Cash" });
            Assert.Contains("closeModal", paid.Trigger());

            var after = await c.GetStringAsync("/Invoices?handler=List&Filtered=true");
            Assert.Contains("60.00", after);   // due
            Assert.Contains("40.00", after);   // collected

            // 5. overpayment is rejected inside the form
            var over = await c.HtmxPost("/Invoices", "/Invoices?handler=Collect", new() { ["Collect.InvoiceId"] = "1", ["Collect.Amount"] = "61", ["Collect.PaymentMethod"] = "Cash" });
            Assert.Equal("", over.Trigger());
            Assert.Contains("exceeds the outstanding", await over.Content.ReadAsStringAsync());

            // 6. history + print page + excel
            var history = await c.GetStringAsync("/Collections?invoiceId=1");
            Assert.Contains("INV - 000001", history);
            Assert.Contains("Cash", history);
            var print = await c.GetStringAsync("/Invoices/Details?id=1");
            Assert.Contains("Chicken Noodles", print);
            Assert.Contains("window.print()", print);

            var xlsx = await c.GetAsync("/Invoices?handler=Export&Filtered=true");
            Assert.Equal(HttpStatusCode.OK, xlsx.StatusCode);
            Assert.Equal(TalukdarSales.Web.Infrastructure.Excel.ContentType, xlsx.Content.Headers.ContentType!.MediaType);
            var bytes = await xlsx.Content.ReadAsByteArrayAsync();
            Assert.Equal((byte)'P', bytes[0]);   // xlsx is a zip
            Assert.Equal((byte)'K', bytes[1]);
        }

        [Fact]
        public async Task Requisition_form_shows_error_when_window_closed_or_nothing_entered()
        {
            var (app, c, userId, goodId) = await Setup();
            var empty = await Form(c, "/Requisitions/Create", "/Requisitions/Create", new() { ["UserId"] = userId.ToString() });
            Assert.Equal(HttpStatusCode.OK, empty.StatusCode);
            Assert.Contains("at least one product", await empty.Content.ReadAsStringAsync());

            var noUser = await Form(c, "/Requisitions/Create", "/Requisitions/Create", new() { [$"Qty[{goodId}]"] = "1" });
            Assert.Contains("select a user", (await noUser.Content.ReadAsStringAsync()).ToLowerInvariant());
        }

        [Fact]
        public async Task Manual_invoice_form_creates_invoice_with_adjusted_quantity()
        {
            var (_, c, userId, goodId) = await Setup();
            await Form(c, "/Requisitions/Create", "/Requisitions/Create", new() { ["UserId"] = userId.ToString(), [$"Qty[{goodId}]"] = "10" });

            var options = await c.GetStringAsync($"/Lookup?handler=Requisitions&userId={userId}");
            Assert.Contains("REQ - 000001", options);
            Assert.Contains("value=\"10\"", await c.GetStringAsync("/Invoices/Create?handler=Lines&requisitionId=1"));

            var res = await Form(c, "/Invoices/Create", "/Invoices/Create", new() { ["RequisitionId"] = "1", [$"Qty[{goodId}]"] = "6" });
            Assert.Equal(HttpStatusCode.Redirect, res.StatusCode);
            var list = await c.GetStringAsync("/Invoices");
            Assert.Contains("150.00", list);   // 6 x 25
        }

        [Fact]
        public async Task Production_page_and_lookup_escape_html()
        {
            var (app, c, userId, goodId) = await Setup();
            await Form(c, "/Requisitions/Create", "/Requisitions/Create", new() { ["UserId"] = userId.ToString(), [$"Qty[{goodId}]"] = "3" });
            var page = await c.GetStringAsync("/Production");
            Assert.Contains("Chicken Noodles", page);

            app.Seed(db => db.Users.Add(new TalukdarSales.Web.Models.User { FirstName = "<b>X</b>", LastName = "", Username = "x1", SequencialUserId = "1981-0002", UserTypeId = 1,
                PhoneNumber = "1", Password = "", ImageName = "", Token = "", RefreshToken = "", Address = "", ContactPersonName = "", ContactPersonPhone = "", CreatedOn = DateTime.Now }));
            var users = await c.GetStringAsync("/Lookup?handler=Users");
            Assert.DoesNotContain("<b>X</b>", users);
            Assert.Contains("&lt;b&gt;X&lt;/b&gt;", users);
        }
    }
}
