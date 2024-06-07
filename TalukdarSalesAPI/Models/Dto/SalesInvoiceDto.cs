namespace TalukdarSalesAPI.Models.Dto
{
    public class SalesInvoiceDto
    {
        public int Id { get; set; }
        public int? SalesRequisitionId { get; set; }
        public string SalesRequisitionNo { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public double? Quantity { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public double? TotalPrice { get; set; }
        public double? DiscountAmount { get; set; }
        public double? DiscountPercentage { get; set; }
        public int? CollectionAmount { get; set; }
        public List<SalesInvoiceDetailsDto> SalesInvoiceDetails { get; set; }
    }
}
