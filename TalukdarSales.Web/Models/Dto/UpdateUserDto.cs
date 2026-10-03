namespace TalukdarSales.Web.Models.Dto
{
    public class UpdateUserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public double MaxCreditLimit { get; set; }
    }
}
