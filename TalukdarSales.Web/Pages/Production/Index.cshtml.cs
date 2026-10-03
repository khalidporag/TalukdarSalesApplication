using Microsoft.AspNetCore.Mvc;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Pages.Production
{
    public class IndexModel : PageModelBase
    {
        private readonly RequisitionService _service;
        public IndexModel(RequisitionService service) => _service = service;

        [BindProperty(SupportsGet = true)] public DateTime? Date { get; set; }
        public List<ProductTotal> Totals { get; private set; }

        public void OnGet()
        {
            Date ??= DateTime.Today;
            Totals = _service.ProductTotalsForDay(Date.Value);
        }
    }
}
