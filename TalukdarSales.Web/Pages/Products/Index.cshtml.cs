using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Pages.Products
{
    public class IndexModel : PageModelBase
    {
        private const int PageSize = 12;
        private readonly IFinishedGoodsRepository _goods;
        private readonly IFinishedGoodTypeRepository _types;
        private readonly ImageStore _images;

        public IndexModel(IFinishedGoodsRepository goods, IFinishedGoodTypeRepository types, ImageStore images)
        {
            _goods = goods;
            _types = types;
            _images = images;
        }

        [BindProperty(SupportsGet = true)] public string Name { get; set; }
        [BindProperty(SupportsGet = true)] public int? TypeId { get; set; }
        [BindProperty(SupportsGet = true, Name = "p")] public int PageNo { get; set; } = 1;

        public Paged<FinishedGood> Items { get; private set; }
        public Dictionary<int, string> TypeNames { get; private set; }
        public List<SelectListItem> TypeOptions { get; private set; }

        public CreateInput Create { get; set; } = new();
        public EditInput Edit { get; set; } = new();

        public class CreateInput
        {
            [Required, StringLength(150)] public string Name { get; set; }
            [Required, StringLength(30)] public string UOM { get; set; }
            [Required(ErrorMessage = "Product type is required"), Range(1, int.MaxValue, ErrorMessage = "Product type is required")]
            public int GoodTypeId { get; set; }
            [Required, StringLength(500)] public string Description { get; set; }
            [Range(0, double.MaxValue)] public double UnitPrice { get; set; }
            public IFormFile Image { get; set; }
        }

        public class EditInput
        {
            public int Id { get; set; }
            public string Name { get; set; }
            [Required, StringLength(500)] public string Description { get; set; }
            [Range(0, double.MaxValue)] public double UnitPrice { get; set; }
            public bool IsActive { get; set; }
        }

        [BindProperty(SupportsGet = true)] public int? EditId { get; set; }
        [BindProperty(SupportsGet = true)] public bool New { get; set; }
        public string Mode { get; private set; }

        public IActionResult OnGet()
        {
            LoadOptions(); Load();
            if (EditId != null)
            {
                var g = _goods.GetSingle(EditId.Value);
                if (g == null) return NotFound();
                Edit = new EditInput { Id = g.Id, Name = g.Name, Description = g.Description, UnitPrice = g.UnitPrice, IsActive = g.IsActive };
                Mode = "edit";
            }
            else if (New) Mode = "create";
            return Page();
        }

        public IActionResult OnPostCreateCategory(string name)
        {
            name = (name ?? "").Trim();
            if (name.Length == 0 || name.Length > 100)
                TempData["Flash"] = "Enter a category name.";
            else if (_types.GetAll().Any(t => t.Name.ToLower() == name.ToLower()))
                TempData["Flash"] = "This category already exists.";
            else
            {
                _types.Add(new FinishGoodType { Name = name });
                _types.Commit();
                TempData["Flash"] = "Category added.";
            }
            return RedirectToPage("Index", new { TypeId, Name });
        }

        public IActionResult OnPostToggle(int id)
        {
            var g = _goods.GetSingle(id);
            if (g == null) return NotFound();
            g.IsActive = !g.IsActive;
            _goods.Update(g);
            _goods.Commit();
            TempData["Flash"] = g.IsActive ? $"{g.Name} is available again." : $"{g.Name} is hidden from new orders.";
            return RedirectToPage("Index", new { TypeId, Name, p = PageNo });
        }

        public async Task<IActionResult> OnPostCreateAsync([Bind(Prefix = nameof(Create))] CreateInput input)
        {
            Create = input ?? new CreateInput();
            if (!ImageStore.IsValid(Create.Image))
                ModelState.AddModelError("Create.Image", "Invalid image. Allowed: jpg, jpeg, png, gif, webp up to 5 MB.");
            if (!ModelState.IsValid)
                return Redisplay("create");

            _goods.Add(new FinishedGood
            {
                Name = Create.Name.Trim(),
                UOM = Create.UOM.Trim(),
                GoodTypeId = Create.GoodTypeId,
                Description = Create.Description,
                UnitPrice = Create.UnitPrice,
                LogoName = await _images.SaveAsync(Create.Image, "products"),
                IsActive = true
            });
            _goods.Commit();
            TempData["Flash"] = "Product added.";
            return RedirectToPage("Index");
        }

        public IActionResult OnPostEdit([Bind(Prefix = nameof(Edit))] EditInput input)
        {
            Edit = input ?? new EditInput();
            if (!ModelState.IsValid)
                return Redisplay("edit");

            var g = _goods.GetSingle(Edit.Id);
            if (g == null)
                return NotFound();
            g.Description = Edit.Description;
            g.UnitPrice = Edit.UnitPrice;
            g.IsActive = Edit.IsActive;
            _goods.Update(g);
            _goods.Commit();
            TempData["Flash"] = "Product updated.";
            return RedirectToPage("Index");
        }

        private IActionResult Redisplay(string mode)
        {
            Mode = mode;
            LoadOptions(); Load();
            return Page();
        }

        private void Load()
        {
            var all = _goods.GetAll();
            if (TypeId != null)
                all = all.Where(n => n.GoodTypeId == TypeId);
            if (!string.IsNullOrWhiteSpace(Name))
            {
                var term = Name.Trim().ToLower();
                all = all.Where(n => n.Name.ToLower().Contains(term));
            }
            Items = Paged<FinishedGood>.Create(all.OrderBy(n => n.Name).ThenBy(n => n.Id), PageNo, PageSize);
            TypeNames = _types.GetAll().ToDictionary(t => t.Id, t => t.Name);
        }

        private void LoadOptions()
        {
            TypeOptions = _types.GetAll().OrderBy(t => t.Name).Select(t => new SelectListItem(t.Name, t.Id.ToString())).ToList();
        }
    }
}
