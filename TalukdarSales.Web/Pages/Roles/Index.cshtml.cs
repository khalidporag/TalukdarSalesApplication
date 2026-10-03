using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;

namespace TalukdarSales.Web.Pages.Roles
{
    public class IndexModel : PageModelBase
    {
        private readonly IApplicationRoleRepository _roles;
        private readonly IApplicationModuleRepository _modules;
        private readonly IRoleWisePermissionRepository _permissions;

        public IndexModel(IApplicationRoleRepository roles, IApplicationModuleRepository modules, IRoleWisePermissionRepository permissions)
        {
            _roles = roles;
            _modules = modules;
            _permissions = permissions;
        }

        public List<ApplicationRole> Roles { get; private set; }
        public List<ApplicationModule> Modules { get; private set; }
        public Dictionary<int, List<int>> RoleModuleIds { get; private set; }

        public RoleInput Role { get; set; } = new();
        public ModuleInput Module { get; set; } = new();
        public AssignInput Assign { get; set; } = new();

        public class RoleInput { [Required, StringLength(100)] public string Name { get; set; } }

        public class ModuleInput
        {
            [Required, StringLength(100)] public string Name { get; set; }
            [StringLength(200)] public string Url { get; set; }
        }

        public class AssignInput
        {
            public int RoleId { get; set; }
            public string RoleName { get; set; }
            public List<int> ModuleIds { get; set; } = new();
        }

        public void OnGet() => Load();

        public IActionResult OnGetList()
        {
            Load();
            return Partial("_List", this);
        }

        public IActionResult OnGetCreateRole() => Partial("_RoleForm", this);

        public IActionResult OnPostCreateRole([Bind(Prefix = nameof(Role))] RoleInput input)
        {
            Role = input ?? new RoleInput();
            if (!ModelState.IsValid)
                return Partial("_RoleForm", this);

            var name = Role.Name.Trim();
            if (_roles.FindBy(r => r.Name == name && !r.IsDeleted).Any())
            {
                ModelState.AddModelError("Role.Name", "This role already exists. Please try a new role.");
                return Partial("_RoleForm", this);
            }
            _roles.Add(new ApplicationRole { Name = name });
            _roles.Commit();
            Toast("Role Added!");
            CloseModal();
            RefreshList();
            return new EmptyResult();
        }

        public IActionResult OnGetCreateModule() => Partial("_ModuleForm", this);

        public IActionResult OnPostCreateModule([Bind(Prefix = nameof(Module))] ModuleInput input)
        {
            Module = input ?? new ModuleInput();
            if (!ModelState.IsValid)
                return Partial("_ModuleForm", this);
            _modules.Add(new ApplicationModule { Name = Module.Name.Trim(), Url = Module.Url?.Trim() });
            _modules.Commit();
            Toast("Module Added!");
            CloseModal();
            RefreshList();
            return new EmptyResult();
        }

        public IActionResult OnGetAssign(int id)
        {
            var role = _roles.GetSingle(id);
            if (role == null)
                return NotFound();
            Modules = _modules.GetAll().OrderBy(m => m.Name).ToList();
            Assign = new AssignInput
            {
                RoleId = role.Id,
                RoleName = role.Name,
                ModuleIds = _permissions.GetAll().Where(p => p.RoleId == id).Select(p => p.ModuleId).ToList()
            };
            return Partial("_AssignForm", this);
        }

        // Replaces the role's module set with the ticked modules (the old API only ever appended, creating duplicates).
        public IActionResult OnPostAssign([Bind(Prefix = nameof(Assign))] AssignInput input)
        {
            if (input == null || _roles.GetSingle(input.RoleId) == null)
                return NotFound();

            var wanted = (input.ModuleIds ?? new()).Distinct().ToHashSet();
            var existing = _permissions.GetAll().Where(p => p.RoleId == input.RoleId).ToList();

            foreach (var p in existing.Where(p => !wanted.Contains(p.ModuleId)))
                _permissions.Delete(p);

            var have = existing.Select(p => p.ModuleId).ToHashSet();
            var toAdd = wanted.Where(m => !have.Contains(m))
                .Select(m => new RoleWisePermission { RoleId = input.RoleId, ModuleId = m }).ToList();
            if (toAdd.Count > 0)
                _permissions.AddRange(toAdd);
            _permissions.Commit();

            Toast("Modules updated for role!");
            CloseModal();
            RefreshList();
            return new EmptyResult();
        }

        private void Load()
        {
            Roles = _roles.GetAll().OrderBy(r => r.Name).ToList();
            Modules = _modules.GetAll().OrderBy(m => m.Name).ToList();
            RoleModuleIds = _permissions.GetAll().GroupBy(p => p.RoleId)
                .ToDictionary(g => g.Key, g => g.Select(p => p.ModuleId).ToList());
        }
    }
}
