namespace TalukdarSalesAPI.Models
{
    public class SalesInvoice : EntityBase
    {
        public int SalesRequisitionId { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public double Quantity { get; set; }
        public int UserId { get; set; }
        public double TotalPrice { get; set; }
        public double DiscountAmount { get; set; }
        public double DiscountPercentage { get; set; }
        public int CollectionAmount { get; set; }
    }
}
