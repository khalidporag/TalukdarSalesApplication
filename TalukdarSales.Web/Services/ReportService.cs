using TalukdarSales.Web.Interfaces;

namespace TalukdarSales.Web.Services
{
    public record TopSeller(int UserId, string UserName, string ImageName, double Amount);
    public record ProductSales(int FinishedGoodId, string ProductName, string LogoName, double Quantity);
    public record DueUser(int UserId, string UserName, string Phone, string ImageName, double Due);
    public record DailyProduct(DateTime Date, string ProductName, double Quantity);

    public class ReportService
    {
        private readonly ISalesInvoiceRepository _invoices;
        private readonly ISalesInvoiceDetailsRepository _invoiceDetails;
        private readonly ISalesRequisitionDetailRepository _requisitionDetails;
        private readonly IFinishedGoodsRepository _goods;
        private readonly IUserRepository _users;

        public ReportService(ISalesInvoiceRepository invoices, ISalesInvoiceDetailsRepository invoiceDetails,
            ISalesRequisitionDetailRepository requisitionDetails, IFinishedGoodsRepository goods, IUserRepository users)
        {
            _invoices = invoices;
            _invoiceDetails = invoiceDetails;
            _requisitionDetails = requisitionDetails;
            _goods = goods;
            _users = users;
        }

        /// <summary>Inclusive day range; with no range given, the last month up to now.</summary>
        public static (DateTime Start, DateTime EndExclusive) Range(DateTime? from, DateTime? to)
        {
            if (from != null && to != null)
                return (from.Value.Date, to.Value.Date.AddDays(1));
            return (DateTime.Now.AddMonths(-1), DateTime.Now.Date.AddDays(1));
        }

        public List<TopSeller> TopSellers(DateTime? from, DateTime? to, int take = 5)
        {
            var (start, end) = Range(from, to);
            var top = _invoices.GetAll().Where(i => i.CreatedDateTime >= start && i.CreatedDateTime < end)
                .GroupBy(i => i.UserId)
                .Select(g => new { UserId = g.Key, Amount = g.Sum(i => i.TotalPrice) })
                .OrderByDescending(x => x.Amount).Take(take).ToList();
            var ids = top.Select(t => t.UserId).ToList();
            var users = _users.GetAll().Where(u => ids.Contains(u.Id)).ToDictionary(u => u.Id);
            return top.Select(t => new TopSeller(t.UserId,
                users.TryGetValue(t.UserId, out var u) ? $"{u.FirstName} {u.LastName}".Trim() : "",
                users.TryGetValue(t.UserId, out var u2) ? u2.ImageName : "", t.Amount)).ToList();
        }

        public List<ProductSales> Products(DateTime? from, DateTime? to, bool best, int take = 5)
        {
            var (start, end) = Range(from, to);
            var grouped = _invoiceDetails.GetAll().Where(d => d.CreatedDateTime >= start && d.CreatedDateTime < end)
                .GroupBy(d => d.FinishedGoodsId)
                .Select(g => new { Id = g.Key, Quantity = g.Sum(d => d.Quantity) });
            var top = (best ? grouped.OrderByDescending(x => x.Quantity) : grouped.OrderBy(x => x.Quantity)).Take(take).ToList();
            var ids = top.Select(t => t.Id).ToList();
            var goods = _goods.GetAll().Where(g => ids.Contains(g.Id)).ToDictionary(g => g.Id);
            return top.Select(t => new ProductSales(t.Id, goods.TryGetValue(t.Id, out var p) ? p.Name : "",
                goods.TryGetValue(t.Id, out var p2) ? p2.LogoName : "", t.Quantity)).ToList();
        }

        /// <summary>Customers with the largest outstanding balance on invoices raised in the range (summed per customer).</summary>
        public List<DueUser> TopDue(DateTime? from, DateTime? to, int take = 5)
        {
            var (start, end) = Range(from, to);
            var top = _invoices.GetAll().Where(i => i.CreatedDateTime >= start && i.CreatedDateTime < end)
                .GroupBy(i => i.UserId)
                .Select(g => new { UserId = g.Key, Due = g.Sum(i => i.TotalPrice - i.CollectionAmount) })
                .Where(x => x.Due > 0.005)
                .OrderByDescending(x => x.Due).Take(take).ToList();
            var ids = top.Select(t => t.UserId).ToList();
            var users = _users.GetAll().Where(u => ids.Contains(u.Id)).ToDictionary(u => u.Id);
            return top.Select(t => new DueUser(t.UserId,
                users.TryGetValue(t.UserId, out var u) ? $"{u.FirstName} {u.LastName}".Trim() : "",
                users.TryGetValue(t.UserId, out var u2) ? u2.PhoneNumber : "",
                users.TryGetValue(t.UserId, out var u3) ? u3.ImageName : "", t.Due)).ToList();
        }

        public List<DailyProduct> DailySales(DateTime? from, DateTime? to)
        {
            var (start, end) = Range(from, to);
            var rows = _invoiceDetails.GetAll().Where(d => d.CreatedDateTime >= start && d.CreatedDateTime < end)
                .GroupBy(d => new { d.CreatedDateTime.Date, d.FinishedGoodsId })
                .Select(g => new { g.Key.Date, Id = g.Key.FinishedGoodsId, Quantity = g.Sum(d => d.Quantity) })
                .ToList();
            return WithNames(rows.Select(r => (r.Date, r.Id, r.Quantity)));
        }

        public List<DailyProduct> DailyOrders(DateTime? from, DateTime? to)
        {
            var (start, end) = Range(from, to);
            var rows = _requisitionDetails.GetAll().Where(d => d.CreatedDateTime >= start && d.CreatedDateTime < end)
                .GroupBy(d => new { d.CreatedDateTime.Date, d.FinishedGoodId })
                .Select(g => new { g.Key.Date, Id = g.Key.FinishedGoodId, Quantity = g.Sum(d => d.Quantity) })
                .ToList();
            return WithNames(rows.Select(r => (r.Date, r.Id, r.Quantity)));
        }

        private List<DailyProduct> WithNames(IEnumerable<(DateTime Date, int Id, double Quantity)> rows)
        {
            var list = rows.ToList();
            var ids = list.Select(r => r.Id).Distinct().ToList();
            var goods = _goods.GetAll().Where(g => ids.Contains(g.Id)).ToDictionary(g => g.Id);
            return list.Select(r => new DailyProduct(r.Date, goods.TryGetValue(r.Id, out var p) ? p.Name : "", r.Quantity))
                .OrderBy(d => d.Date).ThenBy(d => d.ProductName).ToList();
        }
    }
}
