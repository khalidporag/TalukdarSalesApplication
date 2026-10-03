using TalukdarSales.Web.Helpers;
using TalukdarSales.Web.Context;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;

namespace TalukdarSales.Web.Services
{
    public record RequisitionLine(int FinishedGoodId, double Quantity);

    public record RequisitionRow(int Id, string Serial, int UserId, string UserName, DateTime CreatedDateTime, bool IsActive);

    public record RequisitionDetailRow(int FinishedGoodId, string ProductName, double Quantity, double Price, DateTime CreatedDateTime)
    {
        public double Total => Quantity * Price;
    }

    public record OrderRow(int Id, string Serial, int UserId, string Customer, DateTime Created, bool IsActive,
        int Items, double Total, int? InvoiceId, string InvoiceNo, bool IsCancelled = false, string CancelReason = null, string TakenBy = null);

    public record OrderBoard(Paged<OrderRow> Page, int Waiting, int Invoiced, int Cancelled = 0);

    public record ProductionItem(int FinishedGoodId, string ProductName, string Category, double Quantity);
    public record ProductionPlan(List<ProductionItem> Items, int Orders, int Customers);

    public record ProductTotal(int FinishedGoodId, string ProductName, double TotalQuantity);

    public class RequisitionService
    {
        private readonly ApplicationDbContext _db;
        private readonly ISalesRequisitionRepository _requisitions;
        private readonly ISalesRequisitionDetailRepository _details;
        private readonly IFinishedGoodsRepository _goods;
        private readonly IUserRepository _users;
        private readonly ITimeSettingRepository _timeSettings;
        private readonly ISalesInvoiceRepository _invoices;
        private readonly CurrentUser _current;
        private readonly AuditService _audit;
        private readonly NotificationService _notify;

        public RequisitionService(ApplicationDbContext db, ISalesRequisitionRepository requisitions,
            ISalesRequisitionDetailRepository details, IFinishedGoodsRepository goods,
            IUserRepository users, ITimeSettingRepository timeSettings, ISalesInvoiceRepository invoices,
            CurrentUser current, AuditService audit, NotificationService notify)
        {
            _db = db;
            _requisitions = requisitions;
            _details = details;
            _goods = goods;
            _users = users;
            _timeSettings = timeSettings;
            _invoices = invoices;
            _current = current;
            _audit = audit;
            _notify = notify;
        }

        /// <summary>True when <paramref name="now"/> is inside the window; supports windows that cross midnight.</summary>
        public static bool IsWithinWindow(string fromTime, string toTime, TimeSpan now)
        {
            if (!TimeSpan.TryParse(fromTime, out var from) || !TimeSpan.TryParse(toTime, out var to))
                return false;
            return from <= to ? (from <= now && now <= to) : (now >= from || now <= to);
        }

        /// <summary>Orders still waiting for an invoice (drives the sidebar badge).</summary>
        public int WaitingCount() => _requisitions.GetAll().Count(r => r.IsActive);

        /// <summary>Orders for the board: tab is "waiting" (default), "invoiced" or "all". <paramref name="days"/> 0 means any date.</summary>
        public OrderBoard Board(string tab, string search, int? userTypeId, int days, int page, int pageSize, DateTime? from = null, DateTime? to = null)
        {
            var q = _requisitions.GetAll();
            if (from != null || to != null)
            {
                if (from != null) { var s0 = from.Value.Date; q = q.Where(r => r.CreatedDateTime >= s0); }
                if (to != null) { var e0 = to.Value.Date.AddDays(1); q = q.Where(r => r.CreatedDateTime < e0); }
            }
            else if (days > 0) { var since = DateTime.Today.AddDays(-(days - 1)); q = q.Where(r => r.CreatedDateTime >= since); }
            var users = _users.GetAll();
            if (userTypeId != null) q = q.Where(r => users.Any(u => u.Id == r.UserId && u.UserTypeId == userTypeId));
            if (!string.IsNullOrWhiteSpace(search))
            {
                var t = search.Trim().ToLower();
                q = q.Where(r => (r.RequisitionSerial != null && r.RequisitionSerial.ToLower().Contains(t)) ||
                    users.Any(u => u.Id == r.UserId && (u.FirstName + " " + u.LastName).ToLower().Contains(t)));
            }
            var waiting = q.Count(r => r.IsActive);
            var invoiced = q.Count(r => !r.IsActive && !r.IsCancelled);
            var cancelled = q.Count(r => r.IsCancelled);
            var shown = tab == "invoiced" ? q.Where(r => !r.IsActive && !r.IsCancelled) : tab == "cancelled" ? q.Where(r => r.IsCancelled)
                : tab == "all" ? q : q.Where(r => r.IsActive);
            shown = tab == "invoiced" || tab == "all" || tab == "cancelled" ? shown.OrderByDescending(r => r.CreatedDateTime).ThenByDescending(r => r.Id)
                : shown.OrderBy(r => r.CreatedDateTime).ThenBy(r => r.Id);
            var paged = Paged<SalesRequisition>.Create(shown, page, pageSize);

            var ids = paged.Items.Select(r => r.Id).ToList();
            var totals = _details.GetAll().Where(d => ids.Contains(d.SalesRequisitionId))
                .GroupBy(d => d.SalesRequisitionId).Select(g => new { Id = g.Key, Items = g.Count(), Total = g.Sum(d => d.Quantity * d.Price) })
                .ToList().ToDictionary(x => x.Id);
            var invs = _invoices.GetAll().Where(i => ids.Contains(i.SalesRequisitionId)).Select(i => new { i.Id, i.InvoiceSerialNo, i.SalesRequisitionId }).ToList()
                .GroupBy(i => i.SalesRequisitionId).ToDictionary(g => g.Key, g => g.First());
            var names = ToRows(paged.Items).ToDictionary(r => r.Id, r => r.UserName);
            var rows = paged.Items.Select(r =>
            {
                totals.TryGetValue(r.Id, out var t); invs.TryGetValue(r.Id, out var inv);
                return new OrderRow(r.Id, r.RequisitionSerial, r.UserId, names[r.Id], r.CreatedDateTime, r.IsActive,
                    t?.Items ?? 0, Money.Round(t?.Total ?? 0), inv?.Id, inv?.InvoiceSerialNo, r.IsCancelled, r.CancelReason);
            }).ToList();
            return new OrderBoard(new Paged<OrderRow> { Items = rows, Page = paged.Page, PageSize = paged.PageSize, Total = paged.Total }, waiting, invoiced, cancelled);
        }

        /// <summary>The day's quantities per product with their category, plus how many orders and customers they come from.</summary>
        public ProductionPlan PlanForDay(DateTime day, IFinishedGoodTypeRepository types)
        {
            var start = day.Date; var end = start.AddDays(1);
            var totals = ProductTotalsForDay(day);
            var ids = totals.Select(t => t.FinishedGoodId).ToList();
            var goodType = _goods.GetAll().Where(g => ids.Contains(g.Id)).Select(g => new { g.Id, g.GoodTypeId }).ToList().ToDictionary(g => g.Id, g => g.GoodTypeId);
            var typeName = types.GetAll().Select(t => new { t.Id, t.Name }).ToList().ToDictionary(t => t.Id, t => t.Name);
            var items = totals.Select(t => new ProductionItem(t.FinishedGoodId, t.ProductName,
                goodType.TryGetValue(t.FinishedGoodId, out var ty) && typeName.TryGetValue(ty, out var n) ? n : "Other", t.TotalQuantity)).ToList();
            var day_ = _requisitions.GetAll().Where(r => r.CreatedDateTime >= start && r.CreatedDateTime < end && !r.IsCancelled);
            return new ProductionPlan(items, day_.Count(), day_.Select(r => r.UserId).Distinct().Count());
        }

        public int OrderCountForDay(DateTime day)
        {
            var start = day.Date; var end = start.AddDays(1);
            return _requisitions.GetAll().Count(r => r.CreatedDateTime >= start && r.CreatedDateTime < end && !r.IsCancelled);
        }

        public (string From, string To) GetWindow()
        {
            var s = _timeSettings.GetAll().OrderByDescending(n => n.CreatedOn).FirstOrDefault();
            return (s?.From, s?.To);
        }

        public bool IsOpenNow()
        {
            var (from, to) = GetWindow();
            return IsWithinWindow(from, to, DateTime.Now.TimeOfDay);
        }

        /// <summary>Creates a requisition. Prices always come from the product catalogue, never from the caller.</summary>
        public (bool Ok, string Error, SalesRequisition Requisition) Create(int userId, IEnumerable<RequisitionLine> lines)
        {
            if (!IsOpenNow())
            {
                var (from, to) = GetWindow();
                return (false, $"Requisitions are closed right now. Allowed time: {from} - {to}.", null);
            }
            if (_users.GetSingle(userId) == null)
                return (false, "Please select a user.", null);

            var wanted = (lines ?? Enumerable.Empty<RequisitionLine>())
                .Where(l => l.Quantity > 0 && l.FinishedGoodId > 0)
                .GroupBy(l => l.FinishedGoodId)
                .Select(g => new RequisitionLine(g.Key, g.Sum(x => x.Quantity)))
                .ToList();
            if (wanted.Count == 0)
                return (false, "Enter a quantity for at least one product.", null);

            var wantedIds = wanted.Select(l => l.FinishedGoodId).ToList();
            var goods = _goods.GetAll().Where(g => g.IsActive && wantedIds.Contains(g.Id)).ToDictionary(g => g.Id);
            if (wanted.Any(l => !goods.ContainsKey(l.FinishedGoodId)))
                return (false, "One or more products are not available.", null);

            using var tx = _db.Database.BeginTransaction();
            var now = DateTime.Now;
            var requisition = new SalesRequisition { UserId = userId, CreatedDateTime = now, IsActive = true, CreatedByUserId = _current.Id };
            _requisitions.Add(requisition);
            _requisitions.Commit();

            requisition.RequisitionSerial = "REQ - " + requisition.Id.ToString("D6");
            _requisitions.Update(requisition);

            _details.AddRange(wanted.Select(l => new SalesRequisitionDetail
            {
                SalesRequisitionId = requisition.Id,
                CreatedDateTime = now,
                FinishedGoodId = l.FinishedGoodId,
                Quantity = l.Quantity,
                Price = goods[l.FinishedGoodId].UnitPrice
            }).ToList());
            _requisitions.Commit();
            tx.Commit();

            var user = _users.GetSingle(userId);
            var name = $"{user.FirstName} {user.LastName}".Trim();
            var total = Money.Round(wanted.Sum(l => l.Quantity * goods[l.FinishedGoodId].UnitPrice));
            _audit.Log("order.create", "Order", requisition.Id, $"{requisition.RequisitionSerial} for {name}, ৳ {total:0.##}");
            _notify.Notify(Security.Perm.RequisitionApprove, "new-order", $"New order {requisition.RequisitionSerial}", $"{name} · ৳ {total:0.##}", "/Requisitions?id=" + requisition.Id);
            if (user.MaxCreditLimit > 0 && user.DueAmount + total > user.MaxCreditLimit)
                _notify.Notify(Security.Perm.RequisitionApprove, "over-limit", $"Over credit limit: {name}",
                    $"{requisition.RequisitionSerial} takes them to ৳ {user.DueAmount + total:0.##} against a limit of ৳ {user.MaxCreditLimit:0.##}.", "/Requisitions?id=" + requisition.Id);
            return (true, null, requisition);
        }

        /// <summary>Replaces the lines of a waiting order. Products already on the order keep their price; new products use the catalogue price.</summary>
        public (bool Ok, string Error) Update(int id, IEnumerable<RequisitionLine> lines)
        {
            var r = _requisitions.GetSingle(id);
            if (r == null || !r.IsActive || r.IsCancelled)
                return (false, "Only waiting orders can be edited.");
            var wanted = (lines ?? Enumerable.Empty<RequisitionLine>()).Where(l => l.Quantity > 0 && l.FinishedGoodId > 0)
                .GroupBy(l => l.FinishedGoodId).Select(g => new RequisitionLine(g.Key, g.Sum(x => x.Quantity))).ToList();
            if (wanted.Count == 0)
                return (false, "Enter a quantity for at least one product, or cancel the order.");

            var old = _details.GetAll().Where(d => d.SalesRequisitionId == id).OrderBy(d => d.Id).ToList();
            var oldPrice = old.GroupBy(d => d.FinishedGoodId).ToDictionary(g => g.Key, g => g.First().Price);
            var ids = wanted.Select(l => l.FinishedGoodId).ToList();
            var goods = _goods.GetAll().Where(g => ids.Contains(g.Id)).ToDictionary(g => g.Id);
            if (wanted.Any(l => !oldPrice.ContainsKey(l.FinishedGoodId) && (!goods.TryGetValue(l.FinishedGoodId, out var g) || !g.IsActive)))
                return (false, "One or more products are not available.");

            var before = Money.Round(old.Sum(d => d.Quantity * d.Price));
            using var tx = _db.Database.BeginTransaction();
            foreach (var d in old) _details.Delete(d);
            var now = DateTime.Now;
            _details.AddRange(wanted.Select(l => new SalesRequisitionDetail
            {
                SalesRequisitionId = id, CreatedDateTime = r.CreatedDateTime, FinishedGoodId = l.FinishedGoodId, Quantity = l.Quantity,
                Price = oldPrice.TryGetValue(l.FinishedGoodId, out var p) ? p : goods[l.FinishedGoodId].UnitPrice
            }).ToList());
            _requisitions.Commit();
            tx.Commit();
            var after = Money.Round(wanted.Sum(l => l.Quantity * (oldPrice.TryGetValue(l.FinishedGoodId, out var p) ? p : goods[l.FinishedGoodId].UnitPrice)));
            _audit.Log("order.edit", "Order", id, $"{r.RequisitionSerial} edited: ৳ {before:0.##} to ৳ {after:0.##}");
            return (true, null);
        }

        public (bool Ok, string Error) Cancel(int id, string reason)
        {
            reason = (reason ?? "").Trim();
            if (reason.Length == 0) return (false, "Give a reason for cancelling.");
            var r = _requisitions.GetSingle(id);
            if (r == null || !r.IsActive || r.IsCancelled) return (false, "Only waiting orders can be cancelled.");
            r.IsActive = false; r.IsCancelled = true; r.CancelReason = reason.Length > 300 ? reason[..300] : reason;
            _requisitions.Update(r);
            _requisitions.Commit();
            _audit.Log("order.cancel", "Order", id, $"{r.RequisitionSerial} cancelled: {r.CancelReason}");
            return (true, null);
        }

        /// <summary>Filtered requisitions (SQL). Dates are inclusive days; <paramref name="userTypeId"/> filters by the requester's group.</summary>
        private IQueryable<SalesRequisition> Filter(bool? isActive, int? userId, int? userTypeId, string serial, DateTime? from, DateTime? to)
        {
            var q = _requisitions.GetAll();
            if (isActive != null) q = q.Where(r => r.IsActive == isActive);
            if (userId != null) q = q.Where(r => r.UserId == userId);
            if (userTypeId != null)
            {
                var users = _users.GetAll();
                q = q.Where(r => users.Any(u => u.Id == r.UserId && u.UserTypeId == userTypeId));
            }
            if (!string.IsNullOrWhiteSpace(serial))
            {
                var term = serial.Trim().ToLower();
                q = q.Where(r => r.RequisitionSerial != null && r.RequisitionSerial.ToLower().Contains(term));
            }
            if (from != null && to != null)
            {
                var start = from.Value.Date;
                var end = to.Value.Date.AddDays(1);
                q = q.Where(r => r.CreatedOn >= start && r.CreatedOn < end);
            }
            return q.OrderByDescending(r => r.CreatedDateTime).ThenByDescending(r => r.Id);
        }

        private List<RequisitionRow> ToRows(IEnumerable<SalesRequisition> items)
        {
            var list = items.ToList();
            var userIds = list.Select(r => r.UserId).Distinct().ToList();
            var users = _users.GetAll().Where(u => userIds.Contains(u.Id)).ToDictionary(u => u.Id);
            return list.Select(r => new RequisitionRow(r.Id, r.RequisitionSerial, r.UserId,
                users.TryGetValue(r.UserId, out var u) ? $"{u.FirstName} {u.LastName}".Trim() : "",
                r.CreatedDateTime, r.IsActive)).ToList();
        }

        public List<RequisitionRow> List(bool? isActive, int? userId, string serial, DateTime? from, DateTime? to) =>
            ToRows(Filter(isActive, userId, null, serial, from, to));

        /// <summary>One page of requisitions; counting and paging happen in SQL.</summary>
        public Paged<RequisitionRow> Page(bool? isActive, int? userId, int? userTypeId, string serial, DateTime? from, DateTime? to, int page, int pageSize)
        {
            var paged = Paged<SalesRequisition>.Create(Filter(isActive, userId, userTypeId, serial, from, to), page, pageSize);
            return new Paged<RequisitionRow> { Items = ToRows(paged.Items), Page = paged.Page, PageSize = paged.PageSize, Total = paged.Total };
        }

        public (RequisitionRow Header, List<RequisitionDetailRow> Lines)? Get(int id)
        {
            var r = _requisitions.GetSingle(id);
            if (r == null)
                return null;
            var header = ToRows(new[] { r }).Single();
            var details = _details.GetAll().Where(d => d.SalesRequisitionId == id).OrderBy(d => d.Id).ToList();
            var goodIds = details.Select(d => d.FinishedGoodId).Distinct().ToList();
            var goods = _goods.GetAll().Where(g => goodIds.Contains(g.Id)).ToDictionary(g => g.Id);
            var lines = details.Select(d => new RequisitionDetailRow(d.FinishedGoodId,
                goods.TryGetValue(d.FinishedGoodId, out var g) ? g.Name : "", d.Quantity, d.Price, d.CreatedDateTime)).ToList();
            return (header, lines);
        }

        /// <summary>Total quantity per product requisitioned on the given day (production planning).</summary>
        public List<ProductTotal> ProductTotalsForDay(DateTime day)
        {
            var start = day.Date;
            var end = start.AddDays(1);
            var cancelled = _requisitions.GetAll().Where(r => r.IsCancelled).Select(r => r.Id);
            var totals = _details.GetAll()
                .Where(d => d.CreatedDateTime >= start && d.CreatedDateTime < end && !cancelled.Contains(d.SalesRequisitionId))
                .GroupBy(d => d.FinishedGoodId)
                .Select(g => new { Id = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToList();
            var ids = totals.Select(t => t.Id).ToList();
            var goods = _goods.GetAll().Where(g => ids.Contains(g.Id)).ToDictionary(g => g.Id);
            return totals.Select(t => new ProductTotal(t.Id, goods.TryGetValue(t.Id, out var p) ? p.Name : "", t.Quantity))
                .OrderBy(p => p.ProductName).ToList();
        }
    }
}
