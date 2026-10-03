using Microsoft.AspNetCore.Mvc;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Pages.Collections
{
    public class IndexModel : PageModelBase
    {
        private const int DefaultSize = 25;
        private readonly InvoiceService _invoices;
        public IndexModel(InvoiceService invoices) => _invoices = invoices;

        /// <summary>Preset window in days (1, 7, 30; 0 = all). A custom From/To overrides it.</summary>
        [BindProperty(SupportsGet = true)] public int Days { get; set; } = 7;
        [BindProperty(SupportsGet = true)] public DateTime? From { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? To { get; set; }
        [BindProperty(SupportsGet = true)] public string Method { get; set; }
        [BindProperty(SupportsGet = true)] public string Q { get; set; }
        [BindProperty(SupportsGet = true)] public int Size { get; set; }
        [BindProperty(SupportsGet = true, Name = "p")] public int PageNo { get; set; } = 1;

        public bool Custom => From != null || To != null;
        public CollectionPage Data { get; private set; }
        public List<(DateTime Day, double Amount)> Last14 { get; private set; }

        public void OnGet()
        {
            Load(PageNo, PageSizes.Clamp(Size, DefaultSize));
            var start = DateTime.Today.AddDays(-13);
            var byDay = _invoices.CollectedByDay(start, DateTime.Today);
            Last14 = Enumerable.Range(0, 14).Select(i => start.AddDays(i)).Select(d => (d, byDay.TryGetValue(d, out var v) ? v : 0)).ToList();
        }

        public IActionResult OnGetExport()
        {
            Load(1, 100000);
            return Excel.Sheet("CollectionHistory.xlsx", "Collections",
                new[] { "Invoice No", "Customer", "Collection Time", "Payment Method", "Amount" },
                Data.Page.Items.Select(r => new object[] { r.InvoiceNumber, r.UserName, r.Time, r.PaymentMethod, r.Amount }),
                new object[] { "Total", null, null, null, Data.Total });
        }

        private void Load(int page, int size)
        {
            DateTime? from, to;
            if (Custom) { from = From; to = To; }
            else { from = Days <= 0 ? null : DateTime.Today.AddDays(-(Days - 1)); to = from == null ? null : DateTime.Today; }
            Data = _invoices.Collections(from, to, Method, Q, page, size);
        }
    }
}
