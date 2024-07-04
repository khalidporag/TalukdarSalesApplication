namespace TalukdarSalesAPI.Models.Dto
{
    public class SalesRequisitionInfoDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string RequisitionSerial { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public bool IsActive { get; set; }
        public bool Selected { get; set; }
    }
}
