using Microsoft.AspNetCore.Mvc;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Pages.Invoices
{
    public class DetailsModel : PageModelBase
    {
        private readonly InvoiceService _invoices;
        public DetailsModel(InvoiceService invoices) => _invoices = invoices;

        public InvoiceDetail Invoice { get; private set; }

        public IActionResult OnGet(int id)
        {
            Invoice = _invoices.Get(id);
            return Invoice == null ? NotFound() : Page();
        }

        public IActionResult OnGetExport(int id)
        {
            var d = _invoices.Get(id);
            if (d == null)
                return NotFound();
            var i = 1;
            return Excel.Sheet($"Invoice_{id}.xlsx", "Invoice Details",
                new[] { "SL", "Product Name", "Unit Price", "Quantity", "Total" },
                d.Lines.Select(l => new object[] { i++, l.ProductName, l.Price, l.Quantity, l.Total }),
                new object[] { "Invoice Total", null, null, d.Quantity, d.Header.Total });
        }
    }
}
