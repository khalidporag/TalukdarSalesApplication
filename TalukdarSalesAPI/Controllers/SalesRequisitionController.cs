using Microsoft.AspNetCore.Mvc;
using TalukdarSalesAPI.Interfaces;
using TalukdarSalesAPI.Models;

namespace TalukdarSalesAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class SalesRequisitionController : ControllerBase
    {
        private readonly ISalesRequisitionRepository _salesRequisitionRepository;
        private readonly ISalesRequisitionDetailRepository _salesRequisitionDetailRepository;

        public SalesRequisitionController(ISalesRequisitionRepository salesRequisitionRepository,
            ISalesRequisitionDetailRepository salesRequisitionDetailRepository)
        {
            _salesRequisitionRepository = salesRequisitionRepository;
            _salesRequisitionDetailRepository = salesRequisitionDetailRepository;
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
