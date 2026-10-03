using TalukdarSales.Web.Context;
using TalukdarSales.Web.Helpers;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;
using TalukdarSales.Web.Models.Dto;

namespace TalukdarSales.Web.Services
{
    public class UserService
    {
        private const string UserIdPrefix = "1981-";

        private readonly IUserRepository _users;
        private readonly IUserTypeRepository _userTypes;
        private readonly IUserRoleMappingRepository _roleMappings;
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public UserService(IUserRepository users, IUserTypeRepository userTypes,
            IUserRoleMappingRepository roleMappings, ApplicationDbContext db, IWebHostEnvironment env)
        {
            _users = users;
            _userTypes = userTypes;
            _roleMappings = roleMappings;
            _db = db;
            _env = env;
        }

        public List<UserDto> Search(int? userTypeId, string name)
        {
            IEnumerable<User> list = _users.GetAll().OrderByDescending(n => n.CreatedOn);
            if (userTypeId != null)
                list = list.Where(n => n.UserTypeId == userTypeId);
            if (!string.IsNullOrWhiteSpace(name))
            {
                var term = name.Trim().ToLowerInvariant();
                list = list.Where(n =>
                    (n.FirstName ?? "").ToLowerInvariant().Contains(term) ||
                    (n.LastName ?? "").ToLowerInvariant().Contains(term) ||
                    (n.PhoneNumber ?? "").ToLowerInvariant().Contains(term) ||
                    (n.Username ?? "").ToLowerInvariant().Contains(term));
            }

            var types = _userTypes.GetAll().ToDictionary(n => n.Id);
            return list.Select(n => new UserDto
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
            }).ToList();
        }

        public User Get(int id) => _users.GetSingle(id);

        public async Task<(bool Ok, string Message)> CreateAsync(CreateUserDto input)
        {
            if (input == null)
                return (false, "Invalid request.");
            if (input.Image != null && !UploadValidator.IsValidImage(input.Image))
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
                ImageName = await SaveImageAsync(input.Image),
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

        // Next number based on the highest issued id (including deleted users), not on the row count.
        private string NextUserNumber()
        {
            var max = _db.Users
                .Select(u => u.SequencialUserId)
                .AsEnumerable()
                .Where(id => id != null && id.StartsWith(UserIdPrefix))
                .Select(id => int.TryParse(id.Substring(UserIdPrefix.Length), out var n) ? n : 0)
                .DefaultIfEmpty(0)
                .Max();
            return (max + 1).ToString("D4");
        }

        private async Task<string> SaveImageAsync(IFormFile image)
        {
            if (image == null)
                return "";
            var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName).ToLowerInvariant();
            var dir = Path.Combine(_env.WebRootPath, "images", "users");
            Directory.CreateDirectory(dir);
            await using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
            await image.CopyToAsync(stream);
            return fileName;
        }
    }
}
