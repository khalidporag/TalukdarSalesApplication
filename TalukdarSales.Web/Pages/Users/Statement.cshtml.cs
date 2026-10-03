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
        public Statement Data { get; private set; }

        public IActionResult OnGet() => Load() ? Page() : NotFound();

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
