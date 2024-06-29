using Microsoft.AspNetCore.Mvc;
using TalukdarSalesAPI.Interfaces;
using TalukdarSalesAPI.Models;

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
        public IActionResult CreateRole([FromBody] Notice input)
        {
            if (input == null)
                return BadRequest();

            var notice = _noticeRepository.FindBy(x => x.Title == input.Title).FirstOrDefault();

            if (notice != null)
                return NotFound(new { Message = "Title already Exist. Please try with another title!"});

            _noticeRepository.Add(input);
            _noticeRepository.Commit();

            return Ok(new
            {
                Status = 200,
                Message = "Notice Added!"
            });
        }

        [HttpGet("getAllNotices")]
        public ActionResult<Notice> GetAllNotices()
        {
            return Ok(_noticeRepository.GetAll());
        }
    }
}
