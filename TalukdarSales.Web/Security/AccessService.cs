using System.Security.Claims;
using TalukdarSales.Web.Context;
using TalukdarSales.Web.Models;

namespace TalukdarSales.Web.Security
{
    public class UserAccess
    {
        public static readonly UserAccess None = new(false, false, new HashSet<string>());

        public UserAccess(bool exists, bool isAdmin, IReadOnlySet<string> keys)
        {
            Exists = exists;
            IsAdmin = isAdmin;
            Keys = keys;
        }

        /// <summary>False when the user was deleted since the cookie was issued.</summary>
        public bool Exists { get; }
        public bool IsAdmin { get; }
        public IReadOnlySet<string> Keys { get; }

        public bool Has(string key) => Exists && (IsAdmin || Keys.Contains(key));
        public bool HasAny(IEnumerable<string> keys) => Exists && (IsAdmin || keys.Any(Keys.Contains));
    }

    /// <summary>
    /// Resolves what a user may do from the database on every request, so role changes apply immediately
    /// (no stale cookie claims). Cached per request (scoped).
    /// </summary>
    public class AccessService
    {
        private readonly ApplicationDbContext _db;
        private readonly Dictionary<int, UserAccess> _cache = new();

        public AccessService(ApplicationDbContext db) => _db = db;

        public static bool IsAdministratorRole(string roleName) =>
            string.Equals(roleName?.Trim(), Perm.AdministratorRole, StringComparison.OrdinalIgnoreCase);

        public UserAccess For(ClaimsPrincipal principal) =>
            int.TryParse(principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? For(id) : UserAccess.None;

        public UserAccess For(int userId)
        {
            if (_cache.TryGetValue(userId, out var cached))
                return cached;

            UserAccess access;
            if (!_db.Users.Any(u => u.Id == userId && !u.IsDeleted))
                access = UserAccess.None;
            else
            {
                var roleIds = _db.UserRoleMappings.Where(m => m.UserId == userId && !m.IsDeleted).Select(m => m.RoleId).ToList();
                var roles = _db.ApplicationRoles.Where(r => roleIds.Contains(r.Id) && !r.IsDeleted).Select(r => new { r.Id, r.Name }).ToList();
                var activeRoleIds = roles.Select(r => r.Id).ToList();

                var keys = _db.RoleWisePermissions.Where(p => activeRoleIds.Contains(p.RoleId) && !p.IsDeleted)
                    .Join(_db.ApplicationModules.Where(m => !m.IsDeleted), p => p.ModuleId, m => m.Id, (p, m) => m.Url)
                    .ToList()
                    .Where(k => k != null && Perm.Find(k) != null)
                    .ToHashSet();

                access = new UserAccess(true, roles.Any(r => IsAdministratorRole(r.Name)), keys);
            }
            _cache[userId] = access;
            return access;
        }

        /// <summary>Active users currently holding the Administrator role.</summary>
        public List<int> AdministratorUserIds()
        {
            var adminRoleIds = _db.ApplicationRoles.Where(r => !r.IsDeleted).AsEnumerable()
                .Where(r => IsAdministratorRole(r.Name)).Select(r => r.Id).ToList();
            var userIds = _db.UserRoleMappings.Where(m => !m.IsDeleted && adminRoleIds.Contains(m.RoleId)).Select(m => m.UserId).Distinct().ToList();
            return _db.Users.Where(u => !u.IsDeleted && userIds.Contains(u.Id)).Select(u => u.Id).ToList();
        }

        public void Invalidate() => _cache.Clear();
    }

    /// <summary>Startup sync: permission catalog into ApplicationModules, the Administrator role, and a first administrator.</summary>
    public class AccessSeeder
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<AccessSeeder> _log;

        public AccessSeeder(ApplicationDbContext db, IConfiguration config, ILogger<AccessSeeder> log)
        {
            _db = db;
            _config = config;
            _log = log;
        }

        public void Run()
        {
            var now = DateTime.Now;

            foreach (var perm in Perm.Catalog)
            {
                var module = _db.ApplicationModules.FirstOrDefault(m => m.Url == perm.Key);
                if (module == null)
                    _db.ApplicationModules.Add(new ApplicationModule { Name = perm.Name, Url = perm.Key, CreatedOn = now });
                else
                {
                    module.Name = perm.Name;
                    module.IsDeleted = false;
                }
            }

            var adminRole = _db.ApplicationRoles.AsEnumerable().FirstOrDefault(r => !r.IsDeleted && AccessService.IsAdministratorRole(r.Name));
            if (adminRole == null)
            {
                adminRole = new ApplicationRole { Name = Perm.AdministratorRole, CreatedOn = now };
                _db.ApplicationRoles.Add(adminRole);
            }
            _db.SaveChanges();

            var hasAdmin = _db.UserRoleMappings.Any(m => !m.IsDeleted && m.RoleId == adminRole.Id
                && _db.Users.Any(u => u.Id == m.UserId && !u.IsDeleted));
            if (hasAdmin)
                return;

            // Nobody can manage roles yet (first start after enabling access control): promote one user.
            var username = _config["Security:InitialAdmin"];
            var candidate = !string.IsNullOrWhiteSpace(username)
                ? _db.Users.FirstOrDefault(u => !u.IsDeleted && u.Username == username)
                : _db.Users.Where(u => !u.IsDeleted).OrderBy(u => u.Id).FirstOrDefault();
            if (candidate == null)
            {
                _log.LogWarning("No Administrator exists and no user could be promoted. Create a user, then restart, or set Security:InitialAdmin.");
                return;
            }

            _db.UserRoleMappings.Add(new UserRoleMapping { UserId = candidate.Id, RoleId = adminRole.Id, CreatedOn = now });
            _db.SaveChanges();
            _log.LogWarning("No Administrator existed; granted the Administrator role to user '{User}'. Review this in Role Management.", candidate.Username);
        }
    }
}
