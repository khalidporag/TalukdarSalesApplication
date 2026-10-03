using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TalukdarSales.Web.Infrastructure
{
    /// <summary>Shared helpers for htmx-driven pages.</summary>
    public abstract class PageModelBase : PageModel
    {
        public bool IsHtmx => Request.Headers.ContainsKey("HX-Request");

        public int CurrentUserId =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        /// <summary>Show a toast on the client (handled by wwwroot/js/app.js).</summary>
        protected void Toast(string message, bool success = true)
        {
            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new
            {
                toast = new { message, success }
            });
        }
    }
}
