using Microsoft.AspNetCore.Mvc;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Security;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Pages.Requisitions
{
    public record OrderCustomer(int Id, string Name, string Code, string Phone, double Due, double Limit, int CreditDays);
    public record OrderProduct(int Id, string Name, string Category, int CategoryIndex, double Price, string Uom);

    public class CreateModel : PageModelBase
    {
        private readonly RequisitionService _service;
        private readonly IUserRepository _users;
        private readonly IFinishedGoodTypeRepository _goodTypes;
        private readonly IFinishedGoodsRepository _goods;
        private readonly ISalesRequisitionRepository _requisitions;
        private readonly AccessService _access;

        public CreateModel(RequisitionService service, IUserRepository users, IFinishedGoodTypeRepository goodTypes,
            IFinishedGoodsRepository goods, ISalesRequisitionRepository requisitions, AccessService access)
        {
            _access = access;
            _service = service; _users = users; _goodTypes = goodTypes; _goods = goods; _requisitions = requisitions;
        }

        [BindProperty] public int? UserId { get; set; }
        /// <summary>When set, the page edits this waiting order instead of creating one.</summary>
        [BindProperty(SupportsGet = true, Name = "edit")] public int? EditId { get; set; }
        public string EditSerial { get; private set; }
        public Dictionary<int, double?> Qty { get; private set; } = new();

        public string Error { get; private set; }
        public string WindowText { get; private set; }
        public bool IsOpen { get; private set; }
        public List<OrderCustomer> Customers { get; private set; }
        public List<OrderCustomer> Recent { get; private set; }
        public List<string> Categories { get; private set; }
        public List<OrderProduct> Products { get; private set; }

        public IActionResult OnGet()
        {
            if (EditId != null)
            {
                if (!_access.For(User).Has(Perm.RequisitionEdit)) return Forbid();
                var got = _service.Get(EditId.Value);
                if (got == null) return NotFound();
                var (h, lines) = got.Value;
                if (!h.IsActive) return RedirectToPage("/Requisitions/Index", new { id = h.Id, tab = "all" });
                UserId = h.UserId;
                EditSerial = h.Serial;
                Qty = lines.GroupBy(l => l.FinishedGoodId).ToDictionary(g => g.Key, g => (double?)g.Sum(l => l.Quantity));
            }
            LoadAll();
            return Page();
        }

        public IActionResult OnPost()
        {
            Qty = QtyForm.Read(Request.Form);
            if (EditId != null)
            {
                if (!_access.For(User).Has(Perm.RequisitionEdit)) return Forbid();
                var (ok2, error2) = _service.Update(EditId.Value, Qty.Where(kv => kv.Value > 0).Select(kv => new RequisitionLine(kv.Key, kv.Value.Value)));
                if (ok2)
                {
                    TempData["Flash"] = "Order updated.";
                    return RedirectToPage("/Requisitions/Index", new { id = EditId, tab = "waiting" });
                }
                Error = error2;
                EditSerial = _service.Get(EditId.Value)?.Header.Serial;
                LoadAll();
                return Page();
            }
            if (UserId == null)
                Error = "Choose a customer first.";
            else
            {
                var lines = Qty.Where(kv => kv.Value > 0).Select(kv => new RequisitionLine(kv.Key, kv.Value.Value)).ToList();
                var (ok, error, requisition) = _service.Create(UserId.Value, lines);
                if (ok)
                {
                    TempData["Flash"] = $"{requisition.RequisitionSerial} sent. It is waiting for an invoice.";
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
            WindowText = from == null ? "Order window not configured" : $"Orders open {Dashboard.IndexModel.Clock(from)} to {Dashboard.IndexModel.Clock(to)}";
            Customers = _users.GetAll().OrderBy(u => u.FirstName).ThenBy(u => u.LastName).ToList()
                .Select(u => new OrderCustomer(u.Id, $"{u.FirstName} {u.LastName}".Trim(), u.SequencialUserId, u.PhoneNumber, u.DueAmount, u.MaxCreditLimit, u.MaxCreditDays)).ToList();
            var recentIds = _requisitions.GetAll().OrderByDescending(r => r.CreatedDateTime).Select(r => r.UserId).Take(60).ToList().Distinct().Take(4).ToList();
            Recent = recentIds.Select(id => Customers.FirstOrDefault(c => c.Id == id)).Where(c => c != null).ToList();
            var types = _goodTypes.GetAll().OrderBy(t => t.Name).ToList();
            Categories = types.Select(t => t.Name).ToList();
            Products = _goods.GetAll().Where(g => g.IsActive).OrderBy(g => g.Name).ToList()
                .Select(g =>
                {
                    var i = types.FindIndex(t => t.Id == g.GoodTypeId);
                    return new OrderProduct(g.Id, g.Name, i >= 0 ? types[i].Name : "Other", i < 0 ? 0 : i % 4 + 1, g.UnitPrice, g.UOM);
                }).ToList();
        }
    }
}
