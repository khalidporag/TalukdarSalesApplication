using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models.Dto;
using TalukdarSales.Web.Security;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Pages.Users
{
    public class IndexModel : PageModelBase
    {
        private const int PageSize = 12;

        private readonly UserService _users;
        private readonly IUserTypeRepository _userTypes;
        private readonly IApplicationRoleRepository _roles;
        private readonly AccessService _access;

        public IndexModel(UserService users, IUserTypeRepository userTypes, IApplicationRoleRepository roles, AccessService access)
        {
            _access = access;
            _users = users;
            _userTypes = userTypes;
            _roles = roles;
        }

        // list filters
        [BindProperty(SupportsGet = true)] public string Name { get; set; }
        [BindProperty(SupportsGet = true)] public int? TypeId { get; set; }
        [BindProperty(SupportsGet = true, Name = "p")] public int PageNo { get; set; } = 1;

        [BindProperty(SupportsGet = true)] public int? EditId { get; set; }
        [BindProperty(SupportsGet = true)] public bool New { get; set; }
        /// <summary>"create", "edit" or null: which form the side panel shows.</summary>
        public string Mode { get; private set; }
        public Models.User EditUser { get; private set; }

        public Paged<UserDto> Users { get; private set; }
        public List<SelectListItem> UserTypeOptions { get; private set; }
        public List<SelectListItem> RoleOptions { get; private set; }
        public Dictionary<int, string> RoleByUser { get; private set; } = new();
        public bool CanAssignRoles => _access.For(User).Has(Perm.Roles);

        // Not [BindProperty]: bound per handler via parameters so the two forms never validate each other.
        public CreateInput Create { get; set; } = new();
        public EditInput Edit { get; set; } = new();

        public class CreateInput
        {
            [Required(ErrorMessage = "User type is required"), Range(1, int.MaxValue, ErrorMessage = "User type is required")]
            public int UserTypeId { get; set; }
            public int RoleId { get; set; }
            [Required, StringLength(100)] public string FirstName { get; set; }
            [StringLength(100)] public string LastName { get; set; }
            [Required, StringLength(30)] public string PhoneNumber { get; set; }
            [Range(0, double.MaxValue)] public double MaxCreditLimit { get; set; }
            [Range(0, int.MaxValue)] public int MaxCreditDays { get; set; }
            [StringLength(300)] public string Address { get; set; }
            [StringLength(100)] public string ContactPersonName { get; set; }
            [StringLength(30)] public string ContactPersonPhone { get; set; }
            public bool IsPayRollUser { get; set; }
            public IFormFile Image { get; set; }
        }

        public class EditInput
        {
            public int Id { get; set; }
            [Required, StringLength(100)] public string FirstName { get; set; }
            [StringLength(100)] public string LastName { get; set; }
            [Range(0, double.MaxValue)] public double MaxCreditLimit { get; set; }
            /// <summary>null = the role field was not shown/posted, leave the role alone; 0 = remove the role.</summary>
            public int? RoleId { get; set; }
        }

        public IActionResult OnGet()
        {
            LoadOptions();
            LoadUsers();
            if (EditId != null)
            {
                var user = _users.Get(EditId.Value);
                if (user == null) return NotFound();
                Edit = new EditInput
                {
                    Id = user.Id, FirstName = user.FirstName, LastName = user.LastName, MaxCreditLimit = user.MaxCreditLimit,
                    RoleId = CanAssignRoles ? _users.CurrentRoleId(user.Id) ?? 0 : null
                };
                EditUser = user;
                Mode = "edit";
            }
            else if (New) Mode = "create";
            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync([Bind(Prefix = nameof(Create))] CreateInput input)
        {
            Create = input ?? new CreateInput();

            var roleError = _users.ValidateRoleChoice(Create.RoleId, _access.For(User));
            if (roleError != null)
                ModelState.AddModelError("Create.RoleId", roleError);

            if (!ModelState.IsValid)
                return Redisplay("create");

            var dto = new CreateUserDto
            {
                UserTypeId = Create.UserTypeId,
                RoleId = Create.RoleId,
                FirstName = Create.FirstName?.Trim(),
                LastName = Create.LastName?.Trim(),
                PhoneNumber = Create.PhoneNumber?.Trim(),
                MaxCreditLimit = Create.MaxCreditLimit,
                MaxCreditDays = Create.MaxCreditDays,
                Address = Create.Address,
                ContactPersonName = Create.ContactPersonName,
                ContactPersonPhone = Create.ContactPersonPhone,
                IsPayRollUser = Create.IsPayRollUser,
                Image = Create.Image
            };

            var (ok, message) = await _users.CreateAsync(dto);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, message);
                return Redisplay("create");
            }

            TempData["Flash"] = message;
            return RedirectToPage("Index");
        }

        public IActionResult OnPostEdit([Bind(Prefix = nameof(Edit))] EditInput input)
        {
            Edit = input ?? new EditInput();

            if (!ModelState.IsValid)
                return Redisplay("edit");

            if (Edit.RoleId != null)
            {
                var (roleOk, roleError) = _users.AssignRole(Edit.Id, Edit.RoleId.Value, _access.For(User));
                if (!roleOk)
                {
                    ModelState.AddModelError("Edit.RoleId", roleError);
                    return Redisplay("edit");
                }
            }

            var updated = _users.Update(new UpdateUserDto
            {
                Id = Edit.Id,
                FirstName = Edit.FirstName?.Trim(),
                LastName = Edit.LastName?.Trim(),
                MaxCreditLimit = Edit.MaxCreditLimit
            });
            if (!updated)
                return NotFound();

            TempData["Flash"] = "Customer updated.";
            return RedirectToPage("Index");
        }

        private IActionResult Redisplay(string mode)
        {
            Mode = mode;
            LoadOptions();
            LoadUsers();
            if (mode == "edit") EditUser = _users.Get(Edit.Id);
            return Page();
        }

        private void LoadUsers()
        {
            Users = _users.Search(TypeId, Name, PageNo, PageSize);
            RoleByUser = _users.RoleNames(Users.Items.Select(u => u.Id));
        }

        private void LoadOptions()
        {
            UserTypeOptions = _userTypes.GetAll()
                .Select(t => new SelectListItem(t.TypeName, t.Id.ToString())).ToList();
            RoleOptions = _users.AssignableRoles(_access.For(User))
                .Select(r => new SelectListItem(r.Name, r.Id.ToString())).ToList();
        }
    }
}
