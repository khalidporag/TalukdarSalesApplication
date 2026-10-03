using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Pages.Requisitions
{
    public class IndexModel : PageModelBase
    {
        private const int PageSize = 25;
        private readonly RequisitionService _requisitions;
        private readonly InvoiceService _invoices;
        private readonly IUserTypeRepository _userTypes;
        private readonly IUserRepository _users;

        public IndexModel(RequisitionService requisitions, InvoiceService invoices, IUserTypeRepository userTypes, IUserRepository users)
        {
            _requisitions = requisitions;
            _invoices = invoices;
            _userTypes = userTypes;
            _users = users;
        }

        [BindProperty(SupportsGet = true)] public bool Active { get; set; } = true;
        [BindProperty(SupportsGet = true)] public int? TypeId { get; set; }
        [BindProperty(SupportsGet = true)] public int? UserId { get; set; }
        [BindProperty(SupportsGet = true)] public string Serial { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? From { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? To { get; set; }
        [BindProperty(SupportsGet = true, Name = "p")] public int PageNo { get; set; } = 1;

        public Paged<RequisitionRow> Rows { get; private set; }
        public List<SelectListItem> TypeOptions { get; private set; }
        public List<SelectListItem> UserOptions { get; private set; }

        public void OnGet()
        {
            LoadOptions();
            Load();
        }

        public IActionResult OnGetList()
        {
            Load();
            return Partial("_List", this);
        }

        // Approve = turn the ticked active requisitions into invoices
        public IActionResult OnPostApprove(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                Toast("Select at least one requisition.", false);
            }
            else
            {
                var (ok, error, created) = _invoices.CreateFromRequisitions(ids);
                Toast(ok ? $"{created.Count} invoice(s) created." : error, ok);
            }
            Load();
            return Partial("_List", this);
        }

        private void Load()
        {
            var all = _requisitions.List(Active, UserId, Serial, From, To)
                .Where(r => TypeId == null || UserId != null || UserBelongsToType(r.UserId, TypeId.Value));
            Rows = Paged<RequisitionRow>.Create(all, PageNo, PageSize);
        }

        private Dictionary<int, int> _userTypeByUser;
        private bool UserBelongsToType(int userId, int typeId)
        {
            _userTypeByUser ??= _users.GetAll().ToDictionary(u => u.Id, u => u.UserTypeId);
            return _userTypeByUser.TryGetValue(userId, out var t) && t == typeId;
        }

        private void LoadOptions()
        {
            TypeOptions = _userTypes.GetAll().Select(t => new SelectListItem(t.TypeName, t.Id.ToString())).ToList();
            UserOptions = _users.GetAll().Where(u => TypeId == null || u.UserTypeId == TypeId)
                .OrderBy(u => u.FirstName).Select(u => new SelectListItem($"{u.FirstName} {u.LastName} ({u.Username})", u.Id.ToString())).ToList();
        }
    }
}
