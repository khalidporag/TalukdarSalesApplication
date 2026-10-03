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

        public void OnGet()
        {
            LoadOptions();
            LoadUsers();
        }

        public IActionResult OnGetList()
        {
            LoadUsers();
            return Partial("_UserList", this);
        }

        public IActionResult OnGetCreate()
        {
            LoadOptions();
            return Partial("_CreateForm", this);
        }

        public async Task<IActionResult> OnPostCreateAsync([Bind(Prefix = nameof(Create))] CreateInput input)
        {
            Create = input ?? new CreateInput();

            var roleError = _users.ValidateRoleChoice(Create.RoleId, _access.For(User));
            if (roleError != null)
                ModelState.AddModelError("Create.RoleId", roleError);

            if (!ModelState.IsValid)
            {
                LoadOptions();
                return Partial("_CreateForm", this);
            }

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
                LoadOptions();
                return Partial("_CreateForm", this);
            }

            Toast(message);
            CloseModal();
            RefreshList();
            return new EmptyResult();
        }

        public IActionResult OnGetEdit(int id)
        {
            var user = _users.Get(id);
            if (user == null)
                return NotFound();
            Edit = new EditInput
            {
                Id = user.Id, FirstName = user.FirstName, LastName = user.LastName, MaxCreditLimit = user.MaxCreditLimit,
                RoleId = CanAssignRoles ? _users.CurrentRoleId(user.Id) ?? 0 : null
            };
            LoadOptions();
            return Partial("_EditForm", this);
        }

        public IActionResult OnPostEdit([Bind(Prefix = nameof(Edit))] EditInput input)
        {
            Edit = input ?? new EditInput();

            if (!ModelState.IsValid)
            {
                LoadOptions();
                return Partial("_EditForm", this);
            }

            if (Edit.RoleId != null)
            {
                var (roleOk, roleError) = _users.AssignRole(Edit.Id, Edit.RoleId.Value, _access.For(User));
                if (!roleOk)
                {
                    ModelState.AddModelError("Edit.RoleId", roleError);
                    LoadOptions();
                    return Partial("_EditForm", this);
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

            Toast("User Updated!");
            CloseModal();
            RefreshList();
            return new EmptyResult();
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
