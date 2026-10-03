using TalukdarSales.Web.Context;
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

            var goods = _goods.GetAll().Where(g => g.IsActive).ToDictionary(g => g.Id);
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

        public List<RequisitionRow> List(bool? isActive, int? userId, string serial, DateTime? from, DateTime? to)
        {
            var users = _users.GetAll().ToDictionary(u => u.Id);
            IEnumerable<SalesRequisition> q = _requisitions.GetAll();
            if (isActive != null) q = q.Where(r => r.IsActive == isActive);
            if (userId != null) q = q.Where(r => r.UserId == userId);
            if (!string.IsNullOrWhiteSpace(serial))
                q = q.Where(r => (r.RequisitionSerial ?? "").Contains(serial.Trim(), StringComparison.OrdinalIgnoreCase));
            if (from != null && to != null)
            {
                var end = to.Value.Date.AddDays(1);
                q = q.Where(r => r.CreatedOn >= from.Value.Date && r.CreatedOn < end);
            }
            return q.OrderByDescending(r => r.CreatedDateTime)
                .Select(r => new RequisitionRow(r.Id, r.RequisitionSerial, r.UserId,
                    users.TryGetValue(r.UserId, out var u) ? $"{u.FirstName} {u.LastName}".Trim() : "",
                    r.CreatedDateTime, r.IsActive))
                .ToList();
        }

        public (RequisitionRow Header, List<RequisitionDetailRow> Lines)? Get(int id)
        {
            var r = _requisitions.GetSingle(id);
            if (r == null)
                return null;
            var header = List(null, null, null, null, null).FirstOrDefault(x => x.Id == id);
            var goods = _goods.GetAll().ToDictionary(g => g.Id);
            var lines = _details.GetAll().Where(d => d.SalesRequisitionId == id)
                .Select(d => new RequisitionDetailRow(d.FinishedGoodId,
                    goods.TryGetValue(d.FinishedGoodId, out var g) ? g.Name : "", d.Quantity, d.Price, d.CreatedDateTime))
                .ToList();
            return (header, lines);
        }

        /// <summary>Total quantity per product requisitioned on the given day (production planning).</summary>
        public List<ProductTotal> ProductTotalsForDay(DateTime day)
        {
            var goods = _goods.GetAll().ToDictionary(g => g.Id);
            return _details.GetAll()
                .Where(d => d.CreatedDateTime.Date == day.Date)
                .GroupBy(d => d.FinishedGoodId)
                .Select(g => new ProductTotal(g.Key, goods.TryGetValue(g.Key, out var p) ? p.Name : "", g.Sum(x => x.Quantity)))
                .OrderBy(p => p.ProductName)
                .ToList();
        }
    }
}
