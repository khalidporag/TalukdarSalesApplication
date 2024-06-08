namespace TalukdarSalesAPI.Models.Dto
{
    public class CollectionLedgerDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? SalesInvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public double? CollectionAmount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime? CollectionTime { get; set; }
    }
}
