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
        private readonly IUserRepository _userRepository;

        public SalesRequisitionController(ISalesRequisitionRepository salesRequisitionRepository,
            ISalesRequisitionDetailRepository salesRequisitionDetailRepository,
            IFinishedGoodsRepository finishedGoodsRepository,
            IUserRepository userRepository)
        {
            _salesRequisitionRepository = salesRequisitionRepository;
            _salesRequisitionDetailRepository = salesRequisitionDetailRepository;
            _finishedGoodsRepository = finishedGoodsRepository;
            _userRepository = userRepository;
        }

        [HttpPost("createSalesRequisitionWithDetail")]
        public IActionResult CreateSalesRequisitionWithDetail([FromBody] SalesRequisitionDto salesRequisitionWithDetailObj)
        {
            if (salesRequisitionWithDetailObj == null)
                return BadRequest();
            if (salesRequisitionWithDetailObj?.UserId == 0)
                return BadRequest();

            var addRequisition = new SalesRequisition();
            addRequisition.UserId = salesRequisitionWithDetailObj.UserId;
            addRequisition.CreatedDateTime = DateTime.Now;
            addRequisition.IsActive = true;

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
                requistionDetail.CreatedDateTime = DateTime.Now;
                requistionDetail.FinishedGoodId = (int)detail?.FinishedGoodId;
                requistionDetail.Quantity = (int)detail?.Quantity;
                requistionDetail.Price = (int)detail?.Price;

                if(requistionDetail.Quantity > 0 && requistionDetail.FinishedGoodId != 0)
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

        [HttpGet("getSalesRequisitionList")]
        public ActionResult<SalesRequisitionInfoDto> GetSalesRequisitionList(bool? isActive, int? userId, string requisitionNo)
        {
            var userList = _userRepository.GetAll().ToDictionary(n => n.Id);
            var requisitionList = _salesRequisitionRepository.GetAll();
            if (isActive != null)
                requisitionList = requisitionList.Where(n => n.IsActive == isActive);
            if(userId != null)
                requisitionList = requisitionList.Where(n => n.UserId == userId);
            if(requisitionNo != null)
                requisitionList = requisitionList.Where(n => n.RequisitionSerial.Contains(requisitionNo));
           var result =  requisitionList.AsEnumerable().Select(s => new SalesRequisitionInfoDto
            {
                Id = s.Id,
                RequisitionSerial = s.RequisitionSerial,
                UserId = s.UserId,
                UserName = userList.ContainsKey(s.UserId) ? userList[s.UserId].FirstName + " " + userList[s.UserId].LastName : "",
                CreatedDateTime = s.CreatedDateTime,
                IsActive = s.IsActive
            }).ToList();

            return Ok(result);
        }

        [HttpGet("getSalesRequisitionDetailsList")]
        public ActionResult<SalesRequisitionDetailsDto> GetSalesRequisitionDetailsList(int? requisitionId)
        {
            if (requisitionId == null)
                return BadRequest();
            var finishedGoodList = _finishedGoodsRepository.GetAll().AsEnumerable().ToDictionary(n => n.Id);
            var requisitionDetailsList = _salesRequisitionDetailRepository.GetAll().Where(n => n.SalesRequisitionId == requisitionId).Select(s =>
            new SalesRequisitionDetailsDto
            {
                SalesRequisitionId = s.SalesRequisitionId,
                CreatedDateTime = s.CreatedDateTime,
                FinishedGoodId = s.FinishedGoodId,
                FinishedGoodName = finishedGoodList.ContainsKey((int)s.FinishedGoodId)? finishedGoodList[s.FinishedGoodId].Name : "",
                Quantity = s.Quantity,
                Price = s.Price
            }).ToList();
            return Ok(requisitionDetailsList);
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
