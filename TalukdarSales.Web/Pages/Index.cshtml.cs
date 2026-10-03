using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;
using TalukdarSales.Web.Security;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Pages
{
    /// <summary>The notice feed everyone sees; people with the Notices permission can also post, edit and remove.</summary>
    public class IndexModel : PageModelBase
    {
        private readonly INoticeRepository _notices;
        private readonly ImageStore _images;
        private readonly AccessService _access;

        public IndexModel(INoticeRepository notices, ImageStore images, AccessService access)
        {
            _notices = notices;
            _images = images;
            _access = access;
        }

        public List<Notice> Items { get; private set; }
        public bool CanManage => _access.For(User).Has(Perm.Notices);

        [BindProperty(SupportsGet = true, Name = "edit")] public int? EditId { get; set; }
        [BindProperty] public FormInput Form { get; set; } = new();
        [TempData] public string Flash { get; set; }

        public class FormInput
        {
            public int Id { get; set; }
            [Required, StringLength(200)] public string Title { get; set; }
            [Required, StringLength(4000)] public string Description { get; set; }
            public IFormFile Image { get; set; }
        }

        public void OnGet()
        {
            Load();
            if (EditId != null && CanManage)
            {
                var n = _notices.GetSingle(EditId.Value);
                if (n != null)
                    Form = new FormInput { Id = n.Id, Title = n.Title, Description = n.Description };
            }
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ImageStore.IsValid(Form.Image))
                ModelState.AddModelError("Form.Image", "Invalid image. Allowed: jpg, png, gif, webp up to 5 MB.");

            var title = Form.Title?.Trim();
            if (!string.IsNullOrEmpty(title) && _notices.GetAll().Any(x => x.Title == title && x.Id != Form.Id))
                ModelState.AddModelError("Form.Title", "A notice with this title already exists.");

            if (!ModelState.IsValid)
            {
                Load();
                return Page();
            }

            if (Form.Id > 0)
            {
                var n = _notices.GetSingle(Form.Id);
                if (n == null)
                    return NotFound();
                n.Title = title;
                n.Description = Form.Description;
                if (Form.Image != null)
                    n.LogoName = await _images.SaveAsync(Form.Image, "notices");
                _notices.Update(n);
                Flash = "Notice updated";
            }
            else
            {
                _notices.Add(new Notice { Title = title, Description = Form.Description, LogoName = await _images.SaveAsync(Form.Image, "notices") });
                Flash = "Notice published";
            }
            _notices.Commit();
            return RedirectToPage();
        }

        public IActionResult OnPostRemove(int id)
        {
            var n = _notices.GetSingle(id);
            if (n == null)
                return NotFound();
            _notices.Delete(n);
            _notices.Commit();
            Flash = "Notice removed";
            return RedirectToPage();
        }

        private void Load() => Items = _notices.GetAll().OrderByDescending(n => n.CreatedOn).ThenByDescending(n => n.Id).ToList();
    }
}
