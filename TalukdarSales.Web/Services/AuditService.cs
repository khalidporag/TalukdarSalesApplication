using TalukdarSales.Web.Context;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Models;

namespace TalukdarSales.Web.Services
{
    public record AuditFilter(string Search, string Entity, DateTime? From, DateTime? To);

    public class AuditService
    {
        private readonly ApplicationDbContext _db;
        private readonly CurrentUser _user;

        public AuditService(ApplicationDbContext db, CurrentUser user) { _db = db; _user = user; }

        /// <summary>Records an action by the signed-in user. Saves immediately, so call it after the change it describes succeeded.</summary>
        public void Log(string action, string entity, int? entityId, string summary)
        {
            _db.AuditLogs.Add(new AuditLog
            {
                UserId = _user.Id, UserName = _user.Name, Action = action, Entity = entity, EntityId = entityId,
                Summary = summary?.Length > 500 ? summary[..500] : summary, CreatedOn = DateTime.Now
            });
            _db.SaveChanges();
        }

        public static readonly string[] Entities = { "Order", "Invoice", "Payment", "Credit note", "Product", "Customer", "Role", "Notice", "Setting" };

        public IQueryable<AuditLog> Query(AuditFilter f)
        {
            var q = _db.AuditLogs.Where(a => !a.IsDeleted);
            if (!string.IsNullOrWhiteSpace(f.Entity)) q = q.Where(a => a.Entity == f.Entity);
            if (f.From != null) { var start = f.From.Value.Date; q = q.Where(a => a.CreatedOn >= start); }
            if (f.To != null) { var end = f.To.Value.Date.AddDays(1); q = q.Where(a => a.CreatedOn < end); }
            if (!string.IsNullOrWhiteSpace(f.Search))
            {
                var t = f.Search.Trim().ToLower();
                q = q.Where(a => (a.Summary != null && a.Summary.ToLower().Contains(t)) || (a.UserName != null && a.UserName.ToLower().Contains(t)) || (a.Action != null && a.Action.ToLower().Contains(t)));
            }
            return q.OrderByDescending(a => a.CreatedOn).ThenByDescending(a => a.Id);
        }

        public Paged<AuditLog> Page(AuditFilter f, int page, int size) => Paged<AuditLog>.Create(Query(f), page, size);
    }
}
