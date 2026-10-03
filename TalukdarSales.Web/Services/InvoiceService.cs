using TalukdarSales.Web.Context;
using TalukdarSales.Web.Helpers;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;

namespace TalukdarSales.Web.Services
{
    public record InvoiceLine(int FinishedGoodId, double Quantity);

    internal record InvoiceBuildLine(int FinishedGoodId, double Quantity, double Price);

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

    public record InvoiceBoard(Paged<InvoiceRow> Page, double Billed, double Collected,
        int All, int Unpaid, int Partial, int Paid)
    {
        public double Due => Billed - Collected;
    }

    public record OpenInvoice(int Id, string Number, DateTime Created, double Due);

    public record CollectionRow(int Id, int InvoiceId, string InvoiceNumber, int UserId, double Amount, string PaymentMethod, DateTime Time, string UserName = "");

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

            var reqIds = requisitions.Select(r => r.Id).ToList();
            var linesByRequisition = _requisitionDetails.GetAll().Where(d => reqIds.Contains(d.SalesRequisitionId)).OrderBy(d => d.Id).ToList()
                .GroupBy(d => d.SalesRequisitionId).ToDictionary(g => g.Key, g => g.ToList());

            using var tx = _db.Database.BeginTransaction();
            var created = new List<SalesInvoice>();
            foreach (var r in requisitions)
            {
                var lines = (linesByRequisition.TryGetValue(r.Id, out var rows) ? rows : new List<SalesRequisitionDetail>())
                    .Select(d => new InvoiceBuildLine(d.FinishedGoodId, d.Quantity, d.Price)).ToList();
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

            var prices = _requisitionDetails.GetAll().Where(d => d.SalesRequisitionId == requisitionId).OrderBy(d => d.Id).ToList()
                .GroupBy(d => d.FinishedGoodId).ToDictionary(g => g.Key, g => g.First().Price);
            var picked = (lines ?? Enumerable.Empty<InvoiceLine>())
                .Where(l => l.Quantity > 0 && prices.ContainsKey(l.FinishedGoodId))
                .Select(l => new InvoiceBuildLine(l.FinishedGoodId, l.Quantity, prices[l.FinishedGoodId])).ToList();
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
        private SalesInvoice Build(SalesRequisition requisition, List<InvoiceBuildLine> lines)
        {
            var user = _users.GetSingle(requisition.UserId);
            if (user == null)
                return null;

            var now = DateTime.Now;
            var invoice = new SalesInvoice
            {
                UserId = requisition.UserId,
                SalesRequisitionId = requisition.Id,
                Quantity = lines.Sum(l => l.Quantity),
                TotalPrice = Money.Round(lines.Sum(l => l.Quantity * l.Price)),
                CreatedDateTime = now
            };
            _invoices.Add(invoice);
            _invoices.Commit();

            invoice.InvoiceSerialNo = "INV - " + invoice.Id.ToString("D6");
            _invoices.Update(invoice);

            _invoiceDetails.AddRange(lines.Select(l => new SalesInvoiceDetails
            {
                SalesInvoiceId = invoice.Id,
                FinishedGoodsId = l.FinishedGoodId,
                CreatedDateTime = now,
                Quantity = l.Quantity,
                Price = l.Price
            }).ToList());

            user.DueAmount = Money.Round(user.DueAmount + invoice.TotalPrice);
            _users.Update(user);

            requisition.IsActive = false;
            _requisitions.Update(requisition);

            _invoices.Commit();
            return invoice;
        }

        private List<InvoiceRow> ToRows(IEnumerable<SalesInvoice> items)
        {
            var list = items.ToList();
            var userIds = list.Select(i => i.UserId).Distinct().ToList();
            var reqIds = list.Select(i => i.SalesRequisitionId).Distinct().ToList();
            var users = _users.GetAll().Where(u => userIds.Contains(u.Id)).ToDictionary(u => u.Id);
            var reqs = _requisitions.GetAll().Where(r => reqIds.Contains(r.Id)).ToDictionary(r => r.Id);
            return list.Select(i => ToRow(i, users, reqs)).ToList();
        }

        public InvoiceList List(int? userId, DateTime? from, DateTime? to)
        {
            var q = _invoices.GetAll();
            if (userId != null) q = q.Where(i => i.UserId == userId);
            if (from != null && to != null)
            {
                var start = from.Value.Date;
                var end = to.Value.Date.AddDays(1);
                q = q.Where(i => i.CreatedDateTime >= start && i.CreatedDateTime < end);
            }
            return new InvoiceList(ToRows(q.OrderByDescending(i => i.CreatedDateTime).ThenByDescending(i => i.Id)));
        }

        /// <summary>Invoices for the list: status is all, unpaid, partial or paid; days 0 means any date.</summary>
        public InvoiceBoard Board(string status, string search, int days, int page, int pageSize)
        {
            var q = _invoices.GetAll();
            if (days > 0) { var since = DateTime.Today.AddDays(-(days - 1)); q = q.Where(i => i.CreatedDateTime >= since); }
            if (!string.IsNullOrWhiteSpace(search))
            {
                var t = search.Trim().ToLower();
                var users = _users.GetAll();
                q = q.Where(i => (i.InvoiceSerialNo != null && i.InvoiceSerialNo.ToLower().Contains(t)) ||
                    users.Any(u => u.Id == i.UserId && (u.FirstName + " " + u.LastName).ToLower().Contains(t)));
            }
            var billed = q.Sum(i => (double?)i.TotalPrice) ?? 0;
            var collected = q.Sum(i => (double?)i.CollectionAmount) ?? 0;
            var unpaid = q.Count(i => i.CollectionAmount <= 0.005);
            var paid = q.Count(i => i.TotalPrice - i.CollectionAmount <= 0.005 && i.CollectionAmount > 0.005);
            var all = q.Count();
            var shown = status switch
            {
                "unpaid" => q.Where(i => i.CollectionAmount <= 0.005),
                "partial" => q.Where(i => i.CollectionAmount > 0.005 && i.TotalPrice - i.CollectionAmount > 0.005),
                "paid" => q.Where(i => i.TotalPrice - i.CollectionAmount <= 0.005 && i.CollectionAmount > 0.005),
                "due" => q.Where(i => i.TotalPrice - i.CollectionAmount > 0.005),
                _ => q
            };
            var paged = Paged<SalesInvoice>.Create(shown.OrderByDescending(i => i.CreatedDateTime).ThenByDescending(i => i.Id), page, pageSize);
            var board = new Paged<InvoiceRow> { Items = ToRows(paged.Items), Page = paged.Page, PageSize = paged.PageSize, Total = paged.Total };
            return new InvoiceBoard(board, billed, collected, all, unpaid, all - unpaid - paid, paid);
        }

        /// <summary>Invoices the customer still owes on, oldest first (the order a payment clears them).</summary>
        public List<OpenInvoice> OpenInvoices(int userId) =>
            _invoices.GetAll().Where(i => i.UserId == userId && i.TotalPrice - i.CollectionAmount > 0.005)
                .OrderBy(i => i.CreatedDateTime).ThenBy(i => i.Id).ToList()
                .Select(i => new OpenInvoice(i.Id, i.InvoiceSerialNo, i.CreatedDateTime, Money.Round(i.TotalPrice - i.CollectionAmount))).ToList();

        /// <summary>What the customer still owes across all invoices (summed in SQL).</summary>
        public double OutstandingFor(int userId) =>
            _invoices.GetAll().Where(i => i.UserId == userId).Sum(i => i.TotalPrice - i.CollectionAmount);

        public InvoiceDetail Get(int id)
        {
            var invoice = _invoices.GetSingle(id);
            if (invoice == null)
                return null;
            var header = ToRows(new[] { invoice }).Single();
            var user = _users.GetSingle(invoice.UserId);
            var details = _invoiceDetails.GetAll().Where(d => d.SalesInvoiceId == id).OrderBy(d => d.Id).ToList();
            var goodIds = details.Select(d => d.FinishedGoodsId).Distinct().ToList();
            var goods = _goods.GetAll().Where(g => goodIds.Contains(g.Id)).ToDictionary(g => g.Id);
            var lines = details.Select(d => new InvoiceLineView(goods.TryGetValue(d.FinishedGoodsId, out var g) ? g.Name : "", d.Quantity, d.Price)).ToList();
            return new InvoiceDetail(header, user?.SequencialUserId ?? "", invoice.Quantity, lines);
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
            amount = Money.Round(amount);
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
            var totalOutstanding = Money.Round(outstanding.Sum(i => i.TotalPrice - i.CollectionAmount));
            if (amount > totalOutstanding + 0.005)
                return (false, "Collection amount exceeds the outstanding amount.");

            using var tx = _db.Database.BeginTransaction();
            var remaining = amount;
            var collected = 0.0;
            foreach (var inv in outstanding)
            {
                if (remaining <= 0.000001)
                    break;
                var applied = Money.Round(Math.Min(remaining, inv.TotalPrice - inv.CollectionAmount));
                inv.CollectionAmount = Money.Round(inv.CollectionAmount + applied);
                _invoices.Update(inv);
                _ledger.Add(new CollectionLedger
                {
                    UserId = inv.UserId,
                    SalesInvoiceId = inv.Id,
                    CollectionAmount = applied,
                    PaymentMethod = paymentMethod
                });
                remaining = Money.Round(remaining - applied);
                collected = Money.Round(collected + applied);
            }
            user.DueAmount = Money.Round(user.DueAmount - collected);
            _users.Update(user);
            _invoices.Commit();
            tx.Commit();
            return (true, null);
        }

        public (List<CollectionRow> Rows, double Total) CollectionHistory(int? userId, int? invoiceId, DateTime? from, DateTime? to)
        {
            var q = _ledger.GetAll();
            if (userId != null) q = q.Where(c => c.UserId == userId);
            if (invoiceId != null) q = q.Where(c => c.SalesInvoiceId == invoiceId);
            if (from != null && to != null)
            {
                var start = from.Value.Date;
                var end = to.Value.Date.AddDays(1);
                q = q.Where(c => c.CreatedOn >= start && c.CreatedOn < end);
            }
            var items = q.OrderByDescending(c => c.CreatedOn).ThenByDescending(c => c.Id).ToList();
            var invoiceIds = items.Select(c => c.SalesInvoiceId).Distinct().ToList();
            var invoices = _invoices.GetAll().Where(i => invoiceIds.Contains(i.Id)).ToDictionary(i => i.Id);
            var userIds = items.Select(c => c.UserId).Distinct().ToList();
            var names = _users.GetAll().Where(u => userIds.Contains(u.Id)).Select(u => new { u.Id, u.FirstName, u.LastName }).ToList()
                .ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim());
            var rows = items.Select(c => new CollectionRow(c.Id, c.SalesInvoiceId,
                invoices.TryGetValue(c.SalesInvoiceId, out var i) ? i.InvoiceSerialNo : "",
                c.UserId, c.CollectionAmount, c.PaymentMethod, c.CreatedOn, names.TryGetValue(c.UserId, out var n) ? n : "")).ToList();
            return (rows, rows.Where(r => r.Amount > 0).Sum(r => r.Amount));
        }
    }
}
