using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Pages.Collections
{
    public class IndexModel : PageModelBase
    {
        private readonly InvoiceService _invoices;
        private readonly IUserTypeRepository _userTypes;
        private readonly IUserRepository _users;

        public IndexModel(InvoiceService invoices, IUserTypeRepository userTypes, IUserRepository users)
        {
            _invoices = invoices;
            _userTypes = userTypes;
            _users = users;
        }

        [BindProperty(SupportsGet = true)] public int? TypeId { get; set; }
        [BindProperty(SupportsGet = true)] public int? UserId { get; set; }
        [BindProperty(SupportsGet = true)] public int? InvoiceId { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? From { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? To { get; set; }

        public List<CollectionRow> Rows { get; private set; }
        public double Total { get; private set; }
        public List<SelectListItem> TypeOptions { get; private set; }
        public List<SelectListItem> UserOptions { get; private set; }

        public void OnGet()
        {
            TypeOptions = _userTypes.GetAll().Select(t => new SelectListItem(t.TypeName, t.Id.ToString())).ToList();
            UserOptions = _users.GetAll().Where(u => TypeId == null || u.UserTypeId == TypeId)
                .OrderBy(u => u.FirstName).Select(u => new SelectListItem($"{u.FirstName} {u.LastName} ({u.Username})", u.Id.ToString())).ToList();
            Load();
        }

        public IActionResult OnGetList()
        {
            Load();
            return Partial("_List", this);
        }

        public IActionResult OnGetExport()
        {
            Load();
            return Excel.Sheet("CollectionHistory.xlsx", "Collections",
                new[] { "Invoice No", "Collection Time", "Payment Method", "Amount" },
                Rows.Select(r => new object[] { r.InvoiceNumber, r.Time, r.PaymentMethod, r.Amount }),
                new object[] { "Total", null, null, Total });
        }

        private void Load() => (Rows, Total) = _invoices.CollectionHistory(UserId, InvoiceId, From, To ?? From);
    }
}
