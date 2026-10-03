using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;

namespace TalukdarSales.Web.Pages.ProductTypes
{
    public class IndexModel : PageModelBase
    {
        private readonly IFinishedGoodTypeRepository _types;
        public IndexModel(IFinishedGoodTypeRepository types) => _types = types;

        [BindProperty(SupportsGet = true)] public string Name { get; set; }
        public List<FinishGoodType> Items { get; private set; }
        public CreateInput Create { get; set; } = new();

        public class CreateInput
        {
            [Required, StringLength(100)] public string Name { get; set; }
        }

        public void OnGet() => Load();

        public IActionResult OnGetList()
        {
            Load();
            return Partial("_List", this);
        }

        public IActionResult OnGetCreate() => Partial("_CreateForm", this);

        public IActionResult OnPostCreate([Bind(Prefix = nameof(Create))] CreateInput input)
        {
            Create = input ?? new CreateInput();
            if (!ModelState.IsValid)
                return Partial("_CreateForm", this);

            var name = Create.Name.Trim();
            if (_types.GetAll().Any(t => t.Name.ToLower() == name.ToLower()))
            {
                ModelState.AddModelError("Create.Name", "This product type already exists.");
                return Partial("_CreateForm", this);
            }

            _types.Add(new FinishGoodType { Name = name });
            _types.Commit();
            Toast("Finish Good Type Added!");
            CloseModal();
            RefreshList();
            return new EmptyResult();
        }

        private void Load()
        {
            var all = _types.GetAll();
            if (!string.IsNullOrWhiteSpace(Name))
            {
                var term = Name.Trim().ToLower();
                all = all.Where(n => n.Name.ToLower().Contains(term));
            }
            Items = all.OrderBy(n => n.Name).ThenBy(n => n.Id).ToList();
        }
    }
}
