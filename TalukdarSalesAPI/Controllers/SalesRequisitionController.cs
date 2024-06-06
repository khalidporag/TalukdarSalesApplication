using Microsoft.AspNetCore.Mvc;
using TalukdarSalesAPI.Interfaces;
using TalukdarSalesAPI.Models;
using TalukdarSalesAPI.Models.Dto;

namespace TalukdarSalesAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class SalesRequisitionController : ControllerBase
    {
        private readonly ISalesRequisitionRepository _salesRequisitionRepository;
        private readonly ISalesRequisitionDetailRepository _salesRequisitionDetailRepository;
        private readonly IFinishedGoodsRepository _finishedGoodsRepository;

        public SalesRequisitionController(ISalesRequisitionRepository salesRequisitionRepository,
            ISalesRequisitionDetailRepository salesRequisitionDetailRepository,
            IFinishedGoodsRepository finishedGoodsRepository)
        {
            _salesRequisitionRepository = salesRequisitionRepository;
            _salesRequisitionDetailRepository = salesRequisitionDetailRepository;
            _finishedGoodsRepository = finishedGoodsRepository;
        }

        [HttpPost("createSalesRequisitionWithDetail")]
        public IActionResult CreateSalesRequisitionWithDetail([FromBody] SalesRequisitionDto salesRequisitionWithDetailObj)
        {
            if (salesRequisitionWithDetailObj == null)
                return BadRequest();

            var addRequisition = new SalesRequisition();
            addRequisition.UserId = salesRequisitionWithDetailObj.UserId;
            addRequisition.CreatedDateTime = DateTime.UtcNow;

            _salesRequisitionRepository.Add(addRequisition);
            _salesRequisitionRepository.Commit();

            if (addRequisition?.Id == 0)
                return BadRequest();

            string requistionSerialNo = "REQ - " + addRequisition.Id.ToString("D6");

            addRequisition.RequisitionSerial = requistionSerialNo;
            _salesRequisitionRepository.Update(addRequisition);
            _salesRequisitionRepository.Commit();

            var requistionDetailsList = new List<SalesRequisitionDetail>();

            foreach(var detail in salesRequisitionWithDetailObj?.RequisitionDetails)
            {
                var requistionDetail = new SalesRequisitionDetail();
                requistionDetail.SalesRequisitionId = addRequisition.Id;
                requistionDetail.CreatedDateTime = DateTime.UtcNow;
                requistionDetail.FinishedGoodId = (int)detail?.FinishedGoodId;
                requistionDetail.Quantity = (int)detail?.Quantity;
                requistionDetail.Price = (int)detail?.Price;
                requistionDetail.IsActive = true;

                requistionDetailsList.Add(requistionDetail);
            }

            _salesRequisitionDetailRepository.AddRange(requistionDetailsList);
            _salesRequisitionDetailRepository.Commit();

            return Ok(new
            {
                Status = 200,
                Message = "Sales Requisition Details Added!"
            });
        }

        [HttpPost("createSalesRequisition")]
        public IActionResult CreateSalesRequisition([FromBody] SalesRequisition salesRequisitionObj)
        {
            if (salesRequisitionObj == null)
                return BadRequest();

            _salesRequisitionRepository.Add(salesRequisitionObj);
            _salesRequisitionRepository.Commit();
            return Ok(new
            {
                Status = 200,
                Message = "Sales Requisition Added!"
            });
        }

        [HttpGet("getAllSalesRequisition")]
        public ActionResult<SalesRequisition> GetAllSalesRequisition()
        {
            return Ok(_salesRequisitionRepository.GetAll());
        }

        [HttpPost("createSalesRequisitionDetail")]
        public IActionResult CreateSalesRequisitionDetail([FromBody] SalesRequisitionDetail salesRequisitionDetailObj)
        {
            if (salesRequisitionDetailObj == null)
                return BadRequest();

            _salesRequisitionDetailRepository.Add(salesRequisitionDetailObj);
            _salesRequisitionDetailRepository.Commit();
            return Ok(new
            {
                Status = 200,
                Message = "Sales Requisition Details Added!"
            });
        }

        [HttpGet("getAllSalesRequisitionDetail")]
        public ActionResult<SalesRequisitionDetail> GetAllSalesRequisitionDetail()
        {
            return Ok(_salesRequisitionDetailRepository.GetAll());
        }
    }
}
