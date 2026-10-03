using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TalukdarSales.Web.Context;
using TalukdarSales.Web.Security;
using TalukdarSales.Web.Services;
using Xunit;

namespace TalukdarSales.Tests
{
    /// <summary>Order edit and cancel, discounts, returns, statements, audit log, notifications and salesperson attribution.</summary>
    public class Batch2Tests
    {
        private static (TestApp app, int userId, int goodId) Setup()
        {
            var app = new TestApp();
            var userId = app.SeedUser("admin", "pw", "Ada");
            var goodId = app.SeedGood("Soap", 25);
            app.SeedOpenWindow();
            return (app, userId, goodId);
        }

        private static async Task<HttpClient> Login(TestApp app, string user = "admin")
        {
            var c = app.NewClient();
            await TestApp.Login(c, user, "pw");
            return c;
        }

        private static int NewOrder(TestApp app, int userId, int goodId, double qty) =>
            app.Run(sp => sp.GetRequiredService<RequisitionService>().Create(userId, new[] { new RequisitionLine(goodId, qty) })).Requisition.Id;

        private static int Invoice(TestApp app, int reqId) =>
            app.Run(sp => sp.GetRequiredService<InvoiceService>().CreateFromRequisitions(new[] { reqId })).Invoices[0].Id;

        private static T Svc<T>(TestApp app, Func<IServiceProvider, T> f) => app.Run(f);

        // ---- #5 order edit and cancel -----------------------------------------------------------------------

        [Fact]
        public async Task A_waiting_order_can_be_edited_and_keeps_its_price()
        {
            var (app, userId, good) = Setup();
            var req = NewOrder(app, userId, good, 4);
            app.Seed(db => db.FinishedGoods.Single(g => g.Id == good).UnitPrice = 40);   // catalogue price changes afterwards
            var c = await Login(app);

            Assert.Contains("Edit REQ - 000001", await c.GetStringAsync($"/Requisitions/Create?edit={req}"));
            var res = await c.FormPost($"/Requisitions/Create?edit={req}", $"/Requisitions/Create?edit={req}", new() { [$"Qty[{good}]"] = "2" });
            Assert.Equal("Order updated.", await c.FlashAfter(res));

            var panel = await c.GetStringAsync($"/Requisitions?handler=Panel&id={req}");
            Assert.Contains("৳ 50", panel);     // 2 x the original price 25, not 40
            Assert.Contains("Soap", panel);
        }

        [Fact]
        public async Task Cancelled_orders_leave_the_invoice_list_production_and_figures()
        {
            var (app, userId, good) = Setup();
            var req = NewOrder(app, userId, good, 4);
            var c = await Login(app);

            var noReason = await c.FormPost("/Requisitions", $"/Requisitions?handler=Cancel&id={req}", new() { ["reason"] = " " });
            Assert.Contains("reason", await c.FlashAfter(noReason));

            var ok = await c.FormPost("/Requisitions", $"/Requisitions?handler=Cancel&id={req}", new() { ["reason"] = "Customer called" });
            Assert.Equal("Order cancelled.", await c.FlashAfter(ok));

            Assert.DoesNotContain("REQ - 000001", await c.GetStringAsync("/Requisitions"));
            var tab = await c.GetStringAsync("/Requisitions?tab=cancelled&id=" + req);
            Assert.Contains("Cancelled", tab);
            Assert.Contains("Customer called", tab);
            Assert.Empty(Svc(app, sp => sp.GetRequiredService<RequisitionService>().ProductTotalsForDay(DateTime.Today)));
            Assert.Equal(0, Svc(app, sp => sp.GetRequiredService<RequisitionService>().OrderCountForDay(DateTime.Today)));
            Assert.Equal(0, Svc(app, sp => sp.GetRequiredService<AnalyticsService>().Build(Period.Parse("30"))).Orders);

            // a cancelled order cannot be invoiced or edited
            Assert.False(Svc(app, sp => sp.GetRequiredService<InvoiceService>().CreateFromRequisitions(new[] { req })).Ok);
            Assert.False(Svc(app, sp => sp.GetRequiredService<RequisitionService>().Update(req, new[] { new RequisitionLine(good, 1) })).Ok);
        }

        [Fact]
        public async Task Editing_and_cancelling_need_their_own_permission()
        {
            var (app, userId, good) = Setup();
            var req = NewOrder(app, userId, good, 2);
            app.SeedRole("Clerk", Perm.RequisitionView, Perm.RequisitionApprove, Perm.RequisitionCreate);
            app.SeedUser("clerk", "pw", "Cleo", role: "Clerk");
            var c = await Login(app, "clerk");
            var editPage = await c.GetAsync($"/Requisitions/Create?edit={req}");
            Assert.True(editPage.StatusCode == HttpStatusCode.Forbidden || editPage.StatusCode == HttpStatusCode.Redirect && editPage.Headers.Location!.OriginalString.Contains("AccessDenied"));
            var res = await c.FormPost("/Requisitions", $"/Requisitions?handler=Cancel&id={req}", new() { ["reason"] = "x" });
            Assert.True(res.StatusCode == HttpStatusCode.Forbidden || res.StatusCode == HttpStatusCode.Redirect && res.Headers.Location!.OriginalString.Contains("AccessDenied"));
            Assert.DoesNotContain("handler=Cancel", await c.GetStringAsync($"/Requisitions?id={req}"));   // no Cancel button
            Assert.True(Svc(app, sp => sp.GetRequiredService<RequisitionService>().Get(req))!.Value.Header.IsActive);
        }

        // ---- #6 discounts -----------------------------------------------------------------------------------

        [Fact]
        public async Task Discount_reduces_the_invoice_and_the_customer_balance_within_the_limit()
        {
            var (app, userId, good) = Setup();
            var req = NewOrder(app, userId, good, 4);   // 100
            var tooMuch = Svc(app, sp => sp.GetRequiredService<InvoiceService>().CreateForRequisition(req, new[] { new InvoiceLine(good, 4) }, 0, 25));
            Assert.False(tooMuch.Ok);
            Assert.Contains("limited to 20", tooMuch.Error);
            Assert.False(Svc(app, sp => sp.GetRequiredService<InvoiceService>().CreateForRequisition(req, new[] { new InvoiceLine(good, 4) }, 150, 0)).Ok);

            var ok = Svc(app, sp => sp.GetRequiredService<InvoiceService>().CreateForRequisition(req, new[] { new InvoiceLine(good, 4) }, 0, 10));
            Assert.True(ok.Ok);
            Assert.Equal(90, ok.Invoice.TotalPrice);
            Assert.Equal(10, ok.Invoice.DiscountAmount);
            Assert.Equal(90, app.Run(sp => sp.GetRequiredService<InvoiceService>().OutstandingFor(userId)));

            var c = await Login(app);
            var print = await c.GetStringAsync($"/Invoices/Details?id={ok.Invoice.Id}");
            Assert.Contains("Discount", print);
        }

        [Fact]
        public async Task The_page_refuses_discounts_without_the_discount_permission()
        {
            var (app, userId, good) = Setup();
            var req = NewOrder(app, userId, good, 4);
            app.SeedRole("Clerk", Perm.RequisitionView, Perm.InvoiceCreate);
            app.SeedUser("clerk", "pw", "Cleo", role: "Clerk");
            var c = await Login(app, "clerk");
            Assert.DoesNotContain("name=\"discount\"", await c.GetStringAsync($"/Requisitions?handler=Panel&id={req}"));
            var res = await c.FormPost("/Requisitions", $"/Requisitions?handler=Create&id={req}", new() { [$"Qty[{good}]"] = "4", ["discount"] = "10", ["discountKind"] = "percent" });
            Assert.Contains("permission to give discounts", await c.FlashAfter(res));
            Assert.True(Svc(app, sp => sp.GetRequiredService<RequisitionService>().Get(req))!.Value.Header.IsActive);   // nothing invoiced

            app.SeedRole("Clerk", Perm.RequisitionView, Perm.InvoiceCreate, Perm.InvoiceDiscount);
            Assert.Contains("name=\"discount\"", await c.GetStringAsync($"/Requisitions?handler=Panel&id={req}"));
            var ok = await c.FormPost("/Requisitions", $"/Requisitions?handler=Create&id={req}", new() { [$"Qty[{good}]"] = "4", ["discount"] = "10", ["discountKind"] = "percent" });
            Assert.Contains("created", await c.FlashAfter(ok));
        }

        // ---- #10 returns ------------------------------------------------------------------------------------

        [Fact]
        public void Returning_goods_on_a_paid_invoice_credits_and_refunds_the_excess()
        {
            var (app, userId, good) = Setup();
            var inv = Invoice(app, NewOrder(app, userId, good, 4));   // 100
            Assert.True(app.Run(sp => sp.GetRequiredService<InvoiceService>().Collect(inv, 100, "Cash")).Ok);

            Assert.False(app.Run(sp => sp.GetRequiredService<InvoiceService>().ReturnGoods(inv, new[] { new InvoiceLine(good, 1) }, " ")).Ok);          // reason needed
            Assert.False(app.Run(sp => sp.GetRequiredService<InvoiceService>().ReturnGoods(inv, new[] { new InvoiceLine(good, 5) }, "too many")).Ok);   // more than sold

            var r = app.Run(sp => sp.GetRequiredService<InvoiceService>().ReturnGoods(inv, new[] { new InvoiceLine(good, 1) }, "Damaged"));
            Assert.True(r.Ok);
            Assert.Equal(25, r.Note.Amount);
            Assert.Equal(25, r.Note.Refund);

            var d = app.Run(sp => sp.GetRequiredService<InvoiceService>().Get(inv));
            Assert.Equal(75, d.Header.Total);
            Assert.Equal(75, d.Header.Collected);
            Assert.Equal(25, d.Returned);
            Assert.Equal(0, app.Run(sp => sp.GetRequiredService<InvoiceService>().OutstandingFor(userId)));
            Assert.Contains(app.Run(sp => sp.GetRequiredService<InvoiceService>().CollectionHistory(userId, inv, null, null)).Rows, x => x.PaymentMethod == "Refund" && x.Amount == -25);

            Assert.Equal(3, app.Run(sp => sp.GetRequiredService<InvoiceService>().Returnable(inv)).Single().Left);
            Assert.False(app.Run(sp => sp.GetRequiredService<InvoiceService>().ReturnGoods(inv, new[] { new InvoiceLine(good, 4) }, "again")).Ok);   // only 3 left
        }

        [Fact]
        public void Returns_on_an_unpaid_discounted_invoice_reduce_what_is_owed_by_the_discounted_value()
        {
            var (app, userId, good) = Setup();
            var req = NewOrder(app, userId, good, 4);
            var inv = app.Run(sp => sp.GetRequiredService<InvoiceService>().CreateForRequisition(req, new[] { new InvoiceLine(good, 4) }, 0, 10)).Invoice.Id;   // 90
            var r = app.Run(sp => sp.GetRequiredService<InvoiceService>().ReturnGoods(inv, new[] { new InvoiceLine(good, 2) }, "Wrong item"));
            Assert.Equal(45, r.Note.Amount);       // half the goods, half the discounted total
            Assert.Equal(0, r.Note.Refund);
            Assert.Equal(45, app.Run(sp => sp.GetRequiredService<InvoiceService>().OutstandingFor(userId)));
            Assert.Equal(45, app.Run(sp => sp.GetRequiredService<InvoiceService>().Get(inv)).Header.Total);
        }

        [Fact]
        public async Task Return_pages_render_and_need_the_return_permission()
        {
            var (app, userId, good) = Setup();
            var inv = Invoice(app, NewOrder(app, userId, good, 4));
            var c = await Login(app);
            Assert.Contains("Return goods", await c.GetStringAsync($"/Invoices?id={inv}") + await c.GetStringAsync($"/Invoices/Details?id={inv}"));
            var res = await c.FormPost($"/Invoices/Return?id={inv}", $"/Invoices/Return?id={inv}", new() { [$"Qty[{good}]"] = "1", ["Reason"] = "Damaged" });
            Assert.Contains("CN - 000001", await c.FlashAfter(res));
            Assert.Contains("CN - 000001", await c.GetStringAsync($"/Invoices/Details?id={inv}"));

            app.SeedRole("Viewer", Perm.InvoiceView);
            app.SeedUser("viewer", "pw", "Vic", role: "Viewer");
            var v = await Login(app, "viewer");
            Assert.DoesNotContain("Return goods", await v.GetStringAsync($"/Invoices/Details?id={inv}"));
            var denied = await v.GetAsync($"/Invoices/Return?id={inv}");
            Assert.True(denied.StatusCode == HttpStatusCode.Forbidden || denied.StatusCode == HttpStatusCode.Redirect);
        }

        // ---- #4 statement -----------------------------------------------------------------------------------

        [Fact]
        public async Task Statement_running_balance_ends_at_what_the_customer_owes()
        {
            var (app, userId, good) = Setup();
            var inv = Invoice(app, NewOrder(app, userId, good, 4));                 // +100
            app.Run(sp => sp.GetRequiredService<InvoiceService>().Collect(inv, 60, "bKash"));   // -60
            app.Run(sp => sp.GetRequiredService<InvoiceService>().ReturnGoods(inv, new[] { new InvoiceLine(good, 1) }, "Damaged"));   // -25 (total 75, paid 60, due 15)
            var inv2 = Invoice(app, NewOrder(app, userId, good, 2));                // +50

            var st = app.Run(sp => sp.GetRequiredService<InvoiceService>().StatementFor(userId, DateTime.Today.AddDays(-30), DateTime.Today));
            Assert.Equal(0, st.Opening);
            Assert.Equal(new[] { "Credit note", "Invoice", "Invoice", "Payment" }, st.Entries.Select(e => e.Kind).OrderBy(k => k).ToArray());
            Assert.Equal(65, st.Closing);                                           // 100 - 60 - 25 + 50
            Assert.Equal(app.Run(sp => sp.GetRequiredService<InvoiceService>().OutstandingFor(userId)), st.Closing);
            Assert.Equal(st.Closing, app.Run(sp => sp.GetRequiredService<ApplicationDbContext>().Users.Single(u => u.Id == userId).DueAmount));

            // a later period starts from the earlier balance
            var later = app.Run(sp => sp.GetRequiredService<InvoiceService>().StatementFor(userId, DateTime.Today.AddDays(1), DateTime.Today.AddDays(5)));
            Assert.Equal(65, later.Opening);
            Assert.Empty(later.Entries);

            var c = await Login(app);
            var page = await c.GetStringAsync($"/Users/Statement?id={userId}");
            Assert.Contains("Statement of account", page);
            Assert.Contains("INV - 000001", page);
            Assert.Contains("CN - 000001", page);
            Assert.Equal(HttpStatusCode.OK, (await c.GetAsync($"/Users/Statement?handler=Export&id={userId}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync("/Users/Statement?id=999")).StatusCode);
        }

        // ---- #11 audit log ----------------------------------------------------------------------------------

        [Fact]
        public async Task Actions_are_recorded_with_who_did_them_and_the_log_is_permission_protected()
        {
            var (app, userId, good) = Setup();
            var c = await Login(app);
            await c.FormPost("/Requisitions/Create", "/Requisitions/Create", new() { ["UserId"] = userId.ToString(), [$"Qty[{good}]"] = "2" });
            await c.FormPost("/Requisitions", "/Requisitions?handler=Invoice&id=1", new());
            await c.FormPost("/Invoices", "/Invoices?handler=Collect", new() { ["invoiceId"] = "1", ["amount"] = "20", ["method"] = "Cash" });
            await c.HtmxPost("/Products?editId=1", "/Products?handler=Edit", new() { ["Edit.Id"] = "1", ["Edit.Description"] = "x", ["Edit.UnitPrice"] = "30", ["Edit.IsActive"] = "true" });

            var log = app.Run(sp => sp.GetRequiredService<ApplicationDbContext>().AuditLogs.ToList());
            Assert.Contains(log, l => l.Action == "order.create" && l.UserName == "Ada Lovelace" && l.UserId == userId);
            Assert.Contains(log, l => l.Action == "invoice.create");
            Assert.Contains(log, l => l.Action == "payment.collect" && l.Summary.Contains("20"));
            Assert.Contains(log, l => l.Action == "price.change" && l.Summary.Contains("25") && l.Summary.Contains("30"));

            var page = await c.GetStringAsync("/Audit");
            Assert.Contains("order.create", page);
            Assert.Contains("Ada Lovelace", await c.GetStringAsync("/Audit?q=lovelace&entity=Payment"));
            Assert.DoesNotContain("order.create", await c.GetStringAsync("/Audit?entity=Payment"));
            Assert.Equal(HttpStatusCode.OK, (await c.GetAsync("/Audit?handler=Export")).StatusCode);

            app.SeedRole("Viewer", Perm.InvoiceView);
            app.SeedUser("viewer", "pw", "Vic", role: "Viewer");
            var v = await Login(app, "viewer");
            var denied = await v.GetAsync("/Audit");
            Assert.True(denied.StatusCode == HttpStatusCode.Forbidden || denied.StatusCode == HttpStatusCode.Redirect);
            app.SeedRole("Auditor", Perm.AuditView);
            app.SeedUser("auditor", "pw", "Aud", role: "Auditor");
            Assert.Equal(HttpStatusCode.OK, (await (await Login(app, "auditor")).GetAsync("/Audit")).StatusCode);
        }

        // ---- #12 notifications ------------------------------------------------------------------------------

        [Fact]
        public async Task New_orders_and_over_limit_orders_notify_the_people_who_invoice()
        {
            var (app, userId, good) = Setup();
            app.SeedRole("Clerk", Perm.RequisitionView, Perm.RequisitionApprove);
            var clerk = app.SeedUser("clerk", "pw", "Cleo", role: "Clerk");
            app.SeedRole("Other", Perm.Production);
            var other = app.SeedUser("other", "pw", "Otto", role: "Other");
            app.Seed(db => { var u = db.Users.Single(x => x.Id == userId); u.MaxCreditLimit = 50; });

            NewOrder(app, userId, good, 4);   // 100 > limit 50

            var notes = app.Run(sp => sp.GetRequiredService<ApplicationDbContext>().Notifications.ToList());
            Assert.Contains(notes, n => n.UserId == clerk && n.Kind == "new-order");
            Assert.Contains(notes, n => n.UserId == clerk && n.Kind == "over-limit" && n.Body.Contains("limit"));
            Assert.Contains(notes, n => n.UserId == userId && n.Kind == "new-order");        // administrators are included
            Assert.DoesNotContain(notes, n => n.UserId == other);                           // no permission, no notification

            var c = await Login(app, "clerk");
            var html = await c.GetStringAsync("/Notifications");
            Assert.Contains("New order REQ - 000001", html);
            Assert.Contains("Over credit limit", html);
            Assert.Equal(2, app.Run(sp => sp.GetRequiredService<NotificationService>().UnreadCount(clerk)));
            Assert.Contains("badge", await c.GetStringAsync("/Notifications"));              // unread count in the menu

            var id = notes.First(n => n.UserId == clerk && n.Kind == "new-order").Id;
            var open = await c.GetAsync($"/Notifications?handler=Open&id={id}&to=%2FRequisitions");
            Assert.Equal(HttpStatusCode.Redirect, open.StatusCode);
            Assert.Equal(1, app.Run(sp => sp.GetRequiredService<NotificationService>().UnreadCount(clerk)));
            await c.FormPost("/Notifications", "/Notifications?handler=ReadAll", new());
            Assert.Equal(0, app.Run(sp => sp.GetRequiredService<NotificationService>().UnreadCount(clerk)));

            // someone else's notification cannot be marked read
            app.Run(sp => { sp.GetRequiredService<NotificationService>().MarkRead(other, id); return 0; });
            Assert.False(app.Run(sp => sp.GetRequiredService<ApplicationDbContext>().Notifications.Single(n => n.Id == id).UserId == other));
        }

        [Fact]
        public void The_closing_reminder_is_sent_once_a_day_and_only_near_closing_time()
        {
            var app = new TestApp();
            app.SeedUser("admin", "pw", "Ada");
            var now = DateTime.Now;
            var end = now.AddMinutes(10);
            if (end.Date != now.Date) return;   // too close to midnight for this check
            app.SeedOpenWindow("00:00", end.ToString("HH:mm"));

            Assert.True(app.Run(sp => ClosingReminderService.Check(sp, now)));
            Assert.False(app.Run(sp => ClosingReminderService.Check(sp, now)));   // once per day
            Assert.Contains(app.Run(sp => sp.GetRequiredService<ApplicationDbContext>().Notifications.ToList()), n => n.Kind == "closing");

            var early = new TestApp();
            early.SeedUser("admin", "pw", "Ada");
            early.SeedOpenWindow("00:00", now.AddHours(3).ToString("HH:mm"));
            if (now.AddHours(3).Date == now.Date) Assert.False(early.Run(sp => ClosingReminderService.Check(sp, now)));   // 3 hours left: too early
        }

        // ---- #9 salesperson attribution ---------------------------------------------------------------------

        [Fact]
        public async Task Orders_are_attributed_to_the_person_who_took_them_and_shown_in_the_team_board()
        {
            var (app, userId, good) = Setup();
            app.SeedRole("Seller", Perm.RequisitionCreate, Perm.RequisitionView, Perm.RequisitionApprove, Perm.Reports);
            var sellerId = app.SeedUser("seller", "pw", "Sam", role: "Seller");
            var c = await Login(app, "seller");
            await c.FormPost("/Requisitions/Create", "/Requisitions/Create", new() { ["UserId"] = userId.ToString(), [$"Qty[{good}]"] = "4" });   // 100
            await c.FormPost("/Requisitions/Create", "/Requisitions/Create", new() { ["UserId"] = userId.ToString(), [$"Qty[{good}]"] = "2" });   // 50
            NewOrder(app, userId, good, 1);   // taken outside a request: not recorded
            Assert.All(app.Run(sp => sp.GetRequiredService<ApplicationDbContext>().SalesRequisitions.Where(r => r.Id <= 2).ToList()), r => Assert.Equal(sellerId, r.CreatedByUserId));

            Invoice(app, 1); Invoice(app, 2);
            app.Run(sp => sp.GetRequiredService<InvoiceService>().Collect(1, 100, "Cash"));

            var d = app.Run(sp => sp.GetRequiredService<AnalyticsService>().Build(Period.Parse("30")));
            var sam = d.Staff.Single(s => s.UserId == sellerId);
            Assert.Equal("Sam Lovelace", sam.Name);
            Assert.Equal(2, sam.Orders);
            Assert.Equal(150, sam.Billed);
            Assert.Equal(100, sam.Collected);
            Assert.Contains(d.Staff, s => s.UserId == 0 && s.Orders == 1);

            var page = await c.GetStringAsync("/Reports");
            Assert.Contains("Sales team", page);
            Assert.Contains("Sam Lovelace", page);
            Assert.DoesNotContain("Commission", page);   // off until Kpi:CommissionPercent is set
        }

        [Fact]
        public async Task New_pages_render_for_an_administrator()
        {
            var (app, userId, good) = Setup();
            var inv = Invoice(app, NewOrder(app, userId, good, 2));
            var c = await Login(app);
            foreach (var url in new[] { "/Audit", "/Notifications", $"/Users/Statement?id={userId}", $"/Invoices/Return?id={inv}", "/Requisitions?tab=cancelled" })
                Assert.Equal(HttpStatusCode.OK, (await c.GetAsync(url)).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync("/Invoices/Return?id=999")).StatusCode);
        }
    }
}
