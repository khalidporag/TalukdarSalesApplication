using Microsoft.AspNetCore.Mvc;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Pages.Reports
{
    public class IndexModel : PageModelBase
    {
        private readonly ReportService _reports;
        public IndexModel(ReportService reports) => _reports = reports;

        [BindProperty(SupportsGet = true)] public DateTime? From { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? To { get; set; }

        public List<TopSeller> TopSellers { get; private set; }
        public List<ProductSales> BestProducts { get; private set; }
        public List<ProductSales> WorstProducts { get; private set; }
        public List<DueUser> DueUsers { get; private set; }
        public List<DailyProduct> Daily { get; private set; }

        public string Kind { get; private set; }

        // Each panel is an htmx fragment with its own date range
        public IActionResult OnGetPanel(string kind)
        {
            Kind = kind;
            Load(kind);
            return Partial("_Panel", this);
        }

        public IActionResult OnGetExport(string kind)
        {
            Load(kind);
            switch (kind)
            {
                case "sellers":
                    return Excel.Sheet("TopSellers.xlsx", "Top Sellers", new[] { "#", "Name", "Amount" },
                        TopSellers.Select((s, i) => new object[] { i + 1, s.UserName, s.Amount }));
                case "best":
                case "worst":
                    var list = kind == "best" ? BestProducts : WorstProducts;
                    return Excel.Sheet("Products.xlsx", "Products", new[] { "#", "Product", "Quantity" },
                        list.Select((p, i) => new object[] { i + 1, p.ProductName, p.Quantity }));
                case "due":
                    return Excel.Sheet("DueCustomers.xlsx", "Due", new[] { "#", "Name", "Phone", "Due" },
                        DueUsers.Select((u, i) => new object[] { i + 1, u.UserName, u.Phone, u.Due }));
                case "sales":
                case "orders":
                    return Excel.Sheet("DailySummary.xlsx", "Daily", new[] { "Date", "Product", "Quantity" },
                        Daily.Select(d => new object[] { d.Date, d.ProductName, d.Quantity }));
                default:
                    return NotFound();
            }
        }

        public void Load(string kind)
        {
            switch (kind)
            {
                case "sellers": TopSellers = _reports.TopSellers(From, To); break;
                case "best": BestProducts = _reports.Products(From, To, best: true); break;
                case "worst": WorstProducts = _reports.Products(From, To, best: false); break;
                case "due": DueUsers = _reports.TopDue(From, To); break;
                case "sales": Daily = _reports.DailySales(From, To); break;
                case "orders": Daily = _reports.DailyOrders(From, To); break;
            }
        }

        public static readonly (string Kind, string Title)[] Panels =
        {
            ("sellers", "Top Five Sellers"),
            ("best", "Top Five Selling Products"),
            ("worst", "Least Five Selling Products"),
            ("due", "Top Five Customers by Due Amount"),
            ("sales", "Daily Accumulated Sales Summary"),
            ("orders", "Daily Accumulated Order Summary"),
        };
    }
}
