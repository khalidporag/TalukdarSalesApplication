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
        public IndexModel(ITimeSettingRepository settings) => _settings = settings;

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
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

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
            TempData["Saved"] = "Time Setting Updated!";
            return RedirectToPage();
        }

        // stored values may be "8:00" or "08:00:00"; the <input type=time> needs HH:mm
        private static string Normalize(string v) =>
            TimeSpan.TryParse(v, out var t) ? t.ToString(@"hh\:mm") : v;
    }
}
