using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TalukdarSales.Web.Context;
using TalukdarSales.Web.Helpers;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Security;

namespace TalukdarSales.Web.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IUserRepository _users;
        private readonly IUserRoleMappingRepository _roleMappings;
        private readonly ApplicationDbContext _db;
        private readonly AccessService _access;

        public LoginModel(IUserRepository users, IUserRoleMappingRepository roleMappings, ApplicationDbContext db, AccessService access)
        {
            _access = access;
            _users = users;
            _roleMappings = roleMappings;
            _db = db;
        }

        [BindProperty] public string Username { get; set; }
        [BindProperty] public string Password { get; set; }
        [BindProperty] public string ReturnUrl { get; set; }
        public string Error { get; private set; }

        public IActionResult OnGet(string returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return LocalRedirect(SafeReturnUrl(returnUrl));
            ReturnUrl = returnUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = string.IsNullOrWhiteSpace(Username)
                ? null
                : _users.FindBy(u => u.Username == Username.Trim()).FirstOrDefault();

            if (user == null || user.IsDeleted || !PasswordHasher.VerifyPassword(Password, user.Password))
            {
                Error = "Invalid username or password.";
                return Page();
            }

            if (PasswordHasher.NeedsRehash(user.Password))
            {
                user.Password = PasswordHasher.HashPassword(Password);
                _users.Update(user);
                _users.Commit();
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.GivenName, $"{user.FirstName} {user.LastName}".Trim())
            };

            var roleIds = _roleMappings.FindBy(m => m.UserId == user.Id).Select(m => m.RoleId).ToList();
            foreach (var role in _db.ApplicationRoles.Where(r => roleIds.Contains(r.Id)).ToList())
                claims.Add(new Claim(ClaimTypes.Role, role.Name));

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return LocalRedirect(SafeReturnUrl(ReturnUrl, _access.For(user.Id).Has(Perm.Dashboard) ? "/Dashboard" : "/"));
        }

        private string SafeReturnUrl(string url, string fallback = "/") =>
            !string.IsNullOrEmpty(url) && Url.IsLocalUrl(url) ? url : fallback;
    }
}
