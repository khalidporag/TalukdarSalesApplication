using System.Security.Claims;

namespace TalukdarSales.Web.Services
{
    /// <summary>The signed-in user for the current request (0 / "system" outside a request, e.g. background jobs and tests of services).</summary>
    public class CurrentUser
    {
        private readonly IHttpContextAccessor _http;
        public CurrentUser(IHttpContextAccessor http) => _http = http;

        private ClaimsPrincipal Principal => _http.HttpContext?.User;

        public int Id => int.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
        public string Name => Principal?.FindFirstValue(ClaimTypes.GivenName) ?? Principal?.Identity?.Name ?? "system";
    }
}
