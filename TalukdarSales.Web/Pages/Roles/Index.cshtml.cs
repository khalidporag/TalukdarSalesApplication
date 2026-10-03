using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;
using TalukdarSales.Web.Security;

namespace TalukdarSales.Web.Pages.Roles
{
    public class IndexModel : PageModelBase
    {
        private readonly IApplicationRoleRepository _roles;
        private readonly IApplicationModuleRepository _modules;
        private readonly IRoleWisePermissionRepository _permissions;
        private readonly AccessService _access;

        public IndexModel(IApplicationRoleRepository roles, IApplicationModuleRepository modules,
            IRoleWisePermissionRepository permissions, AccessService access)
        {
            _roles = roles;
            _modules = modules;
            _permissions = permissions;
            _access = access;
        }

        private static readonly List<string> CatalogKeys = Perm.Catalog.Select(p => p.Key).ToList();

        public List<ApplicationRole> Roles { get; private set; }
        /// <summary>Granted permission keys per role id.</summary>
        public Dictionary<int, HashSet<string>> RoleKeys { get; private set; }

        public RoleInput Role { get; set; } = new();
        public AssignInput Assign { get; set; } = new();
        public UserAccess Actor => _access.For(User);

        public class RoleInput { [Required, StringLength(100)] public string Name { get; set; } }

        public class AssignInput
        {
            public int RoleId { get; set; }
            public string RoleName { get; set; }
            public List<string> Keys { get; set; } = new();
        }

        [BindProperty(SupportsGet = true, Name = "role")] public int? SelectedId { get; set; }
        public ApplicationRole Selected { get; private set; }
        public HashSet<string> SelectedKeys { get; private set; } = new();

        public void OnGet() => Load();

        public IActionResult OnPostCreateRole(string name)
        {
            name = (name ?? "").Trim();
            if (name.Length == 0 || name.Length > 100)
                TempData["Flash"] = "Enter a role name (up to 100 characters).";
            else if (_roles.GetAll().Any(r => r.Name.ToLower() == name.ToLower()))
                TempData["Flash"] = "This role already exists. Please try a new role.";
            else
            {
                var role = new ApplicationRole { Name = name };
                _roles.Add(role);
                _roles.Commit();
                TempData["Flash"] = "Role added.";
                return RedirectToPage("Index", new { role = role.Id });
            }
            return RedirectToPage("Index");
        }

        /// <summary>
        /// Replaces the role's permissions with the ticked ones. Non-administrators can only change permissions they
        /// hold themselves (no way to grant yourself more than you have); everything else stays as it was.
        /// </summary>
        public IActionResult OnPostAssign([Bind(Prefix = nameof(Assign))] AssignInput input)
        {
            var role = input == null ? null : _roles.GetSingle(input.RoleId);
            if (role == null)
                return NotFound();
            if (AccessService.IsAdministratorRole(role.Name))
            {
                TempData["Flash"] = "The Administrator role always has full access.";
                return RedirectToPage("Index", new { role = role.Id });
            }

            var actor = Actor;
            bool Changeable(string key) => actor.IsAdmin || actor.Keys.Contains(key);

            var posted = (input.Keys ?? new()).Where(k => Perm.Find(k) != null).ToHashSet();
            var existing = GrantedKeys(role.Id);
            var wanted = Perm.Catalog.Select(p => p.Key)
                .Where(k => Changeable(k) ? posted.Contains(k) : existing.Contains(k))
                .ToHashSet();

            var moduleIdByKey = _modules.GetAll().Where(m => CatalogKeys.Contains(m.Url))
                .ToList().GroupBy(m => m.Url).ToDictionary(g => g.Key, g => g.First().Id);
            var rows = _permissions.GetAll().Where(p => p.RoleId == role.Id).ToList();
            var idToKey = moduleIdByKey.ToDictionary(kv => kv.Value, kv => kv.Key);

            foreach (var row in rows)
            {
                // drop rows for permissions no longer wanted, and legacy rows pointing at non-catalog modules stay untouched
                if (idToKey.TryGetValue(row.ModuleId, out var key) && !wanted.Contains(key))
                    _permissions.Delete(row);
            }
            var have = rows.Where(r => idToKey.ContainsKey(r.ModuleId)).Select(r => idToKey[r.ModuleId]).ToHashSet();
            var toAdd = wanted.Where(k => !have.Contains(k) && moduleIdByKey.ContainsKey(k))
                .Select(k => new RoleWisePermission { RoleId = role.Id, ModuleId = moduleIdByKey[k] }).ToList();
            if (toAdd.Count > 0)
                _permissions.AddRange(toAdd);
            _permissions.Commit();
            _access.Invalidate();

            TempData["Flash"] = "Permissions saved.";
            return RedirectToPage("Index", new { role = role.Id });
        }

        private HashSet<string> GrantedKeys(int roleId)
        {
            // permissions granted to the role whose module is one of the catalog entries (legacy modules are ignored)
            return _permissions.GetAll().Where(p => p.RoleId == roleId)
                .Join(_modules.GetAll().Where(m => CatalogKeys.Contains(m.Url)), p => p.ModuleId, m => m.Id, (p, m) => m.Url)
                .ToList().ToHashSet();
        }

        private void Load()
        {
            Roles = _roles.GetAll().ToList().OrderBy(r => !AccessService.IsAdministratorRole(r.Name)).ThenBy(r => r.Name).ToList();
            var roleIds = Roles.Select(r => r.Id).ToList();
            var grants = _permissions.GetAll().Where(p => roleIds.Contains(p.RoleId))
                .Join(_modules.GetAll().Where(m => CatalogKeys.Contains(m.Url)), p => p.ModuleId, m => m.Id, (p, m) => new { p.RoleId, Key = m.Url })
                .ToList();
            RoleKeys = Roles.ToDictionary(r => r.Id, r => grants.Where(g => g.RoleId == r.Id).Select(g => g.Key).ToHashSet());
            Selected = Roles.FirstOrDefault(r => r.Id == SelectedId) ?? Roles.FirstOrDefault(r => !AccessService.IsAdministratorRole(r.Name)) ?? Roles.FirstOrDefault();
            SelectedKeys = Selected == null ? new() : RoleKeys[Selected.Id];
        }
    }
}
