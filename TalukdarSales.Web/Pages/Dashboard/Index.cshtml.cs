using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly AnalyticsService _analytics;
        private readonly RequisitionService _requisitions;
        private readonly INoticeRepository _notices;

        public IndexModel(AnalyticsService analytics, RequisitionService requisitions, INoticeRepository notices)
        {
            _analytics = analytics; _requisitions = requisitions; _notices = notices;
        }

        [BindProperty(SupportsGet = true, Name = "p")] public string PeriodKey { get; set; }
        [BindProperty(SupportsGet = true)] public bool Slow { get; set; }

        public Overview Data { get; private set; }
        public List<ProductTotal> Plan { get; private set; }
        public int PlanOrders { get; private set; }
        public List<Notice> Notices { get; private set; }
        public bool WindowOpen { get; private set; }
        public string WindowText { get; private set; }
        public string FirstName => (User.FindFirst(ClaimTypes.GivenName)?.Value ?? "").Split(' ').FirstOrDefault() ?? "";

        public static string Greeting() => DateTime.Now.Hour < 12 ? "Good morning" : DateTime.Now.Hour < 17 ? "Good afternoon" : "Good evening";

        public void OnGet()
        {
            Data = _analytics.Build(Period.Parse(PeriodKey));
            Plan = _requisitions.ProductTotalsForDay(DateTime.Today).OrderByDescending(p => p.TotalQuantity).Take(5).ToList();
            PlanOrders = _requisitions.OrderCountForDay(DateTime.Today);
            Notices = _notices.GetAll().OrderByDescending(n => n.CreatedOn).Take(2).ToList();
            var (from, to) = _requisitions.GetWindow();
            WindowOpen = RequisitionService.IsWithinWindow(from, to, DateTime.Now.TimeOfDay);
            WindowText = WindowOpen ? $"Orders open until {Clock(to)} · {Left(to)} left" : $"Orders closed · reopen at {Clock(from)}";
        }

        public static string Clock(string t) => TimeSpan.TryParse(t, out var ts) ? DateTime.Today.Add(ts).ToString("h:mm tt") : t;

        private static string Left(string to)
        {
            if (!TimeSpan.TryParse(to, out var end)) return "";
            var left = end - DateTime.Now.TimeOfDay;
            if (left < TimeSpan.Zero) left += TimeSpan.FromDays(1);
            return left.TotalHours >= 1 ? $"{(int)left.TotalHours}h {left.Minutes}m" : $"{left.Minutes}m";
        }
    }
}
