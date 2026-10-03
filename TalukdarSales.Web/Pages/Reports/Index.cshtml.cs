using Microsoft.AspNetCore.Mvc;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Pages.Reports
{
    public class IndexModel : PageModelBase
    {
        private readonly AnalyticsService _analytics;
        private readonly IConfiguration _config;

        public IndexModel(AnalyticsService analytics, IConfiguration config)
        {
            _analytics = analytics; _config = config;
        }

        [BindProperty(SupportsGet = true, Name = "p")] public string PeriodKey { get; set; }
        [BindProperty(SupportsGet = true)] public bool? Compare { get; set; }

        public Overview Data { get; private set; }
        public double MonthBilled { get; private set; }
        public bool ShowCompare => Compare != false;

        // Targets are plain configuration so each business can set its own (Kpi:* in appsettings).
        public double MonthlySalesTarget => _config.GetValue("Kpi:MonthlySalesTarget", 1500000d);
        public double CollectionRateTarget => _config.GetValue("Kpi:CollectionRateTarget", 85d);
        public double DailyOrdersTarget => _config.GetValue("Kpi:DailyOrdersTarget", 0d);

        public void OnGet()
        {
            var period = Period.Parse(PeriodKey);
            if (period.Key == "today") period = Period.Parse("7");
            Data = _analytics.Build(period);
            MonthBilled = _analytics.MonthToDateBilled();
        }

        public IActionResult OnGetExport()
        {
            OnGet();
            return Excel.Sheet("KpiDaily.xlsx", "Daily", new[] { "Date", "Billed", "Collected" },
                Data.Days.Select(d => new object[] { d.Date, d.Billed, d.Collected }));
        }
    }
}
