using Microsoft.Extensions.DependencyInjection;
using TalukdarSales.Web.Context;
using TalukdarSales.Web.Services;
using Xunit;

namespace TalukdarSales.Tests
{
    public class SalesServiceTests
    {
        // each test gets its own app/database
        private static (TestApp app, int userId, int goodId) Setup(double price = 10)
        {
            var app = new TestApp();
            var userId = app.SeedUser();
            var goodId = app.SeedGood("Biscuit", price);
            app.SeedOpenWindow();
            return (app, userId, goodId);
        }

        private static int Requisition(TestApp app, int userId, int goodId, double qty) =>
            app.Run(sp => sp.GetRequiredService<RequisitionService>().Create(userId, new[] { new RequisitionLine(goodId, qty) })).Requisition.Id;

        private static int Invoice(TestApp app, int requisitionId) =>
            app.Run(sp => sp.GetRequiredService<InvoiceService>().CreateFromRequisitions(new[] { requisitionId })).Invoices[0].Id;

        private static double Due(TestApp app, int userId) =>
            app.Run(sp => sp.GetRequiredService<ApplicationDbContext>().Users.Single(u => u.Id == userId).DueAmount);

        [Theory]
        [InlineData("08:00", "17:00", "09:00", true)]
        [InlineData("08:00", "17:00", "17:01", false)]
        [InlineData("22:00", "02:00", "23:30", true)]   // crosses midnight
        [InlineData("22:00", "02:00", "01:00", true)]
        [InlineData("22:00", "02:00", "12:00", false)]
        [InlineData(null, null, "12:00", false)]
        public void Window_check(string from, string to, string now, bool expected) =>
            Assert.Equal(expected, RequisitionService.IsWithinWindow(from, to, TimeSpan.Parse(now)));

        [Fact]
        public void Requisition_uses_catalogue_price_and_rejects_closed_window_and_empty()
        {
            var (app, userId, goodId) = Setup(price: 12.5);
            var created = app.Run(sp => sp.GetRequiredService<RequisitionService>().Create(userId, new[] { new RequisitionLine(goodId, 4) }));
            Assert.True(created.Ok);
            Assert.Equal("REQ - " + created.Requisition.Id.ToString("D6"), created.Requisition.RequisitionSerial);
            var lines = app.Run(sp => sp.GetRequiredService<RequisitionService>().Get(created.Requisition.Id)).Value.Lines;
            Assert.Equal(12.5, lines.Single().Price);
            Assert.Equal(50, lines.Single().Total);

            var empty = app.Run(sp => sp.GetRequiredService<RequisitionService>().Create(userId, new[] { new RequisitionLine(goodId, 0) }));
            Assert.False(empty.Ok);

            var inactive = app.SeedGood("Old", 5, active: false);
            Assert.False(app.Run(sp => sp.GetRequiredService<RequisitionService>().Create(userId, new[] { new RequisitionLine(inactive, 1) })).Ok);
        }

        [Fact]
        public void Requisition_rejected_outside_window()
        {
            var app = new TestApp();
            var userId = app.SeedUser();
            var goodId = app.SeedGood("Biscuit", 10);
            // a window that excludes the current time of day
            var now = DateTime.Now;
            var from = now.AddHours(2).ToString("HH:mm");
            var to = now.AddHours(3).ToString("HH:mm");
            if (RequisitionService.IsWithinWindow(from, to, now.TimeOfDay)) { from = "00:00"; to = "00:00"; }
            app.SeedOpenWindow(from, to);
            var res = app.Run(sp => sp.GetRequiredService<RequisitionService>().Create(userId, new[] { new RequisitionLine(goodId, 1) }));
            Assert.False(res.Ok);
            Assert.Contains("closed", res.Error);
        }

        [Fact]
        public void Invoices_accumulate_the_users_due_amount()
        {
            var (app, userId, goodId) = Setup(10);
            Invoice(app, Requisition(app, userId, goodId, 10));   // 100
            Assert.Equal(100, Due(app, userId));
            Invoice(app, Requisition(app, userId, goodId, 4));    // + 40
            Assert.Equal(140, Due(app, userId));
        }

        [Fact]
        public void Bulk_approval_skips_already_invoiced_requisitions()
        {
            var (app, userId, goodId) = Setup(10);
            var r1 = Requisition(app, userId, goodId, 1);
            Invoice(app, r1);
            var again = app.Run(sp => sp.GetRequiredService<InvoiceService>().CreateFromRequisitions(new[] { r1 }));
            Assert.False(again.Ok);
            Assert.Equal(10, Due(app, userId));
        }

        [Fact]
        public void Partial_collection_is_not_double_counted()
        {
            var (app, userId, goodId) = Setup(10);
            var inv = Invoice(app, Requisition(app, userId, goodId, 10));   // 100

            Assert.True(app.Run(sp => sp.GetRequiredService<InvoiceService>().Collect(inv, 30, "Cash")).Ok);
            var svc = (InvoiceService s) => s.Get(inv).Header;
            var after = app.Run(sp => svc(sp.GetRequiredService<InvoiceService>()));
            Assert.Equal(30, after.Collected);
            Assert.Equal(70, after.Due);
            Assert.Equal(70, Due(app, userId));
            var (rows, total) = app.Run(sp => sp.GetRequiredService<InvoiceService>().CollectionHistory(null, inv, null, null));
            Assert.Single(rows);
            Assert.Equal(30, total);

            Assert.True(app.Run(sp => sp.GetRequiredService<InvoiceService>().Collect(inv, 70, "Cash")).Ok);
            Assert.Equal(0, Due(app, userId));
            Assert.Equal(0, app.Run(sp => sp.GetRequiredService<InvoiceService>().Get(inv)).Header.Due);
        }

        [Fact]
        public void Collection_is_applied_oldest_invoice_first_across_invoices()
        {
            var (app, userId, goodId) = Setup(10);
            var first = Invoice(app, Requisition(app, userId, goodId, 5));    // 50
            var second = Invoice(app, Requisition(app, userId, goodId, 8));   // 80

            // paying against the NEWER invoice still clears the oldest first
            Assert.True(app.Run(sp => sp.GetRequiredService<InvoiceService>().Collect(second, 100, "Bank")).Ok);

            var list = app.Run(sp => sp.GetRequiredService<InvoiceService>().List(userId, null, null));
            Assert.Equal(50, list.Rows.Single(r => r.Id == first).Collected);
            Assert.Equal(50, list.Rows.Single(r => r.Id == second).Collected);
            Assert.Equal(30, Due(app, userId));
            var (rows, total) = app.Run(sp => sp.GetRequiredService<InvoiceService>().CollectionHistory(userId, null, null, null));
            Assert.Equal(2, rows.Count);
            Assert.Equal(100, total);
        }

        [Fact]
        public void Overpayment_and_non_positive_amounts_are_rejected_without_side_effects()
        {
            var (app, userId, goodId) = Setup(10);
            var inv = Invoice(app, Requisition(app, userId, goodId, 10));   // 100

            Assert.False(app.Run(sp => sp.GetRequiredService<InvoiceService>().Collect(inv, 100.5, "Cash")).Ok);
            Assert.False(app.Run(sp => sp.GetRequiredService<InvoiceService>().Collect(inv, 0, "Cash")).Ok);
            Assert.False(app.Run(sp => sp.GetRequiredService<InvoiceService>().Collect(inv, -5, "Cash")).Ok);
            Assert.False(app.Run(sp => sp.GetRequiredService<InvoiceService>().Collect(9999, 10, "Cash")).Ok);

            Assert.Equal(100, Due(app, userId));
            Assert.Empty(app.Run(sp => sp.GetRequiredService<InvoiceService>().CollectionHistory(null, null, null, null)).Rows);
        }

        [Fact]
        public void Manual_invoice_uses_requisition_price_and_closes_the_requisition()
        {
            var (app, userId, goodId) = Setup(20);
            var req = Requisition(app, userId, goodId, 10);

            var made = app.Run(sp => sp.GetRequiredService<InvoiceService>().CreateForRequisition(req, new[] { new InvoiceLine(goodId, 6) }));
            Assert.True(made.Ok);
            Assert.Equal(120, made.Invoice.TotalPrice);       // 6 x 20, not the requisition's 10
            Assert.Equal(120, Due(app, userId));

            var again = app.Run(sp => sp.GetRequiredService<InvoiceService>().CreateForRequisition(req, new[] { new InvoiceLine(goodId, 1) }));
            Assert.False(again.Ok);

            var detail = app.Run(sp => sp.GetRequiredService<InvoiceService>().Get(made.Invoice.Id));
            Assert.Equal("Biscuit", detail.Lines.Single().ProductName);
            Assert.Equal(6, detail.Quantity);
        }

        [Fact]
        public void Money_stays_exact_to_two_decimals_with_floating_point_unfriendly_amounts()
        {
            var (app, userId, goodId) = Setup(0.1);
            var inv = Invoice(app, Requisition(app, userId, goodId, 3));   // 3 x 0.1 = 0.30000000000000004 in raw double math
            Assert.Equal(0.3, app.Run(sp => sp.GetRequiredService<InvoiceService>().Get(inv)).Header.Total);
            Assert.Equal(0.3, Due(app, userId));

            // three payments of 0.1 settle it exactly: no 5E-17 dust left on the invoice or the customer
            for (var i = 0; i < 3; i++)
                Assert.True(app.Run(sp => sp.GetRequiredService<InvoiceService>().Collect(inv, 0.1, "Cash")).Ok);
            var detail = app.Run(sp => sp.GetRequiredService<InvoiceService>().Get(inv));
            Assert.Equal(0.3, detail.Header.Collected);
            Assert.Equal(0, detail.Header.Due);
            Assert.Equal(0, Due(app, userId));
            Assert.False(app.Run(sp => sp.GetRequiredService<InvoiceService>().Collect(inv, 0.01, "Cash")).Ok);   // nothing left to collect
        }

        [Fact]
        public void Invoice_totals_round_to_paisa_and_user_credit_limit_is_a_plain_double()
        {
            var (app, userId, goodId) = Setup(33.333);
            var inv = Invoice(app, Requisition(app, userId, goodId, 3));   // 99.999 -> 100.00
            Assert.Equal(100.0, app.Run(sp => sp.GetRequiredService<InvoiceService>().Get(inv)).Header.Total);
            Assert.Equal(100.0, Due(app, userId));

            Assert.True(app.Run(sp => sp.GetRequiredService<UserService>().Update(new TalukdarSales.Web.Models.Dto.UpdateUserDto { Id = userId, FirstName = "Ada", MaxCreditLimit = 12345.67 })));
            Assert.Equal(12345.67, app.Run(sp => sp.GetRequiredService<ApplicationDbContext>().Users.Single(u => u.Id == userId).MaxCreditLimit));
        }

        [Fact]
        public void Production_totals_group_by_product_for_the_day()
        {
            var (app, userId, goodId) = Setup(10);
            Requisition(app, userId, goodId, 3);
            Requisition(app, userId, goodId, 4);
            var totals = app.Run(sp => sp.GetRequiredService<RequisitionService>().ProductTotalsForDay(DateTime.Today));
            Assert.Equal(7, totals.Single().TotalQuantity);
        }
    }
}
