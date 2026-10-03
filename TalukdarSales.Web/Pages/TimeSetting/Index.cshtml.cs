using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;

namespace TalukdarSales.Web.Pages.TimeSetting
{
    public class IndexModel : PageModelBase
    {
        private readonly ITimeSettingRepository _settings;
        private readonly Services.AuditService _audit;
        public IndexModel(ITimeSettingRepository settings, Services.AuditService audit) { _settings = settings; _audit = audit; }

        public bool OpenNow { get; private set; }
        public string LeftText { get; private set; }

        [BindProperty] public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required, RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Use HH:mm")] public string From { get; set; }
            [Required, RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Use HH:mm")] public string To { get; set; }
        }

        public void OnGet()
        {
            var current = _settings.GetAll().OrderByDescending(n => n.CreatedOn).FirstOrDefault();
            if (current != null)
                Input = new InputModel { From = Normalize(current.From), To = Normalize(current.To) };
            Status();
        }

        private void Status()
        {
            OpenNow = Services.RequisitionService.IsWithinWindow(Input.From, Input.To, DateTime.Now.TimeOfDay);
            if (OpenNow && TimeSpan.TryParse(Input.To, out var end))
            {
                var left = end - DateTime.Now.TimeOfDay;
                if (left < TimeSpan.Zero) left += TimeSpan.FromDays(1);
                LeftText = left.TotalHours >= 1 ? $"{(int)left.TotalHours}h {left.Minutes}m" : $"{left.Minutes}m";
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                Status();
                return Page();
            }

            var current = _settings.GetAll().OrderByDescending(n => n.CreatedOn).FirstOrDefault();
            if (current == null)
            {
                _settings.Add(new Models.TimeSetting { From = Input.From, To = Input.To });
            }
            else
            {
                current.From = Input.From;
                current.To = Input.To;
                _settings.Update(current);
            }
            _settings.Commit();
            _audit.Log("window.change", "Setting", null, $"Order window set to {Input.From} - {Input.To}");
            TempData["Flash"] = "Order window saved.";
            return RedirectToPage();
        }

        // stored values may be "8:00" or "08:00:00"; the <input type=time> needs HH:mm
        private static string Normalize(string v) =>
            TimeSpan.TryParse(v, out var t) ? t.ToString(@"hh\:mm") : v;
    }
}
