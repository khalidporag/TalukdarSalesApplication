using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TalukdarSales.Web.Infrastructure;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Pages.Invoices
{
    public class IndexModel : PageModelBase
    {
        private readonly InvoiceService _invoices;
        private readonly IUserTypeRepository _userTypes;
        private readonly IUserRepository _users;

        public IndexModel(InvoiceService invoices, IUserTypeRepository userTypes, IUserRepository users)
        {
            _invoices = invoices;
            _userTypes = userTypes;
            _users = users;
        }

        [BindProperty(SupportsGet = true)] public int? TypeId { get; set; }
        [BindProperty(SupportsGet = true)] public int? UserId { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? From { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? To { get; set; }
        /// <summary>Set once the user touched the date filter, so an empty range means "all dates" instead of "today".</summary>
        [BindProperty(SupportsGet = true)] public bool Filtered { get; set; }

        public InvoiceList Result { get; private set; }
        public List<SelectListItem> TypeOptions { get; private set; }
        public List<SelectListItem> UserOptions { get; private set; }
        public CollectInput Collect { get; set; } = new();
        public InvoiceRow CollectInvoice { get; private set; }
        public double UserDue { get; private set; }

        public class CollectInput
        {
            public int InvoiceId { get; set; }
            [Required, Range(0.01, double.MaxValue, ErrorMessage = "Enter an amount greater than zero")] public double? Amount { get; set; }
            [Required, StringLength(50)] public string PaymentMethod { get; set; }
        }

        public void OnGet()
        {
            if (!Filtered && From == null && To == null)
                From = To = DateTime.Today;
            LoadOptions();
            Load();
        }

        public IActionResult OnGetList()
        {
            Load();
            return Partial("_List", this);
        }

        public IActionResult OnGetExport()
        {
            Load();
            return Excel.Sheet("InvoiceList.xlsx", "Invoice Data",
                new[] { "Invoice For", "Invoice No", "Requisition No", "Creation Time", "Total", "Collection", "Due" },
                Result.Rows.Select(r => new object[] { r.UserName, r.Number, r.RequisitionNo, r.CreatedDateTime, r.Total, r.Collected, r.Due }),
                new object[] { "Total", null, null, null, Result.Total, Result.Collected, Result.Due });
        }

        public IActionResult OnGetCollect(int id)
        {
            if (!LoadCollect(id))
                return NotFound();
            Collect = new CollectInput { InvoiceId = id, PaymentMethod = "Cash" };
            return Partial("_CollectForm", this);
        }

        public IActionResult OnPostCollect([Bind(Prefix = nameof(Collect))] CollectInput input)
        {
            Collect = input ?? new CollectInput();
            if (!LoadCollect(Collect.InvoiceId))
                return NotFound();
            if (!ModelState.IsValid)
                return Partial("_CollectForm", this);

            var (ok, error) = _invoices.Collect(Collect.InvoiceId, Collect.Amount.Value, Collect.PaymentMethod.Trim());
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, error);
                return Partial("_CollectForm", this);
            }
            Toast("Amount Collected!");
            CloseModal();
            RefreshList();
            return new EmptyResult();
        }

        private bool LoadCollect(int id)
        {
            var detail = _invoices.Get(id);
            if (detail == null)
                return false;
            CollectInvoice = detail.Header;
            UserDue = _invoices.List(detail.Header.UserId, null, null).Rows.Sum(r => r.Due);
            return true;
        }

        private void Load() => Result = _invoices.List(UserId, From, To ?? From);

        private void LoadOptions()
        {
            TypeOptions = _userTypes.GetAll().Select(t => new SelectListItem(t.TypeName, t.Id.ToString())).ToList();
            UserOptions = _users.GetAll().Where(u => TypeId == null || u.UserTypeId == TypeId)
                .OrderBy(u => u.FirstName).Select(u => new SelectListItem($"{u.FirstName} {u.LastName} ({u.Username})", u.Id.ToString())).ToList();
        }
    }
}
