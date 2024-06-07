namespace TalukdarSalesAPI.Models.Dto
{
    public class SalesInvoiceDetailsDto
    {
        public int Id { get; set; }
        public int? SalesInvoiceId { get; set; }
        public int? FinishedGoodsId { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public double? Quantity { get; set; }
        public double? Price { get; set; }
        public double? DiscountAmount { get; set; }
        public double? DiscountPercentage { get; set; }
    }
}
