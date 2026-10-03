namespace TalukdarSales.Web.Models
{
    public class SalesRequisition : EntityBase
    {
        public int UserId { get; set; }
        public string RequisitionSerial { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public bool IsActive { get; set; }
        /// <summary>The staff member who took the order (0 for orders made before this was recorded).</summary>
        public int CreatedByUserId { get; set; }
        public bool IsCancelled { get; set; }
        public string CancelReason { get; set; }
    }
}
