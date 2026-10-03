using Microsoft.AspNetCore.Mvc;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Pages.Users
{
    public class StatementModel : PageModelBase
    {
        private readonly InvoiceService _invoices;
        public StatementModel(InvoiceService invoices) => _invoices = invoices;

        [BindProperty(SupportsGet = true)] public int Id { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? From { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? To { get; set; }
        [BindProperty(SupportsGet = true)] public string Kind { get; set; }
        [BindProperty(SupportsGet = true)] public int Size { get; set; }
        [BindProperty(SupportsGet = true, Name = "p")] public int PageNo { get; set; } = 1;
        /// <summary>Print every line, not just the page on screen.</summary>
        [BindProperty(SupportsGet = true)] public bool All { get; set; }

        public Statement Data { get; private set; }
        public List<StatementEntry> Shown { get; private set; }
        public PagerVm Pager { get; private set; }

        public IActionResult OnGet()
        {
            if (!Load()) return NotFound();
            var rows = Data.Entries.Where(e => string.IsNullOrEmpty(Kind) || e.Kind == Kind).ToList();
            var size = All ? Math.Max(1, rows.Count) : PageSizes.Clamp(Size, 25);
            var page = Math.Clamp(PageNo, 1, Math.Max(1, (int)Math.Ceiling(rows.Count / (double)size)));
            Shown = rows.Skip((page - 1) * size).Take(size).ToList();
            Pager = new PagerVm(page, size, rows.Count, $"/Users/Statement?id={Id}&from={From:yyyy-MM-dd}&to={To:yyyy-MM-dd}&kind={Kind}");
            return Page();
        }

        public IActionResult OnGetExport()
        {
            if (!Load()) return NotFound();
            return Excel.Sheet($"Statement_{Data.Customer.SequencialUserId}.xlsx", "Statement",
                new[] { "Date", "Type", "Reference", "Note", "Debit", "Credit", "Balance" },
                Data.Entries.Select(e => new object[] { e.Date, e.Kind, e.Reference, e.Note, e.Debit, e.Credit, e.Balance }),
                new object[] { "Closing balance", null, null, null, null, null, Data.Closing });
        }

        private bool Load()
        {
            To ??= DateTime.Today;
            From ??= To.Value.AddDays(-89);
            Data = _invoices.StatementFor(Id, From.Value, To.Value);
            return Data != null;
        }
    }
}
