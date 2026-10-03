namespace TalukdarSales.Web.Models.Dto
{
    public class SalesRequisitionDetailsDto
    {
        public int? SalesRequisitionId { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public int? FinishedGoodId { get; set; }
        public string FinishedGoodName { get; set; }
        public Double? Quantity { get; set; }
        public Double? Price { get; set; }
    }
}
