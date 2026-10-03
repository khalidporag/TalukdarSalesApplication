using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TalukdarSales.Web.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet() => RedirectToPage("/Users/Index");
    }
}
