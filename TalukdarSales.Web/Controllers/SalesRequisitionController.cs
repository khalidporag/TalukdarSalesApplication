using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;
using TalukdarSales.Web.Models.Dto;
using TalukdarSales.Web.Services;

namespace TalukdarSales.Web.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class SalesRequisitionController : ControllerBase
    {
        private readonly ISalesRequisitionRepository _salesRequisitionRepository;
        private readonly ISalesRequisitionDetailRepository _salesRequisitionDetailRepository;
        private readonly IFinishedGoodsRepository _finishedGoodsRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITimeSettingRepository _timeSettingRepository;
        private readonly IConfiguration _configuration;
        private readonly IFinishedGoodTypeRepository _finishedGoodTypeRepository;
        private readonly RequisitionService _requisitionService;


        public SalesRequisitionController(ISalesRequisitionRepository salesRequisitionRepository,
            ISalesRequisitionDetailRepository salesRequisitionDetailRepository,
            IFinishedGoodsRepository finishedGoodsRepository,
            IUserRepository userRepository,
            ITimeSettingRepository timeSettingRepository,
            IConfiguration configuration,
            IFinishedGoodTypeRepository finishedGoodTypeRepository,
            RequisitionService requisitionService)
        {
            _requisitionService = requisitionService;
            _salesRequisitionRepository = salesRequisitionRepository;
            _salesRequisitionDetailRepository = salesRequisitionDetailRepository;
            _finishedGoodsRepository = finishedGoodsRepository;
            _userRepository = userRepository;
            _configuration = configuration;
            _timeSettingRepository = timeSettingRepository;
            _finishedGoodTypeRepository = finishedGoodTypeRepository;
        }

        [HttpPost("createSalesRequisitionWithDetail")]
        public IActionResult CreateSalesRequisitionWithDetail([FromBody] SalesRequisitionDto salesRequisitionWithDetailObj)
        {
            if (salesRequisitionWithDetailObj?.RequisitionDetails == null || salesRequisitionWithDetailObj.UserId == 0)
                return BadRequest();

            var lines = salesRequisitionWithDetailObj.RequisitionDetails
                .Where(d => d.FinishedGoodId != null && d.Quantity != null)
                .Select(d => new RequisitionLine(d.FinishedGoodId.Value, d.Quantity.Value));

            var (ok, error, _) = _requisitionService.Create(salesRequisitionWithDetailObj.UserId, lines);
            if (!ok)
                return BadRequest(new { Message = error });
            return Ok(new { Status = 200, Message = "Sales Requisition Details Added!" });
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
        public ActionResult<SalesRequisitionInfoDto> GetSalesRequisitionList(bool? isActive, int? userId, string requisitionNo, DateTime? from, DateTime? to)
        {
            var userList = _userRepository.GetAll().ToDictionary(n => n.Id);
            var requisitionList = _salesRequisitionRepository.GetAll().ToList();
            if (isActive != null)
                requisitionList = requisitionList.Where(n => n.IsActive == isActive).ToList();
            if(userId != null)
                requisitionList = requisitionList.Where(n => n.UserId == userId).ToList();
            if(requisitionNo != null)
                requisitionList = requisitionList.Where(n => n.RequisitionSerial.Contains(requisitionNo)).ToList();
            if (from != null && to != null)
            {
                to = to.Value.AddDays(1).AddSeconds(-1);
                requisitionList = requisitionList.Where(n => n.CreatedOn >= from && n.CreatedOn <= to).ToList();
            }
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

        [HttpGet("getProductWiseDailyRequisition")]
        public ActionResult<ProductWiseRequisitionDto> GetProductWiseDailyRequisition()
        {
            var currentDateTime = DateTime.Now.Date;
            var requisitionList = _salesRequisitionDetailRepository.GetAll().Where(n => n.CreatedDateTime.Date == DateTime.Now.Date).ToList();
            var finishedGoodList = _finishedGoodsRepository.GetAll().ToDictionary(n => n.Id);
            var dailyFinishedGoodsQuantities = requisitionList
           .GroupBy(r => new { Date = r.CreatedDateTime.Date, r.FinishedGoodId })
           .Select(g => new ProductWiseRequisitionDto
           {
               RequisitionDate = g.Key.Date,
               FinishedGoodId = g.Key.FinishedGoodId,
               FinishedGoodName = finishedGoodList.ContainsKey(g.Key.FinishedGoodId)? finishedGoodList[g.Key.FinishedGoodId].Name : "",
               TotalQuantity = g.Sum(r => r.Quantity)
           })
           .OrderBy(result => result.RequisitionDate)
           .ThenBy(result => result.FinishedGoodId)
           .ToList();

            return Ok(dailyFinishedGoodsQuantities);
        }

        [HttpGet("getProductTypeWiseDailyRequisition")]
        public ActionResult<ProductTypewiseRequisitionDto> GetProductTypeWiseDailyRequisition()
        {
            var currentDateTime = DateTime.Now.Date;
            var requisitionList = _salesRequisitionDetailRepository.GetAll().Where(n => n.CreatedDateTime.Date == currentDateTime).ToList();
            var finishedGoodList = _finishedGoodsRepository.GetAll().ToDictionary(n => n.Id);
            var productTypeList = _finishedGoodTypeRepository.GetAll().ToDictionary(n => n.Id);

            var dailyProductTypeWiseQuantities = requisitionList
               .GroupBy(r => new
               {
                   Date = r.CreatedDateTime.Date,
                   TypeId = finishedGoodList.ContainsKey(r.FinishedGoodId) ? finishedGoodList[r.FinishedGoodId].GoodTypeId : 0
               })
               .Select(g => new ProductTypewiseRequisitionDto
               {
                   RequisitionDate = g.Key.Date,
                   ProductTypeId = g.Key.TypeId,
                   ProductTypeName = productTypeList.ContainsKey(g.Key.TypeId) ? productTypeList[g.Key.TypeId].Name : "",
                   FinishedGoods = g.GroupBy(fg => fg.FinishedGoodId)
                                    .Select(fg => new FinishedGoodsDto
                                    {
                                        FinishedGoodId = fg.Key,
                                        FinishedGoodName = finishedGoodList.ContainsKey(fg.Key) ? finishedGoodList[fg.Key].Name : "",
                                        TotalQuantity = fg.Sum(f => f.Quantity),
                                        ProductTypeId = g.Key.TypeId,
                                        ProductTypeName = productTypeList.ContainsKey(g.Key.TypeId) ? productTypeList[g.Key.TypeId].Name : ""
                                    }).ToList()
               })
               .OrderBy(result => result.RequisitionDate)
               .ThenBy(result => result.ProductTypeId)
               .ToList();

            return Ok(dailyProductTypeWiseQuantities);
        }


        [HttpPost("updateTimeSetting")]
        public IActionResult UpdateRequisitionTimeSetting([FromBody] TimeSetting timeSettingObj)
        {
            if (timeSettingObj == null)
                return BadRequest();
            if (timeSettingObj != null && (timeSettingObj.From == null && timeSettingObj.To == null ))
                return BadRequest();
            var timeSetting = _timeSettingRepository.GetAll().OrderByDescending(n => n.CreatedOn).FirstOrDefault();
            if (timeSetting == null)
            {
                var createTimeSetting = new TimeSetting();
                createTimeSetting.From = timeSettingObj.From;
                createTimeSetting.To = timeSettingObj.To;
                _timeSettingRepository.Add(createTimeSetting);
                _timeSettingRepository.Commit();
            }
            else if (timeSettingObj?.From != null && timeSettingObj?.To != null)
            {
                timeSetting.From = timeSettingObj?.From;
                timeSetting.To = timeSettingObj?.To;
                _timeSettingRepository.Update(timeSetting);
                _timeSettingRepository.Commit();
            }
            return Ok(new
            {
                Status = 200,
                Message = "Time Setting Updated!"
            });
        }

        [HttpGet("getTimeSetting")]
        public ActionResult<TimeSetting> GetTimeSetting()
        {
            var result = _timeSettingRepository.GetAll().OrderByDescending(n => n.CreatedOn).FirstOrDefault();
            return Ok(result);
        }

        [HttpGet("getDailyAccumulatedOrderSummary")]
        public ActionResult<ProductWiseRequisitionDto> GetDailyAccumulatedOrderSummary(DateTime? from, DateTime? to)
        {
            var oneMonthAgo = DateTime.Now.AddMonths(-1);
            var requisitionList = _salesRequisitionDetailRepository.GetAll().ToList();

            if (from != null && to != null)
                requisitionList = requisitionList.Where(n => n.CreatedOn >= from && n.CreatedOn < to.Value.Date.AddDays(1)).ToList();
            else
                requisitionList = requisitionList.Where(s => s.CreatedOn >= oneMonthAgo).ToList();

            var finishedGoodList = _finishedGoodsRepository.GetAll().ToDictionary(n => n.Id);
            var dailyFinishedGoodsQuantities = requisitionList
           .GroupBy(r => new { Date = r.CreatedDateTime.Date, r.FinishedGoodId })
           .Select(g => new ProductWiseRequisitionDto
           {
               RequisitionDate = g.Key.Date,
               FinishedGoodId = g.Key.FinishedGoodId,
               FinishedGoodName = finishedGoodList.ContainsKey(g.Key.FinishedGoodId) ? finishedGoodList[g.Key.FinishedGoodId].Name : "",
               TotalQuantity = g.Sum(r => r.Quantity)
           })
           .OrderBy(result => result.RequisitionDate)
           .ThenBy(result => result.FinishedGoodId)
           .ToList();

            return Ok(dailyFinishedGoodsQuantities);
        }
    }
}
