using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TalukdarSales.Web.Security
{
    /// <summary>
    /// Authorizes every Razor Page request against <see cref="Perm.Rules"/> before model binding.
    /// Deny by default: a page or handler without a rule is only reachable by Administrators.
    /// Anonymous requests are left to the authentication convention (challenge to /Login).
    /// </summary>
    public class PermissionPageFilter : IAsyncAuthorizationFilter
    {
        private readonly AccessService _access;
        public PermissionPageFilter(AccessService access) => _access = access;

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context.ActionDescriptor is not PageActionDescriptor page)
                return;
            if (Perm.OpenPages.Contains(page.ViewEnginePath))
                return;

            var http = context.HttpContext;
            if (http.User.Identity?.IsAuthenticated != true)
                return;   // the AuthorizeFolder convention challenges

            var isHtmx = http.Request.Headers.ContainsKey("HX-Request");
            var access = _access.For(http.User);

            if (!access.Exists)
            {
                // the account was removed after sign-in
                await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                if (isHtmx)
                {
                    http.Response.Headers["HX-Redirect"] = "/Login";
                    context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
                }
                else
                    context.Result = new RedirectToPageResult("/Login");
                return;
            }

            if (access.IsAdmin)
                return;

            // Razor Pages picks the handler from the route value, then the query string; mirror that exactly.
            var handler = context.RouteData.Values.TryGetValue("handler", out var routeHandler)
                ? routeHandler?.ToString()
                : http.Request.Query["handler"].ToString();
            if (string.IsNullOrEmpty(handler))
                handler = null;

            var required = Perm.Required(page.ViewEnginePath, handler);
            if (required != null && access.HasAny(required))
                return;

            if (isHtmx)
            {
                http.Response.Headers["HX-Trigger"] = "{\"toast\":{\"message\":\"You do not have permission to do that.\",\"success\":false}}";
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            }
            else
                context.Result = new ForbidResult();
        }
    }
}
