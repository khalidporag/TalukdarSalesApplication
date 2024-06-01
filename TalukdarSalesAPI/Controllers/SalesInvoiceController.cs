using Microsoft.AspNetCore.Mvc;
using TalukdarSalesAPI.Interfaces;
using TalukdarSalesAPI.Models;
using TalukdarSalesAPI.Repositories;

namespace TalukdarSalesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesInvoiceController : ControllerBase
    {
        private readonly ICollectionLedgerRepository _collectionLedgerRepository;
        private readonly ISalesInvoiceRepository _salesInvoiceRepository;
        private readonly ISalesInvoiceDetailsRepository _salesInvoiceDetailsRepository;
        public SalesInvoiceController(ICollectionLedgerRepository collectionLedgerRepository,
            ISalesInvoiceRepository salesInvoiceRepository,
            ISalesInvoiceDetailsRepository salesInvoiceDetailsRepository)
        {
            _collectionLedgerRepository = collectionLedgerRepository;
            _salesInvoiceRepository = salesInvoiceRepository;
            _salesInvoiceDetailsRepository = salesInvoiceDetailsRepository;

        }
        [HttpPost("createCollectionLedger")]
        public IActionResult CreateCollectionLedger([FromBody] CollectionLedger collectionLedgerObj)
        {
            if (collectionLedgerObj == null)
                return BadRequest();

            _collectionLedgerRepository.Add(collectionLedgerObj);
            _collectionLedgerRepository.Commit();
            return Ok(new
            {
                Status = 200,
                Message = "Collection Ledger Added!"
            });
        }

        [HttpGet("getAllCollectionLedger")]
        public ActionResult<CollectionLedger> GetAllCollectionLedger()
        {
            return Ok(_collectionLedgerRepository.GetAll());
        }

        [HttpPost("createSalesInvoice")]
        public IActionResult CreateSalesInvoice([FromBody] SalesInvoice salesInvoiceObj)
        {
            if (salesInvoiceObj == null)
                return BadRequest();

            _salesInvoiceRepository.Add(salesInvoiceObj);
            _salesInvoiceRepository.Commit();
            return Ok(new
            {
                Status = 200,
                Message = "Sales Invoice Created!"
            });
        }

        [HttpGet("getAllSalesInvoice")]
        public ActionResult<SalesInvoice> GetAllSalesInvoice()
        {
            return Ok(_salesInvoiceRepository.GetAll());
        }

        [HttpPost("createSalesInvoiceDetail")]
        public IActionResult CreateSalesInvoiceDetail([FromBody] SalesInvoiceDetails salesRequisitionDetailsObj)
        {
            if (salesRequisitionDetailsObj == null)
                return BadRequest();

            _salesInvoiceDetailsRepository.Add(salesRequisitionDetailsObj);
            _salesInvoiceDetailsRepository.Commit();
            return Ok(new
            {
                Status = 200,
                Message = "Sales Invoice Details Created!"
            });
        }

        [HttpGet("getAllSalesInvoiceDetails")]
        public ActionResult<SalesInvoiceDetails> GetAllSalesInvoiceDetails()
        {
            return Ok(_salesInvoiceDetailsRepository.GetAll());
        }
    }
}
