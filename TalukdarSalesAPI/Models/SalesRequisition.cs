namespace TalukdarSalesAPI.Models
{
    public class SalesRequisition : EntityBase
    {
        public int UserId { get; set; }
        public string RequisitionSerial { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}
