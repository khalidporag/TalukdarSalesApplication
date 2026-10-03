using Microsoft.AspNetCore.Mvc;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Pages.Collections
{
    public class IndexModel : PageModelBase
    {
        private readonly InvoiceService _invoices;
        public IndexModel(InvoiceService invoices) => _invoices = invoices;

        [BindProperty(SupportsGet = true)] public int Days { get; set; } = 7;
        [BindProperty(SupportsGet = true)] public string Method { get; set; }
        [BindProperty(SupportsGet = true)] public string Q { get; set; }

        public List<CollectionRow> Rows { get; private set; }
        public double Total { get; private set; }
        public List<(DateTime Day, double Amount)> Last14 { get; private set; }

        public void OnGet()
        {
            Load();
            var start = DateTime.Today.AddDays(-13);
            var recent = _invoices.CollectionHistory(null, null, start, DateTime.Today).Rows;
            Last14 = Enumerable.Range(0, 14).Select(i => start.AddDays(i))
                .Select(d => (d, recent.Where(r => r.Time.Date == d && r.Amount > 0).Sum(r => r.Amount))).ToList();
        }

        public IActionResult OnGetExport()
        {
            Load();
            return Excel.Sheet("CollectionHistory.xlsx", "Collections",
                new[] { "Invoice No", "Customer", "Collection Time", "Payment Method", "Amount" },
                Rows.Select(r => new object[] { r.InvoiceNumber, r.UserName, r.Time, r.PaymentMethod, r.Amount }),
                new object[] { "Total", null, null, null, Total });
        }

        private void Load()
        {
            var from = Days <= 0 ? (DateTime?)null : DateTime.Today.AddDays(-(Days - 1));
            var rows = _invoices.CollectionHistory(null, null, from, from == null ? null : DateTime.Today).Rows;
            if (!string.IsNullOrWhiteSpace(Method)) rows = rows.Where(r => string.Equals(r.PaymentMethod, Method, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrWhiteSpace(Q))
            {
                var t = Q.Trim();
                rows = rows.Where(r => (r.UserName ?? "").Contains(t, StringComparison.OrdinalIgnoreCase) || (r.InvoiceNumber ?? "").Contains(t, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            Rows = rows;
            Total = rows.Where(r => r.Amount > 0).Sum(r => r.Amount);
        }
    }
}
