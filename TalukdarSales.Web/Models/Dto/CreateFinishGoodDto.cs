namespace TalukdarSales.Web.Models.Dto
{
    public class CreateFinishGoodDto
    {
        public string Name { get; set; }
        public string UOM { get; set; }
        public double UnitPrice { get; set; }
        public string Description { get; set; }
        public bool? IsActive { get; set; }
        public int GoodTypeId { get; set; }
        public IFormFile? Image { get; set; }

    }
}
