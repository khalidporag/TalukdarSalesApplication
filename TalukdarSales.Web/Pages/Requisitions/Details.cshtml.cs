using Microsoft.AspNetCore.Mvc;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Pages.Requisitions
{
    public class DetailsModel : PageModelBase
    {
        private readonly RequisitionService _service;
        public DetailsModel(RequisitionService service) => _service = service;

        public RequisitionRow Header { get; private set; }
        public List<RequisitionDetailRow> Lines { get; private set; }

        public IActionResult OnGet(int id)
        {
            var result = _service.Get(id);
            if (result == null)
                return NotFound();
            (Header, Lines) = result.Value;
            return Page();
        }
    }
}
