namespace TalukdarSalesAPI.Models.Dto
{
    public class CollectAmountDto
    {
        public int SalesInvoiceId { get; set; }
        public double? CollectionAmount { get; set; }
        public string PaymentMethod { get; set; }
    }
}
