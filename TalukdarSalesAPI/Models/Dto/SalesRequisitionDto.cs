namespace TalukdarSalesAPI.Models.Dto
{
    public class SalesRequisitionDto
    {
        public int UserId { get; set; }
        public int GoodTypeId { get; set; }
        public List<SalesRequisitionDetailsDto> RequisitionDetails { get; set; }
    }
}
