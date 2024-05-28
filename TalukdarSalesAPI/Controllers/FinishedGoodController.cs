using Microsoft.AspNetCore.Mvc;
using TalukdarSalesAPI.Interfaces;
using TalukdarSalesAPI.Models;

namespace TalukdarSalesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinishedGoodController : ControllerBase
    {
        private readonly IFinishedGoodsRepository _finishedGoodsRepository;
        private readonly IFinishedGoodTypeRepository _finishedGoodTypeRepository;

        public FinishedGoodController(IFinishedGoodsRepository finishedGoodsRepository,
            IFinishedGoodTypeRepository finishedGoodTypeRepository)
        {
            _finishedGoodsRepository = finishedGoodsRepository;
            _finishedGoodTypeRepository = finishedGoodTypeRepository;
        }

        [HttpPost("finishGoodType")]
        public IActionResult FinishGoodType([FromBody] FinishGoodType finishGoodTypeObj)
        {
            if (finishGoodTypeObj == null)
                return BadRequest();

            _finishedGoodTypeRepository.Add(finishGoodTypeObj);
            _finishedGoodTypeRepository.Commit();
            return Ok(new
            {
                Status = 200,
                Message = "Finish Good Type Added!"
            });
        }

        [HttpGet("getAllFinishGoodTypes")]
        public ActionResult<FinishGoodType> GetAllFinishGoodTypes()
        {
            return Ok(_finishedGoodTypeRepository.GetAll());
        }

        [HttpPost("createFinishedGood")]
        public IActionResult CreateFinishGood([FromBody] FinishedGood finishedGoodObj)
        {
            if (finishedGoodObj == null)
                return BadRequest();

            _finishedGoodsRepository.Add(finishedGoodObj);
            _finishedGoodsRepository.Commit();
            return Ok(new
            {
                Status = 200,
                Message = "Finish Good Type Added!"
            });
        }

        [HttpGet("getAllFinishedGoods")]
        public ActionResult<FinishedGood> GetAllFinishedGoods()
        {
            return Ok(_finishedGoodsRepository.GetAll());
        }
    }
}
