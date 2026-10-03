using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Pages.Requisitions
{
    public class CreateModel : PageModelBase
    {
        private readonly RequisitionService _service;
        private readonly IUserTypeRepository _userTypes;
        private readonly IUserRepository _users;
        private readonly IFinishedGoodTypeRepository _goodTypes;
        private readonly IFinishedGoodsRepository _goods;

        public CreateModel(RequisitionService service, IUserTypeRepository userTypes, IUserRepository users,
            IFinishedGoodTypeRepository goodTypes, IFinishedGoodsRepository goods)
        {
            _service = service;
            _userTypes = userTypes;
            _users = users;
            _goodTypes = goodTypes;
            _goods = goods;
        }

        [BindProperty] public int? UserTypeId { get; set; }
        [BindProperty] public int? UserId { get; set; }
        [BindProperty] public int? GoodTypeId { get; set; }
        public Dictionary<int, double?> Qty { get; set; } = new();

        public string Error { get; private set; }
        public string WindowText { get; private set; }
        public bool IsOpen { get; private set; }
        public List<SelectListItem> UserTypeOptions { get; private set; }
        public List<SelectListItem> UserOptions { get; private set; }
        public List<SelectListItem> GoodTypeOptions { get; private set; }
        public List<FinishedGood> Products { get; set; } = new();

        public void OnGet() => LoadAll();

        // htmx: product table for the chosen product type
        public IActionResult OnGetProducts(int? goodTypeId)
        {
            LoadProducts(goodTypeId);
            return Partial("_Products", this);
        }

        public IActionResult OnPost()
        {
            Qty = QtyForm.Read(Request.Form);
            if (UserId == null)
                Error = "Please select a user.";
            else
            {
                var lines = Qty.Where(kv => kv.Value > 0).Select(kv => new RequisitionLine(kv.Key, kv.Value.Value)).ToList();
                var (ok, error, requisition) = _service.Create(UserId.Value, lines);
                if (ok)
                {
                    TempData["Success"] = $"{requisition.RequisitionSerial} created.";
                    return RedirectToPage("/Requisitions/Index");
                }
                Error = error;
            }
            LoadAll();
            return Page();
        }

        private void LoadAll()
        {
            var (from, to) = _service.GetWindow();
            IsOpen = _service.IsOpenNow();
            WindowText = from == null ? "not configured" : $"{from} - {to}";
            UserTypeOptions = _userTypes.GetAll().Select(t => new SelectListItem(t.TypeName, t.Id.ToString())).ToList();
            UserOptions = _users.GetAll().Where(u => UserTypeId == null || u.UserTypeId == UserTypeId)
                .OrderBy(u => u.FirstName).Select(u => new SelectListItem($"{u.FirstName} {u.LastName} ({u.Username})", u.Id.ToString())).ToList();
            GoodTypeOptions = _goodTypes.GetAll().OrderBy(t => t.Name).Select(t => new SelectListItem(t.Name, t.Id.ToString())).ToList();
            LoadProducts(GoodTypeId);
        }

        private void LoadProducts(int? goodTypeId)
        {
            Products = goodTypeId == null
                ? new List<FinishedGood>()
                : _goods.GetAll().Where(g => g.IsActive && g.GoodTypeId == goodTypeId).OrderBy(g => g.Name).ToList();
        }
    }
}
