using Microsoft.AspNetCore.Mvc;
using Project.Run.Repositories;
using TalukdarSales.Web.Helpers;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;
using TalukdarSales.Web.Models.Dto;

namespace TalukdarSales.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize]
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
        public ActionResult<FinishGoodType> GetAllFinishGoodTypes(string name)
        {
            var result = _finishedGoodTypeRepository.GetAll().ToList();
            if (name != null)
                result = _finishedGoodTypeRepository.GetAll().Where(n => n.Name.ToLower().Contains(name.ToLower())).ToList();
            return Ok(result);
        }

        [HttpPost("createFinishedGood")]
        public async Task<IActionResult> CreateFinishGoodAsync([FromForm] CreateFinishGoodDto input)
        {
            if (input == null)
                return BadRequest();
            if (input.Image != null && !UploadValidator.IsValidImage(input.Image))
                return BadRequest(new { Message = "Invalid image. Allowed: jpg, jpeg, png, gif, webp up to 5 MB." });

            string uniqueFileName = "";
            if (input.Image != null)
            {
                uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(input.Image.FileName);
                var directoryPath = "wwwroot/images/products";
                var filePath = Path.Combine(directoryPath, uniqueFileName);

                // Ensure the directory exists
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Save the file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await input.Image.CopyToAsync(stream);
                }
            }

            var finishedGoodObj = new FinishedGood();
            finishedGoodObj.Name = input.Name;
            finishedGoodObj.LogoName = uniqueFileName;
            finishedGoodObj.IsActive = true;
            finishedGoodObj.UOM = input.UOM;
            finishedGoodObj.UnitPrice = input.UnitPrice;
            finishedGoodObj.Description = input.Description;
            finishedGoodObj.GoodTypeId = input.GoodTypeId;

            _finishedGoodsRepository.Add(finishedGoodObj);
            _finishedGoodsRepository.Commit();
            return Ok(new
            {
                Status = 200,
                Message = "Finish Good Added!"
            });
        }

        [HttpPost("updateFinishedGood")]
        public IActionResult UpdateFinishGood([FromBody] FinishedGoodDto finishedGoodObj)
        {
            if (finishedGoodObj == null)
                return BadRequest();
            var finishedGoodInfo = _finishedGoodsRepository.GetSingle(finishedGoodObj.Id);
            finishedGoodInfo.IsActive = finishedGoodObj.IsActive;
            finishedGoodInfo.UnitPrice = finishedGoodObj.UnitPrice;
            finishedGoodInfo.Description = finishedGoodObj.Description;

            _finishedGoodsRepository.Update(finishedGoodInfo);
            _finishedGoodsRepository.Commit();
            return Ok(new
            {
                Status = 200,
                Message = "Finish Good Type Added!"
            });
        }

        [HttpGet("getAllFinishedGoods")]
        public ActionResult<FinishedGood> GetAllFinishedGoods(int? goodTypeId, string finishedGoodName)
        {
            var result = _finishedGoodsRepository.GetAll().ToList();
            if (goodTypeId != null)
                result = result.Where(n => n.GoodTypeId == goodTypeId).ToList();
            if (finishedGoodName != null)
                result = result.Where(n => n.Name.ToLower().Trim().Contains(finishedGoodName.ToLower().Trim())).ToList();
            return Ok(result);
        }
    }
}
