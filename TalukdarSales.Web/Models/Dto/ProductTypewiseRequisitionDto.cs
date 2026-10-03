namespace TalukdarSales.Web.Models.Dto
{
    public class ProductTypewiseRequisitionDto
    {
        public DateTime RequisitionDate { get; set; }
        public int ProductTypeId { get; set; }
        public string ProductTypeName { get; set; }
        public List<FinishedGoodsDto> FinishedGoods { get; set; }
    }
    public class FinishedGoodsDto
    {
        public int FinishedGoodId { get; set; }
        public string FinishedGoodName { get; set; }
        public double TotalQuantity { get; set; }
        public int ProductTypeId { get; set; }
        public string ProductTypeName { get; set; }
    }
}
