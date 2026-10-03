namespace TalukdarSales.Web.Models
{
    /// <summary>A return against an invoice. Amount reduces the invoice; Refund is the part already paid that goes back to the customer.</summary>
    public class CreditNote : EntityBase
    {
        public string Serial { get; set; }
        public int SalesInvoiceId { get; set; }
        public int UserId { get; set; }
        public double Amount { get; set; }
        public double Refund { get; set; }
        public string Reason { get; set; }
        public int CreatedByUserId { get; set; }
    }

    public class CreditNoteLine : EntityBase
    {
        public int CreditNoteId { get; set; }
        public int FinishedGoodId { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }
    }
}
