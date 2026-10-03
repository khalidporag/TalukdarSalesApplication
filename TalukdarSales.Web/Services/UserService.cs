using TalukdarSales.Web.Context;
using TalukdarSales.Web.Helpers;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;
using TalukdarSales.Web.Models.Dto;
using TalukdarSales.Web.Security;

namespace TalukdarSales.Web.Services
{
    public class UserService
    {
        private const string UserIdPrefix = "1981-";

        private readonly IUserRepository _users;
        private readonly IUserTypeRepository _userTypes;
        private readonly IUserRoleMappingRepository _roleMappings;
        private readonly ApplicationDbContext _db;
        private readonly ImageStore _images;
        private readonly AccessService _access;

        public UserService(IUserRepository users, IUserTypeRepository userTypes,
            IUserRoleMappingRepository roleMappings, ApplicationDbContext db, ImageStore images, AccessService access)
        {
            _access = access;
            _users = users;
            _userTypes = userTypes;
            _roleMappings = roleMappings;
            _db = db;
            _images = images;
        }

        /// <summary>One page of users, newest first. Filtering, counting and paging all run in SQL.</summary>
        public Paged<UserDto> Search(int? userTypeId, string name, int page, int pageSize)
        {
            var q = _users.GetAll();
            if (userTypeId != null)
                q = q.Where(n => n.UserTypeId == userTypeId);
            if (!string.IsNullOrWhiteSpace(name))
            {
                var term = name.Trim().ToLower();
                q = q.Where(n =>
                    n.FirstName.ToLower().Contains(term) ||
                    n.LastName.ToLower().Contains(term) ||
                    n.PhoneNumber.ToLower().Contains(term) ||
                    n.Username.ToLower().Contains(term));
            }

            var paged = Paged<User>.Create(q.OrderByDescending(n => n.CreatedOn).ThenByDescending(n => n.Id), page, pageSize);
            var types = _userTypes.GetAll().ToDictionary(n => n.Id);
            return new Paged<UserDto>
            {
                Page = paged.Page,
                PageSize = paged.PageSize,
                Total = paged.Total,
                Items = paged.Items.Select(n => new UserDto
                {
                    Id = n.Id,
                    CreatedOn = n.CreatedOn,
                    DeletedOn = n.DeletedOn,
                    SequencialUserId = n.SequencialUserId,
                    UserTypeId = n.UserTypeId,
                    UserTypeName = types.TryGetValue(n.UserTypeId, out var t) ? t.TypeName : "",
                    FirstName = n.FirstName,
                    LastName = n.LastName,
                    ImageName = n.ImageName,
                    PhoneNumber = n.PhoneNumber,
                    DueAmount = n.DueAmount,
                    Username = n.Username,
                    MaxCreditDays = n.MaxCreditDays,
                    MaxCreditLimit = n.MaxCreditLimit,
                    Address = n.Address,
                    ContactPersonName = n.ContactPersonName,
                    ContactPersonPhone = n.ContactPersonPhone,
                    IsPayRollUser = n.IsPayRollUser
                }).ToList()
            };
        }

        public User Get(int id) => _users.GetSingle(id);

        public async Task<(bool Ok, string Message)> CreateAsync(CreateUserDto input)
        {
            if (input == null)
                return (false, "Invalid request.");
            if (!ImageStore.IsValid(input.Image))
                return (false, "Invalid image. Allowed: jpg, jpeg, png, gif, webp up to 5 MB.");

            var sequence = NextUserNumber();
            var user = new User
            {
                SequencialUserId = UserIdPrefix + sequence,
                Username = UserIdPrefix + sequence,
                UserTypeId = input.UserTypeId,
                FirstName = input.FirstName,
                LastName = input.LastName,
                PhoneNumber = input.PhoneNumber,
                DueAmount = input.DueAmount,
                MaxCreditLimit = input.MaxCreditLimit,
                MaxCreditDays = input.MaxCreditDays,
                Address = input.Address,
                ContactPersonName = input.ContactPersonName,
                ContactPersonPhone = input.ContactPersonPhone,
                IsPayRollUser = input.IsPayRollUser,
                RefreshToken = input.RefreshToken,
                RefreshTokenExpiryTime = input.RefreshTokenExpiryTime,
                ImageName = await _images.SaveAsync(input.Image, "users"),
                Token = ""
            };

            if (_users.FindBy(x => x.Username == user.Username).Any())
                return (false, "Username Already Exist");

            // initial password is the phone number (existing behaviour)
            user.Password = PasswordHasher.HashPassword(user.PhoneNumber ?? "");
            _users.Add(user);
            _users.Commit();

            if (user.Id > 0 && input.RoleId > 0)
            {
                _roleMappings.Add(new UserRoleMapping { UserId = user.Id, RoleId = input.RoleId });
                _roleMappings.Commit();
            }

            return (true, "User Added!");
        }

        public bool Update(UpdateUserDto input)
        {
            var user = _users.GetSingle(input.Id);
            if (user == null)
                return false;
            user.FirstName = input.FirstName;
            user.LastName = input.LastName;
            user.MaxCreditLimit = input.MaxCreditLimit;
            _users.Update(user);
            _users.Commit();
            return true;
        }

        /// <summary>Role name(s) for the given users, for display.</summary>
        public Dictionary<int, string> RoleNames(IEnumerable<int> userIds)
        {
            var ids = userIds.ToList();
            var rows = _roleMappings.GetAll().Where(m => ids.Contains(m.UserId))
                .Join(_db.ApplicationRoles.Where(r => !r.IsDeleted), m => m.RoleId, r => r.Id, (m, r) => new { m.UserId, r.Name })
                .ToList();
            return rows.GroupBy(x => x.UserId).ToDictionary(g => g.Key, g => string.Join(", ", g.Select(x => x.Name)));
        }

        public int? CurrentRoleId(int userId) =>
            _roleMappings.GetAll().Where(m => m.UserId == userId).Select(m => (int?)m.RoleId).FirstOrDefault();

        /// <summary>Roles the actor may hand out: everything for administrators, everything but Administrator otherwise.</summary>
        public List<ApplicationRole> AssignableRoles(UserAccess actor) =>
            actor.Has(Perm.Roles)
                ? _db.ApplicationRoles.Where(r => !r.IsDeleted).AsEnumerable()
                    .Where(r => actor.IsAdmin || !AccessService.IsAdministratorRole(r.Name))
                    .OrderBy(r => r.Name).ToList()
                : new List<ApplicationRole>();

        /// <summary>Checks the actor may give <paramref name="roleId"/> (0 = no role) to somebody.</summary>
        public string ValidateRoleChoice(int roleId, UserAccess actor)
        {
            if (roleId <= 0)
                return null;
            if (!actor.Has(Perm.Roles))
                return "You do not have permission to assign roles.";
            var role = _db.ApplicationRoles.FirstOrDefault(r => r.Id == roleId && !r.IsDeleted);
            if (role == null)
                return "Role not found.";
            if (AccessService.IsAdministratorRole(role.Name) && !actor.IsAdmin)
                return "Only administrators can grant the Administrator role.";
            return null;
        }

        /// <summary>Set the user's single role (0 = none) with escalation and lock-out guards.</summary>
        public (bool Ok, string Error) AssignRole(int userId, int roleId, UserAccess actor)
        {
            if (!actor.Has(Perm.Roles))
                return (false, "You do not have permission to assign roles.");
            if (_users.GetSingle(userId) == null)
                return (false, "User not found.");
            var error = ValidateRoleChoice(roleId, actor);
            if (error != null)
                return (false, error);

            var adminRoleIds = _db.ApplicationRoles.Where(r => !r.IsDeleted).AsEnumerable()
                .Where(r => AccessService.IsAdministratorRole(r.Name)).Select(r => r.Id).ToHashSet();
            var current = _roleMappings.GetAll().Where(m => m.UserId == userId).ToList();
            var isAdminNow = current.Any(m => adminRoleIds.Contains(m.RoleId));
            var staysAdmin = adminRoleIds.Contains(roleId);

            if (isAdminNow && !staysAdmin)
            {
                if (!actor.IsAdmin)
                    return (false, "Only administrators can change an administrator's role.");
                var admins = _access.AdministratorUserIds();
                if (admins.Count <= 1 && admins.Contains(userId))
                    return (false, "This is the last administrator. Make another user an administrator first.");
            }

            if (current.Count == 1 && current[0].RoleId == roleId)
                return (true, null);

            foreach (var m in current)
                _roleMappings.Delete(m);
            if (roleId > 0)
                _roleMappings.Add(new UserRoleMapping { UserId = userId, RoleId = roleId });
            _roleMappings.Commit();
            _access.Invalidate();
            return (true, null);
        }

        // Next number based on the highest issued id (including deleted users), not on the row count.
        private string NextUserNumber()
        {
            // ids are fixed-width (1981-0001); order by length first so 1981-10000 sorts after 1981-9999. Includes deleted users.
            var last = _db.Users
                .Where(u => u.SequencialUserId != null && u.SequencialUserId.StartsWith(UserIdPrefix))
                .OrderByDescending(u => u.SequencialUserId.Length).ThenByDescending(u => u.SequencialUserId)
                .Select(u => u.SequencialUserId)
                .FirstOrDefault();
            var max = last != null && int.TryParse(last.Substring(UserIdPrefix.Length), out var n) ? n : 0;
            return (max + 1).ToString("D4");
        }
    }
}
