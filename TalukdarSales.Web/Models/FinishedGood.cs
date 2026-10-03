namespace TalukdarSales.Web.Models
{
    public class FinishedGood : EntityBase
    {
        public string Name { get; set; }
        public string LogoName { get; set; }
        public string UOM { get; set; }
        public double UnitPrice { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int GoodTypeId { get; set; }
    }
}
