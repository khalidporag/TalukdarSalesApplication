namespace TalukdarSales.Web.Models
{
    public class SalesRequisition : EntityBase
    {
        public int UserId { get; set; }
        public string RequisitionSerial { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public bool IsActive { get; set; }
    }
}
