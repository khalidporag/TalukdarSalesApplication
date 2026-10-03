using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Pages.Notices
{
    public class IndexModel : PageModelBase
    {
        private readonly INoticeRepository _notices;
        private readonly ImageStore _images;

        public IndexModel(INoticeRepository notices, ImageStore images)
        {
            _notices = notices;
            _images = images;
        }

        public List<Notice> Items { get; private set; }
        public CreateInput Create { get; set; } = new();

        public class CreateInput
        {
            [Required, StringLength(200)] public string Title { get; set; }
            [Required, StringLength(4000)] public string Description { get; set; }
            public IFormFile Image { get; set; }
        }

        public void OnGet() => Load();

        public IActionResult OnGetList()
        {
            Load();
            return Partial("_List", this);
        }

        public IActionResult OnGetCreate() => Partial("_CreateForm", this);

        public async Task<IActionResult> OnPostCreateAsync([Bind(Prefix = nameof(Create))] CreateInput input)
        {
            Create = input ?? new CreateInput();
            if (!ImageStore.IsValid(Create.Image))
                ModelState.AddModelError("Create.Image", "Invalid image. Allowed: jpg, jpeg, png, gif, webp up to 5 MB.");

            var title = Create.Title?.Trim();
            if (!string.IsNullOrEmpty(title) && _notices.FindBy(x => x.Title == title && !x.IsDeleted).Any())
                ModelState.AddModelError("Create.Title", "Title already exists. Please try another title.");

            if (!ModelState.IsValid)
                return Partial("_CreateForm", this);

            // image is stored only after validation passed (no orphan files)
            _notices.Add(new Notice
            {
                Title = title,
                Description = Create.Description,
                LogoName = await _images.SaveAsync(Create.Image, "notices")
            });
            _notices.Commit();

            Toast("Notice Added!");
            CloseModal();
            RefreshList();
            return new EmptyResult();
        }

        private void Load() => Items = _notices.GetAll().OrderByDescending(n => n.CreatedOn).ToList();
    }
}
