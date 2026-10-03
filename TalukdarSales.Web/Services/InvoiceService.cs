using TalukdarSales.Web.Context;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;

namespace TalukdarSales.Web.Services
{
    public record InvoiceLine(int FinishedGoodId, double Quantity);

    public record InvoiceRow(int Id, string Number, int RequisitionId, string RequisitionNo, int UserId, string UserName,
        DateTime CreatedDateTime, double Total, double Collected)
    {
        public double Due => Total - Collected;
    }

    public record InvoiceList(List<InvoiceRow> Rows)
    {
        public double Total => Rows.Sum(r => r.Total);
        public double Collected => Rows.Sum(r => r.Collected);
        public double Due => Total - Collected;
    }

    public record InvoiceLineView(string ProductName, double Quantity, double Price)
    {
        public double Total => Quantity * Price;
    }

    public record InvoiceDetail(InvoiceRow Header, string UserSequentialId, double Quantity, List<InvoiceLineView> Lines);

    public record CollectionRow(int Id, int InvoiceId, string InvoiceNumber, int UserId, double Amount, string PaymentMethod, DateTime Time);

    public class InvoiceService
    {
        private readonly ApplicationDbContext _db;
        private readonly ISalesInvoiceRepository _invoices;
        private readonly ISalesInvoiceDetailsRepository _invoiceDetails;
        private readonly ISalesRequisitionRepository _requisitions;
        private readonly ISalesRequisitionDetailRepository _requisitionDetails;
        private readonly ICollectionLedgerRepository _ledger;
        private readonly IUserRepository _users;
        private readonly IFinishedGoodsRepository _goods;

        public InvoiceService(ApplicationDbContext db, ISalesInvoiceRepository invoices,
            ISalesInvoiceDetailsRepository invoiceDetails, ISalesRequisitionRepository requisitions,
            ISalesRequisitionDetailRepository requisitionDetails, ICollectionLedgerRepository ledger,
            IUserRepository users, IFinishedGoodsRepository goods)
        {
            _db = db;
            _invoices = invoices;
            _invoiceDetails = invoiceDetails;
            _requisitions = requisitions;
            _requisitionDetails = requisitionDetails;
            _ledger = ledger;
            _users = users;
            _goods = goods;
        }

        /// <summary>Approve active requisitions: one invoice per requisition. All-or-nothing.</summary>
        public (bool Ok, string Error, List<SalesInvoice> Invoices) CreateFromRequisitions(IEnumerable<int> requisitionIds)
        {
            var ids = requisitionIds.Distinct().ToList();
            var requisitions = _requisitions.GetAll().Where(r => r.IsActive && ids.Contains(r.Id)).ToList();
            if (requisitions.Count == 0)
                return (false, "No active requisitions selected.", null);

            using var tx = _db.Database.BeginTransaction();
            var created = new List<SalesInvoice>();
            foreach (var r in requisitions)
            {
                var lines = _requisitionDetails.GetAll().Where(d => d.SalesRequisitionId == r.Id)
                    .Select(d => (d.FinishedGoodId, d.Quantity, d.Price)).ToList();
                var invoice = Build(r, lines);
                if (invoice == null)
                    return (false, $"User for {r.RequisitionSerial} was not found.", null);
                created.Add(invoice);
            }
            tx.Commit();
            return (true, null, created);
        }

        /// <summary>Manual invoice for one requisition with adjusted quantities. Prices come from the requisition.</summary>
        public (bool Ok, string Error, SalesInvoice Invoice) CreateForRequisition(int requisitionId, IEnumerable<InvoiceLine> lines)
        {
            var requisition = _requisitions.GetSingle(requisitionId);
            if (requisition == null || !requisition.IsActive)
                return (false, "Requisition not found or already invoiced.", null);

            var prices = _requisitionDetails.GetAll().Where(d => d.SalesRequisitionId == requisitionId)
                .GroupBy(d => d.FinishedGoodId).ToDictionary(g => g.Key, g => g.First().Price);
            var picked = (lines ?? Enumerable.Empty<InvoiceLine>())
                .Where(l => l.Quantity > 0 && prices.ContainsKey(l.FinishedGoodId))
                .Select(l => (l.FinishedGoodId, l.Quantity, prices[l.FinishedGoodId])).ToList();
            if (picked.Count == 0)
                return (false, "Enter a quantity for at least one product.", null);

            using var tx = _db.Database.BeginTransaction();
            var invoice = Build(requisition, picked);
            if (invoice == null)
                return (false, "User not found.", null);
            tx.Commit();
            return (true, null, invoice);
        }

        // Must run inside a transaction. Adds the invoice total to the user's due amount.
        private SalesInvoice Build(SalesRequisition requisition, List<(int GoodId, double Qty, double Price)> lines)
        {
            var user = _users.GetSingle(requisition.UserId);
            if (user == null)
                return null;

            var now = DateTime.Now;
            var invoice = new SalesInvoice
            {
                UserId = requisition.UserId,
                SalesRequisitionId = requisition.Id,
                Quantity = lines.Sum(l => l.Qty),
                TotalPrice = lines.Sum(l => l.Qty * l.Price),
                CreatedDateTime = now
            };
            _invoices.Add(invoice);
            _invoices.Commit();

            invoice.InvoiceSerialNo = "INV - " + invoice.Id.ToString("D6");
            _invoices.Update(invoice);

            _invoiceDetails.AddRange(lines.Select(l => new SalesInvoiceDetails
            {
                SalesInvoiceId = invoice.Id,
                FinishedGoodsId = l.GoodId,
                CreatedDateTime = now,
                Quantity = l.Qty,
                Price = l.Price
            }).ToList());

            user.DueAmount += (decimal)invoice.TotalPrice;
            _users.Update(user);

            requisition.IsActive = false;
            _requisitions.Update(requisition);

            _invoices.Commit();
            return invoice;
        }

        public InvoiceList List(int? userId, DateTime? from, DateTime? to)
        {
            var users = _users.GetAll().ToDictionary(u => u.Id);
            var requisitions = _requisitions.GetAll().ToDictionary(r => r.Id);
            IEnumerable<SalesInvoice> q = _invoices.GetAll();
            if (userId != null) q = q.Where(i => i.UserId == userId);
            if (from != null && to != null)
            {
                var end = to.Value.Date.AddDays(1);
                q = q.Where(i => i.CreatedDateTime >= from.Value.Date && i.CreatedDateTime < end);
            }
            return new InvoiceList(q.OrderByDescending(i => i.CreatedDateTime).Select(i => ToRow(i, users, requisitions)).ToList());
        }

        public InvoiceDetail Get(int id)
        {
            var invoice = _invoices.GetSingle(id);
            if (invoice == null)
                return null;
            var users = _users.GetAll().ToDictionary(u => u.Id);
            var requisitions = _requisitions.GetAll().ToDictionary(r => r.Id);
            var goods = _goods.GetAll().ToDictionary(g => g.Id);
            var lines = _invoiceDetails.GetAll().Where(d => d.SalesInvoiceId == id)
                .Select(d => new InvoiceLineView(goods.TryGetValue(d.FinishedGoodsId, out var g) ? g.Name : "", d.Quantity, d.Price))
                .ToList();
            return new InvoiceDetail(ToRow(invoice, users, requisitions),
                users.TryGetValue(invoice.UserId, out var u) ? u.SequencialUserId : "", invoice.Quantity, lines);
        }

        private static InvoiceRow ToRow(SalesInvoice i, Dictionary<int, User> users, Dictionary<int, SalesRequisition> reqs) =>
            new(i.Id, i.InvoiceSerialNo, i.SalesRequisitionId,
                reqs.TryGetValue(i.SalesRequisitionId, out var r) ? r.RequisitionSerial : "",
                i.UserId, users.TryGetValue(i.UserId, out var u) ? $"{u.FirstName} {u.LastName}".Trim() : "",
                i.CreatedDateTime, i.TotalPrice, i.CollectionAmount);

        /// <summary>
        /// Apply a payment to the billed user's invoices, oldest first. The invoice id picks the user.
        /// Rejects zero/negative amounts and overpayment. All-or-nothing.
        /// </summary>
        public (bool Ok, string Error) Collect(int invoiceId, double amount, string paymentMethod)
        {
            if (amount <= 0)
                return (false, "Invalid collection amount.");
            var invoice = _invoices.GetSingle(invoiceId);
            if (invoice == null)
                return (false, "Invoice not found.");
            var user = _users.GetSingle(invoice.UserId);
            if (user == null)
                return (false, "User not found.");

            var outstanding = _invoices.GetAll()
                .Where(i => i.UserId == invoice.UserId && i.TotalPrice > i.CollectionAmount)
                .OrderBy(i => i.CreatedDateTime).ThenBy(i => i.Id).ToList();
            var totalOutstanding = outstanding.Sum(i => i.TotalPrice - i.CollectionAmount);
            if (amount > totalOutstanding + 0.005)
                return (false, "Collection amount exceeds the outstanding amount.");

            using var tx = _db.Database.BeginTransaction();
            var remaining = amount;
            var collected = 0.0;
            foreach (var inv in outstanding)
            {
                if (remaining <= 0.000001)
                    break;
                var applied = Math.Min(remaining, inv.TotalPrice - inv.CollectionAmount);
                inv.CollectionAmount += applied;
                _invoices.Update(inv);
                _ledger.Add(new CollectionLedger
                {
                    UserId = inv.UserId,
                    SalesInvoiceId = inv.Id,
                    CollectionAmount = applied,
                    PaymentMethod = paymentMethod
                });
                remaining -= applied;
                collected += applied;
            }
            user.DueAmount -= (decimal)collected;
            _users.Update(user);
            _invoices.Commit();
            tx.Commit();
            return (true, null);
        }

        public (List<CollectionRow> Rows, double Total) CollectionHistory(int? userId, int? invoiceId, DateTime? from, DateTime? to)
        {
            var invoices = _invoices.GetAll().ToDictionary(i => i.Id);
            IEnumerable<CollectionLedger> q = _ledger.GetAll();
            if (userId != null) q = q.Where(c => c.UserId == userId);
            if (invoiceId != null) q = q.Where(c => c.SalesInvoiceId == invoiceId);
            if (from != null && to != null)
            {
                var end = to.Value.Date.AddDays(1);
                q = q.Where(c => c.CreatedOn >= from.Value.Date && c.CreatedOn < end);
            }
            var rows = q.OrderByDescending(c => c.CreatedOn)
                .Select(c => new CollectionRow(c.Id, c.SalesInvoiceId,
                    invoices.TryGetValue(c.SalesInvoiceId, out var i) ? i.InvoiceSerialNo : "",
                    c.UserId, c.CollectionAmount, c.PaymentMethod, c.CreatedOn)).ToList();
            return (rows, rows.Where(r => r.Amount > 0).Sum(r => r.Amount));
        }
    }
}
