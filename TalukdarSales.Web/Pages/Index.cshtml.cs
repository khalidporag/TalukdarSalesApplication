using Microsoft.AspNetCore.Mvc;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;

namespace TalukdarSales.Web.Pages
{
    public class IndexModel : PageModelBase
    {
        private readonly INoticeRepository _notices;
        public IndexModel(INoticeRepository notices) => _notices = notices;

        public List<Notice> Notices { get; private set; }
        public Notice Selected { get; private set; }

        public void OnGet() => Notices = _notices.GetAll().OrderByDescending(n => n.CreatedOn).Take(6).ToList();

        public IActionResult OnGetNotice(int id)
        {
            Selected = _notices.GetSingle(id);
            return Selected == null ? NotFound() : Partial("_NoticeModal", this);
        }
    }
}
