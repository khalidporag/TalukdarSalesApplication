namespace TalukdarSalesAPI.Models
{
    public class TimeSetting : EntityBase
    {
        public string From { get; set; }    /// <summary>
        ///  Requisition is Allowed within this time  
        /// </summary>
        public string To { get; set; }
    }
}
