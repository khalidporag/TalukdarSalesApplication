using Microsoft.AspNetCore.Mvc;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Models;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Pages.Notifications
{
    public class IndexModel : PageModelBase
    {
        private readonly NotificationService _notes;
        public IndexModel(NotificationService notes) => _notes = notes;

        public List<Notification> Items { get; private set; }

        public void OnGet() => Items = _notes.Recent(CurrentUserId);

        /// <summary>Marks one as read and goes to what it is about.</summary>
        public IActionResult OnGetOpen(int id, string to)
        {
            _notes.MarkRead(CurrentUserId, id);
            return LocalRedirect(!string.IsNullOrEmpty(to) && Url.IsLocalUrl(to) ? to : "/Notifications");
        }

        public IActionResult OnPostReadAll()
        {
            _notes.MarkAllRead(CurrentUserId);
            return RedirectToPage();
        }
    }
}
