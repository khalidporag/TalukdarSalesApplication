using Microsoft.AspNetCore.Mvc;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Pages.Production
{
    public class IndexModel : PageModelBase
    {
        private readonly RequisitionService _service;
        private readonly IFinishedGoodTypeRepository _types;
        public IndexModel(RequisitionService service, IFinishedGoodTypeRepository types) { _service = service; _types = types; }

        [BindProperty(SupportsGet = true)] public DateTime? Date { get; set; }
        public ProductionPlan Plan { get; private set; }
        public bool WindowOpen { get; private set; }
        public string CloseText { get; private set; }

        public void OnGet()
        {
            Date ??= DateTime.Today;
            Plan = _service.PlanForDay(Date.Value, _types);
            var (_, to) = _service.GetWindow();
            WindowOpen = _service.IsOpenNow();
            CloseText = Dashboard.IndexModel.Clock(to);
        }

        public IActionResult OnGetExport()
        {
            OnGet();
            return Excel.Sheet("ProductionPlan.xlsx", "Production", new[] { "Category", "Product", "Quantity" },
                Plan.Items.OrderBy(i => i.Category).ThenBy(i => i.ProductName).Select(i => new object[] { i.Category, i.ProductName, i.Quantity }));
        }
    }
}
