using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Pages.Invoices
{
    public class CreateModel : PageModelBase
    {
        private readonly InvoiceService _invoices;
        private readonly RequisitionService _requisitions;
        private readonly IUserTypeRepository _userTypes;
        private readonly IUserRepository _users;

        public CreateModel(InvoiceService invoices, RequisitionService requisitions, IUserTypeRepository userTypes, IUserRepository users)
        {
            _invoices = invoices;
            _requisitions = requisitions;
            _userTypes = userTypes;
            _users = users;
        }

        [BindProperty] public int? UserTypeId { get; set; }
        [BindProperty] public int? UserId { get; set; }
        [BindProperty] public int? RequisitionId { get; set; }
        public Dictionary<int, double?> Qty { get; set; } = new();

        public string Error { get; private set; }
        public List<SelectListItem> UserTypeOptions { get; private set; }
        public List<SelectListItem> UserOptions { get; private set; }
        public List<SelectListItem> RequisitionOptions { get; private set; }
        public List<RequisitionDetailRow> Lines { get; set; } = new();

        public void OnGet() => LoadAll();

        // htmx: requisition lines for the chosen requisition
        public IActionResult OnGetLines(int? requisitionId)
        {
            RequisitionId = requisitionId;
            LoadLines();
            return Partial("_Lines", this);
        }

        public IActionResult OnPost()
        {
            Qty = QtyForm.Read(Request.Form);
            if (RequisitionId == null)
                Error = "Please select a requisition.";
            else
            {
                var lines = Qty.Where(kv => kv.Value > 0).Select(kv => new InvoiceLine(kv.Key, kv.Value.Value)).ToList();
                var (ok, error, invoice) = _invoices.CreateForRequisition(RequisitionId.Value, lines);
                if (ok)
                {
                    TempData["Success"] = $"{invoice.InvoiceSerialNo} created.";
                    return RedirectToPage("/Invoices/Index");
                }
                Error = error;
            }
            LoadAll();
            return Page();
        }

        private void LoadAll()
        {
            UserTypeOptions = _userTypes.GetAll().Select(t => new SelectListItem(t.TypeName, t.Id.ToString())).ToList();
            UserOptions = _users.GetAll().Where(u => UserTypeId == null || u.UserTypeId == UserTypeId)
                .OrderBy(u => u.FirstName).Select(u => new SelectListItem($"{u.FirstName} {u.LastName} ({u.Username})", u.Id.ToString())).ToList();
            RequisitionOptions = UserId == null
                ? new List<SelectListItem>()
                : _requisitions.List(true, UserId, null, null, null).Select(r => new SelectListItem($"{r.Serial} - {r.CreatedDateTime:dd MMM yyyy HH:mm}", r.Id.ToString())).ToList();
            LoadLines();
        }

        private void LoadLines()
        {
            var got = RequisitionId == null ? null : _requisitions.Get(RequisitionId.Value);
            Lines = got == null || !got.Value.Header.IsActive ? new List<RequisitionDetailRow>() : got.Value.Lines;
        }
    }
}
