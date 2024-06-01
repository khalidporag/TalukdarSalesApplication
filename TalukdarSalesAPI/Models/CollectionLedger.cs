namespace TalukdarSalesAPI.Models
{
    public class CollectionLedger : EntityBase
    {
        public int UserId { get; set; }
        public int SalesInvoiceId { get; set; }
        public double CollectionAmount { get; set; }
        public string PaymentMethod { get; set; }
    }
}
