namespace TalukdarSales.Web.Models
{
    /// <summary>One in-app notification for one recipient.</summary>
    public class Notification : EntityBase
    {
        public int UserId { get; set; }
        public string Kind { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Link { get; set; }
        public bool IsRead { get; set; }
    }
}
