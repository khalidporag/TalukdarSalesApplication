namespace TalukdarSalesAPI.Models.Dto
{
    public class CreateNoticeDto
    {
        public string Title { get; set; }
        public string LogoName { get; set; }
        public string Description { get; set; }
        public IFormFile? Image { get; set; }

    }
}
