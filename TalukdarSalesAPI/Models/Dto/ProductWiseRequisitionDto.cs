namespace TalukdarSalesAPI.Models.Dto
{
    public class ProductWiseRequisitionDto
    {
        public DateTime RequisitionDate { get; set; }
        public int FinishedGoodId { get; set; }
        public string FinishedGoodName { get; set; }
        public double TotalQuantity { get; set; }
    }
}
