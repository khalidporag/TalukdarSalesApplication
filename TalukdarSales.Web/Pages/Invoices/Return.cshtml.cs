using Microsoft.AspNetCore.Mvc;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Pages.Invoices
{
    public class ReturnModel : PageModelBase
    {
        private readonly InvoiceService _invoices;
        public ReturnModel(InvoiceService invoices) => _invoices = invoices;

        [BindProperty(SupportsGet = true)] public int Id { get; set; }
        [BindProperty] public string Reason { get; set; }
        public InvoiceDetail Invoice { get; private set; }
        public List<ReturnableLine> Lines { get; private set; }
        public Dictionary<int, double?> Qty { get; private set; } = new();
        public string Error { get; private set; }

        public IActionResult OnGet() => Load() ? Page() : NotFound();

        public IActionResult OnPost()
        {
            if (!Load()) return NotFound();
            Qty = QtyForm.Read(Request.Form);
            var (ok, error, note) = _invoices.ReturnGoods(Id, Qty.Where(kv => kv.Value > 0).Select(kv => new InvoiceLine(kv.Key, kv.Value.Value)), Reason);
            if (ok)
            {
                TempData["Flash"] = $"{note.Serial} recorded: ৳ {Fmt.Money(note.Amount)} credited" + (note.Refund > 0 ? $", ৳ {Fmt.Money(note.Refund)} to refund." : ".");
                return RedirectToPage("/Invoices/Details", new { id = Id });
            }
            Error = error;
            return Page();
        }

        private bool Load()
        {
            Invoice = _invoices.Get(Id);
            if (Invoice == null) return false;
            Lines = _invoices.Returnable(Id);
            return true;
        }
    }
}
