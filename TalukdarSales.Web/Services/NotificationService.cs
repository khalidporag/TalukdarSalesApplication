using TalukdarSales.Web.Context;
using TalukdarSales.Web.Models;
using TalukdarSales.Web.Security;

namespace TalukdarSales.Web.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _db;
        private readonly AccessService _access;
        private readonly CurrentUser _user;

        public NotificationService(ApplicationDbContext db, AccessService access, CurrentUser user)
        {
            _db = db; _access = access; _user = user;
        }

        /// <summary>Tells everyone who holds <paramref name="permission"/> (administrators included), except the person who caused it.</summary>
        public int Notify(string permission, string kind, string title, string body, string link)
        {
            var actor = _user.Id;
            var recipients = _access.UserIdsWith(permission).Where(id => id != actor).ToList();
            var now = DateTime.Now;
            foreach (var id in recipients)
                _db.Notifications.Add(new Notification { UserId = id, Kind = kind, Title = title, Body = body, Link = link, CreatedOn = now });
            if (recipients.Count > 0) _db.SaveChanges();
            return recipients.Count;
        }

        /// <summary>True when the recipient already got this kind of notification since <paramref name="since"/> (lets a job notify once).</summary>
        public bool AlreadySent(string kind, DateTime since) =>
            _db.Notifications.Any(n => !n.IsDeleted && n.Kind == kind && n.CreatedOn >= since);

        public int UnreadCount(int userId) => _db.Notifications.Count(n => !n.IsDeleted && n.UserId == userId && !n.IsRead);

        public List<Notification> Recent(int userId, int take = 50) =>
            _db.Notifications.Where(n => !n.IsDeleted && n.UserId == userId).OrderByDescending(n => n.CreatedOn).ThenByDescending(n => n.Id).Take(take).ToList();

        public void MarkRead(int userId, int id)
        {
            var n = _db.Notifications.FirstOrDefault(x => x.Id == id && x.UserId == userId);
            if (n == null || n.IsRead) return;
            n.IsRead = true; _db.SaveChanges();
        }

        public void MarkAllRead(int userId)
        {
            foreach (var n in _db.Notifications.Where(x => x.UserId == userId && !x.IsRead && !x.IsDeleted).ToList()) n.IsRead = true;
            _db.SaveChanges();
        }
    }
}
