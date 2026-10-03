using Microsoft.AspNetCore.Mvc;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Models;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Pages.Audit
{
    public class IndexModel : PageModelBase
    {
        private const int PageSize = 30;
        private readonly AuditService _audit;
        public IndexModel(AuditService audit) => _audit = audit;

        [BindProperty(SupportsGet = true)] public string Q { get; set; }
        [BindProperty(SupportsGet = true)] public string Entity { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? From { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? To { get; set; }
        [BindProperty(SupportsGet = true, Name = "p")] public int PageNo { get; set; } = 1;

        public Paged<AuditLog> Items { get; private set; }

        public void OnGet() => Items = _audit.Page(new AuditFilter(Q, Entity, From, To), PageNo, PageSize);

        public IActionResult OnGetExport()
        {
            var rows = _audit.Query(new AuditFilter(Q, Entity, From, To)).Take(5000).ToList();
            return Excel.Sheet("AuditLog.xlsx", "Audit", new[] { "Time", "User", "Action", "Entity", "Summary" },
                rows.Select(r => new object[] { r.CreatedOn, r.UserName, r.Action, r.Entity, r.Summary }));
        }
    }
}
