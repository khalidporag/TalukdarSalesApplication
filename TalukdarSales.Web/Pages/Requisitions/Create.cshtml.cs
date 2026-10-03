using Microsoft.AspNetCore.Mvc;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Interfaces;
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

        public CreateModel(RequisitionService service, IUserRepository users, IFinishedGoodTypeRepository goodTypes,
            IFinishedGoodsRepository goods, ISalesRequisitionRepository requisitions)
        {
            _service = service; _users = users; _goodTypes = goodTypes; _goods = goods; _requisitions = requisitions;
        }

        [BindProperty] public int? UserId { get; set; }
        public Dictionary<int, double?> Qty { get; private set; } = new();

        public string Error { get; private set; }
        public string WindowText { get; private set; }
        public bool IsOpen { get; private set; }
        public List<OrderCustomer> Customers { get; private set; }
        public List<OrderCustomer> Recent { get; private set; }
        public List<string> Categories { get; private set; }
        public List<OrderProduct> Products { get; private set; }

        public void OnGet() => LoadAll();

        public IActionResult OnPost()
        {
            Qty = QtyForm.Read(Request.Form);
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
