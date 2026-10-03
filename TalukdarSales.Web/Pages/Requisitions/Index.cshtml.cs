using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Pages.Requisitions
{
    public record OrderPanel(OrderRow Header, List<RequisitionDetailRow> Lines);

    public class IndexModel : PageModelBase
    {
        private const int PageSize = 25;
        private readonly RequisitionService _requisitions;
        private readonly InvoiceService _invoices;
        private readonly IUserTypeRepository _userTypes;
        private readonly ISalesInvoiceRepository _invoiceRepo;

        public IndexModel(RequisitionService requisitions, InvoiceService invoices, IUserTypeRepository userTypes, ISalesInvoiceRepository invoiceRepo)
        {
            _requisitions = requisitions; _invoices = invoices; _userTypes = userTypes; _invoiceRepo = invoiceRepo;
        }

        [BindProperty(SupportsGet = true)] public string Tab { get; set; } = "waiting";
        [BindProperty(SupportsGet = true)] public string Q { get; set; }
        [BindProperty(SupportsGet = true)] public int? TypeId { get; set; }
        [BindProperty(SupportsGet = true)] public int Days { get; set; }
        [BindProperty(SupportsGet = true)] public int? Id { get; set; }
        [BindProperty(SupportsGet = true, Name = "p")] public int PageNo { get; set; } = 1;

        public OrderBoard Board { get; private set; }
        public List<SelectListItem> TypeOptions { get; private set; }
        public OrderPanel Panel { get; private set; }

        public void OnGet()
        {
            Tab = Tab is "invoiced" or "all" ? Tab : "waiting";
            Board = _requisitions.Board(Tab, Q, TypeId, Days, PageNo, PageSize);
            TypeOptions = _userTypes.GetAll().Select(t => new SelectListItem(t.TypeName, t.Id.ToString())).ToList();
            if (Id != null) Panel = LoadPanel(Id.Value);
        }

        public IActionResult OnGetPanel(int id)
        {
            Panel = LoadPanel(id);
            return Panel == null ? NotFound() : Partial("_Panel", Panel);
        }

        // One tap: invoice the order exactly as it was placed.
        public IActionResult OnPostInvoice(int id) => Done(_invoices.CreateFromRequisitions(new[] { id }));

        public IActionResult OnPostBulk(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                TempData["Flash"] = "Select at least one order.";
                return Back();
            }
            return Done(_invoices.CreateFromRequisitions(ids));
        }

        // Invoice with quantities adjusted in the panel.
        public IActionResult OnPostCreate(int id)
        {
            var lines = QtyForm.Read(Request.Form).Where(kv => kv.Value > 0).Select(kv => new InvoiceLine(kv.Key, kv.Value.Value)).ToList();
            var (ok, error, invoice) = _invoices.CreateForRequisition(id, lines);
            TempData["Flash"] = ok ? $"{invoice.InvoiceSerialNo} created." : error;
            return ok ? Back() : Back(id);
        }

        private IActionResult Done((bool Ok, string Error, List<Models.SalesInvoice> Invoices) r)
        {
            TempData["Flash"] = r.Ok
                ? (r.Invoices.Count == 1 ? $"{r.Invoices[0].InvoiceSerialNo} created." : $"{r.Invoices.Count} invoices created.")
                : r.Error;
            return Back();
        }

        private IActionResult Back(int? id = null) =>
            RedirectToPage("Index", new { Tab, Q, TypeId, Days, Id = id });

        private OrderPanel LoadPanel(int id)
        {
            var got = _requisitions.Get(id);
            if (got == null) return null;
            var (h, lines) = got.Value;
            var inv = _invoiceRepo.GetAll().Where(i => i.SalesRequisitionId == id).Select(i => new { i.Id, i.InvoiceSerialNo }).FirstOrDefault();
            var row = new OrderRow(h.Id, h.Serial, h.UserId, h.UserName, h.CreatedDateTime, h.IsActive, lines.Count,
                lines.Sum(l => l.Total), inv?.Id, inv?.InvoiceSerialNo);
            return new OrderPanel(row, lines);
        }
    }
}
