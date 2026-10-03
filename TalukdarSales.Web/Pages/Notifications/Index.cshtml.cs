using Microsoft.AspNetCore.Mvc;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Models;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Pages.Notifications
{
    public class IndexModel : PageModelBase
    {
        private const int DefaultSize = 25;
        private readonly NotificationService _notes;
        public IndexModel(NotificationService notes) => _notes = notes;

        [BindProperty(SupportsGet = true)] public bool Unread { get; set; }
        [BindProperty(SupportsGet = true)] public string Kind { get; set; }
        [BindProperty(SupportsGet = true)] public int Size { get; set; }
        [BindProperty(SupportsGet = true, Name = "p")] public int PageNo { get; set; } = 1;

        public Paged<Notification> Items { get; private set; }
        public int UnreadCount { get; private set; }

        public void OnGet()
        {
            Kind = Kind is "new-order" or "over-limit" or "closing" ? Kind : null;
            Items = _notes.Page(CurrentUserId, Kind, Unread, PageNo, PageSizes.Clamp(Size, DefaultSize));
            UnreadCount = _notes.UnreadCount(CurrentUserId);
        }

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
