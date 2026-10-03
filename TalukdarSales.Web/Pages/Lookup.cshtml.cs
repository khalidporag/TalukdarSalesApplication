using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Pages
{
    /// <summary>Tiny htmx endpoints returning &lt;option&gt; lists for dependent dropdowns.</summary>
    public class LookupModel : PageModel
    {
        private readonly IUserRepository _users;
        private readonly RequisitionService _requisitions;

        public LookupModel(IUserRepository users, RequisitionService requisitions)
        {
            _users = users;
            _requisitions = requisitions;
        }

        public IActionResult OnGetUsers(int? typeId, string placeholder = "All users")
        {
            var sb = Option("", placeholder);
            foreach (var u in _users.GetAll().Where(u => typeId == null || u.UserTypeId == typeId).OrderBy(u => u.FirstName).ThenBy(u => u.LastName))
                sb.Append(Option(u.Id.ToString(), $"{u.FirstName} {u.LastName} ({u.Username})".Replace("  ", " ")));
            return Html(sb);
        }

        public IActionResult OnGetRequisitions(int? userId, string placeholder = "Select requisition")
        {
            var sb = Option("", placeholder);
            if (userId != null)
                foreach (var r in _requisitions.List(true, userId, null, null, null))
                    sb.Append(Option(r.Id.ToString(), $"{r.Serial} - {r.CreatedDateTime:dd MMM yyyy HH:mm}"));
            return Html(sb);
        }

        private static StringBuilder Option(string value, string text) =>
            new StringBuilder($"<option value=\"{WebUtility.HtmlEncode(value)}\">{WebUtility.HtmlEncode(text)}</option>");

        private IActionResult Html(StringBuilder sb) => Content(sb.ToString(), "text/html");
    }

}
