using Microsoft.AspNetCore.Mvc;
using TalukdarSalesAPI.Interfaces;
using TalukdarSalesAPI.Models;
using TalukdarSalesAPI.Models.Dto;

namespace TalukdarSalesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NoticeController : ControllerBase
    {
        private readonly INoticeRepository _noticeRepository;
        public NoticeController(INoticeRepository noticeRepository)
        {
            _noticeRepository = noticeRepository;
        }

        [HttpPost("createNotice")]
        public async Task<IActionResult> CreateNoticeAsync([FromForm] CreateNoticeDto input)
        {
            if (input == null)
                return BadRequest();

            string uniqueFileName = "";
            if (input.Image != null)
            {
                uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(input.Image.FileName);
                var directoryPath = "wwwroot/images/notices";
                var filePath = Path.Combine(directoryPath, uniqueFileName);

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await input.Image.CopyToAsync(stream);
                }
            }

            var notice = _noticeRepository.FindBy(x => x.Title == input.Title).FirstOrDefault();

            if (notice != null)
                return NotFound(new { Message = "Title already Exist. Please try with another title!"});

            var createNotice = new Notice();
            createNotice.Title = input.Title;
            createNotice.Description = input.Description;
            createNotice.LogoName = uniqueFileName;

            _noticeRepository.Add(createNotice);
            _noticeRepository.Commit();

            return Ok(new
            {
                Status = 200,
                Message = "Notice Added!"
            });
        }

        [HttpGet("getAllNotices")]
        public ActionResult<Notice> GetAllNotices(bool? isLanding)
        {
            var result = (isLanding != null && isLanding == true) ? _noticeRepository.GetAll().OrderByDescending(n => n.CreatedOn).Take(6) : _noticeRepository.GetAll().OrderByDescending(n => n.CreatedOn);
            return Ok(result);
        }
    }
}
