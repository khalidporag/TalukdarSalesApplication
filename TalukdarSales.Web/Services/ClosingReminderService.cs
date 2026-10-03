namespace TalukdarSales.Web.Services
{
    /// <summary>Once a day, shortly before the order window closes, tells the people who take orders.</summary>
    public class ClosingReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopes;
        private readonly ILogger<ClosingReminderService> _log;

        public ClosingReminderService(IServiceScopeFactory scopes, ILogger<ClosingReminderService> log) { _scopes = scopes; _log = log; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try { await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); }
                catch (OperationCanceledException) { return; }
                try
                {
                    using var scope = _scopes.CreateScope();
                    Check(scope.ServiceProvider, DateTime.Now);
                }
                catch (Exception ex) { _log.LogWarning(ex, "Closing reminder check failed."); }
            }
        }

        /// <summary>Sends the reminder when the window closes within the configured minutes and none was sent today. Returns true when it sent.</summary>
        public static bool Check(IServiceProvider sp, DateTime now)
        {
            var minutes = sp.GetRequiredService<IConfiguration>().GetValue("Notifications:ClosingReminderMinutes", 30);
            if (minutes <= 0) return false;
            var requisitions = sp.GetRequiredService<RequisitionService>();
            var (_, to) = requisitions.GetWindow();
            if (!TimeSpan.TryParse(to, out var end) || !requisitions.IsOpenNow()) return false;
            var left = end - now.TimeOfDay;
            if (left < TimeSpan.Zero) left += TimeSpan.FromDays(1);
            if (left.TotalMinutes > minutes) return false;
            var notes = sp.GetRequiredService<NotificationService>();
            if (notes.AlreadySent("closing", now.Date)) return false;
            var orders = requisitions.OrderCountForDay(now);
            notes.Notify(Security.Perm.RequisitionCreate, "closing", $"Order desk closes in {(int)Math.Ceiling(left.TotalMinutes)} minutes",
                $"{orders} orders so far today. Last chance to take more.", "/Requisitions/Create");
            return true;
        }
    }
}
