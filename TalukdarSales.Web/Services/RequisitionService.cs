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

    public record ProductTotal(int FinishedGoodId, string ProductName, double TotalQuantity);

    public class RequisitionService
    {
        private readonly ApplicationDbContext _db;
        private readonly ISalesRequisitionRepository _requisitions;
        private readonly ISalesRequisitionDetailRepository _details;
        private readonly IFinishedGoodsRepository _goods;
        private readonly IUserRepository _users;
        private readonly ITimeSettingRepository _timeSettings;

        public RequisitionService(ApplicationDbContext db, ISalesRequisitionRepository requisitions,
            ISalesRequisitionDetailRepository details, IFinishedGoodsRepository goods,
            IUserRepository users, ITimeSettingRepository timeSettings)
        {
            _db = db;
            _requisitions = requisitions;
            _details = details;
            _goods = goods;
            _users = users;
            _timeSettings = timeSettings;
        }

        /// <summary>True when <paramref name="now"/> is inside the window; supports windows that cross midnight.</summary>
        public static bool IsWithinWindow(string fromTime, string toTime, TimeSpan now)
        {
            if (!TimeSpan.TryParse(fromTime, out var from) || !TimeSpan.TryParse(toTime, out var to))
                return false;
            return from <= to ? (from <= now && now <= to) : (now >= from || now <= to);
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
            var requisition = new SalesRequisition { UserId = userId, CreatedDateTime = now, IsActive = true };
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
            return (true, null, requisition);
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
            var totals = _details.GetAll()
                .Where(d => d.CreatedDateTime >= start && d.CreatedDateTime < end)
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
