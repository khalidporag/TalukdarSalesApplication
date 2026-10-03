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
            return (DateTime.Now.AddMonths(-1), DateTime.MaxValue);
        }

        public List<TopSeller> TopSellers(DateTime? from, DateTime? to, int take = 5)
        {
            var (start, end) = Range(from, to);
            var users = _users.GetAll().ToDictionary(u => u.Id);
            return _invoices.GetAll().Where(i => i.CreatedDateTime >= start && i.CreatedDateTime < end)
                .GroupBy(i => i.UserId)
                .Select(g => new TopSeller(g.Key,
                    users.TryGetValue(g.Key, out var u) ? $"{u.FirstName} {u.LastName}".Trim() : "",
                    users.TryGetValue(g.Key, out var u2) ? u2.ImageName : "", g.Sum(i => i.TotalPrice)))
                .OrderByDescending(s => s.Amount).Take(take).ToList();
        }

        public List<ProductSales> Products(DateTime? from, DateTime? to, bool best, int take = 5)
        {
            var (start, end) = Range(from, to);
            var goods = _goods.GetAll().ToDictionary(g => g.Id);
            var q = _invoiceDetails.GetAll().Where(d => d.CreatedDateTime >= start && d.CreatedDateTime < end)
                .GroupBy(d => d.FinishedGoodsId)
                .Select(g => new ProductSales(g.Key, goods.TryGetValue(g.Key, out var p) ? p.Name : "",
                    goods.TryGetValue(g.Key, out var p2) ? p2.LogoName : "", g.Sum(d => d.Quantity)));
            return (best ? q.OrderByDescending(p => p.Quantity) : q.OrderBy(p => p.Quantity)).Take(take).ToList();
        }

        /// <summary>Customers with the largest outstanding balance on invoices raised in the range (summed per customer).</summary>
        public List<DueUser> TopDue(DateTime? from, DateTime? to, int take = 5)
        {
            var (start, end) = Range(from, to);
            var users = _users.GetAll().ToDictionary(u => u.Id);
            return _invoices.GetAll().Where(i => i.CreatedDateTime >= start && i.CreatedDateTime < end)
                .GroupBy(i => i.UserId)
                .Select(g => new { g.Key, Due = g.Sum(i => i.TotalPrice - i.CollectionAmount) })
                .Where(x => x.Due > 0.005)
                .OrderByDescending(x => x.Due).Take(take)
                .Select(x => new DueUser(x.Key,
                    users.TryGetValue(x.Key, out var u) ? $"{u.FirstName} {u.LastName}".Trim() : "",
                    users.TryGetValue(x.Key, out var u2) ? u2.PhoneNumber : "",
                    users.TryGetValue(x.Key, out var u3) ? u3.ImageName : "", x.Due))
                .ToList();
        }

        public List<DailyProduct> DailySales(DateTime? from, DateTime? to)
        {
            var (start, end) = Range(from, to);
            var goods = _goods.GetAll().ToDictionary(g => g.Id);
            return _invoiceDetails.GetAll().Where(d => d.CreatedDateTime >= start && d.CreatedDateTime < end)
                .GroupBy(d => new { d.CreatedDateTime.Date, d.FinishedGoodsId })
                .Select(g => new DailyProduct(g.Key.Date, goods.TryGetValue(g.Key.FinishedGoodsId, out var p) ? p.Name : "", g.Sum(d => d.Quantity)))
                .OrderBy(d => d.Date).ThenBy(d => d.ProductName).ToList();
        }

        public List<DailyProduct> DailyOrders(DateTime? from, DateTime? to)
        {
            var (start, end) = Range(from, to);
            var goods = _goods.GetAll().ToDictionary(g => g.Id);
            return _requisitionDetails.GetAll().Where(d => d.CreatedDateTime >= start && d.CreatedDateTime < end)
                .GroupBy(d => new { d.CreatedDateTime.Date, d.FinishedGoodId })
                .Select(g => new DailyProduct(g.Key.Date, goods.TryGetValue(g.Key.FinishedGoodId, out var p) ? p.Name : "", g.Sum(d => d.Quantity)))
                .OrderBy(d => d.Date).ThenBy(d => d.ProductName).ToList();
        }
    }
}
