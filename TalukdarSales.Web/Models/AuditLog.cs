namespace TalukdarSales.Web.Models
{
    /// <summary>Who did what and when. CreatedOn is the time of the action.</summary>
    public class AuditLog : EntityBase
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        /// <summary>Short verb phrase such as "invoice.create" or "price.change".</summary>
        public string Action { get; set; }
        public string Entity { get; set; }
        public int? EntityId { get; set; }
        public string Summary { get; set; }
    }
}
