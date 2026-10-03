using Microsoft.AspNetCore.Mvc;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Pages.Invoices
{
    public record CollectPanel(InvoiceRow Invoice, double Owed, List<OpenInvoice> Open, string[] Methods);

    public class IndexModel : PageModelBase
    {
        private const int PageSize = 25;
        public static readonly string[] Methods = { "Cash", "bKash", "Bank", "Cheque" };
        private readonly InvoiceService _invoices;

        public IndexModel(InvoiceService invoices) => _invoices = invoices;

        [BindProperty(SupportsGet = true)] public string Status { get; set; } = "all";
        [BindProperty(SupportsGet = true)] public string Q { get; set; }
        [BindProperty(SupportsGet = true)] public int Days { get; set; } = 30;
        [BindProperty(SupportsGet = true)] public int? Id { get; set; }
        [BindProperty(SupportsGet = true, Name = "p")] public int PageNo { get; set; } = 1;

        public InvoiceBoard Board { get; private set; }
        public CollectPanel Panel { get; private set; }

        public void OnGet()
        {
            Load();
            if (Id != null) Panel = LoadPanel(Id.Value);
        }

        public IActionResult OnGetPanel(int id)
        {
            Panel = LoadPanel(id);
            return Panel == null ? NotFound() : Partial("_Collect", Panel);
        }

        public IActionResult OnGetExport()
        {
            Load();
            return Excel.Sheet("InvoiceList.xlsx", "Invoice Data",
                new[] { "Invoice For", "Invoice No", "Requisition No", "Creation Time", "Total", "Collection", "Due" },
                Board.Page.Items.Select(r => new object[] { r.UserName, r.Number, r.RequisitionNo, r.CreatedDateTime, r.Total, r.Collected, r.Due }),
                new object[] { "Total", null, null, null, Board.Billed, Board.Collected, Board.Due });
        }

        public IActionResult OnPostCollect(int invoiceId, double? amount, string method)
        {
            if (amount == null || amount <= 0)
                TempData["Flash"] = "Enter an amount greater than zero.";
            else
            {
                var (ok, error) = _invoices.Collect(invoiceId, amount.Value, string.IsNullOrWhiteSpace(method) ? "Cash" : method.Trim());
                TempData["Flash"] = ok ? $"৳ {Fmt.Money(amount.Value)} collected by {method}." : error;
            }
            return RedirectToPage("Index", new { Status, Q, Days, Id = invoiceId });
        }

        private void Load()
        {
            Status = Status is "unpaid" or "partial" or "paid" or "due" ? Status : "all";
            Board = _invoices.Board(Status, Q, Days, PageNo, PageSize);
        }

        private CollectPanel LoadPanel(int id)
        {
            var d = _invoices.Get(id);
            if (d == null) return null;
            return new CollectPanel(d.Header, _invoices.OutstandingFor(d.Header.UserId), _invoices.OpenInvoices(d.Header.UserId), Methods);
        }
    }
}
