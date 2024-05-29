namespace TalukdarSalesAPI.Models
{
    public class SalesRequisitionDetail : EntityBase
    {
        public int SalesRequisitionId { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public int FinishedGoodId { get; set; }
        public Double Quantity { get; set; }
        public Double Price { get; set; }
        public bool IsActive { get; set; }
    }
}
