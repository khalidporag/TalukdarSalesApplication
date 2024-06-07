using Microsoft.AspNetCore.Mvc;
using TalukdarSalesAPI.Interfaces;
using TalukdarSalesAPI.Models;
using TalukdarSalesAPI.Models.Dto;
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
        private readonly ISalesRequisitionRepository _salesRequisitioinRepository;
        public SalesInvoiceController(ICollectionLedgerRepository collectionLedgerRepository,
            ISalesInvoiceRepository salesInvoiceRepository,
            ISalesInvoiceDetailsRepository salesInvoiceDetailsRepository,
            ISalesRequisitionRepository salesRequisitioinRepository)
        {
            _collectionLedgerRepository = collectionLedgerRepository;
            _salesInvoiceRepository = salesInvoiceRepository;
            _salesInvoiceDetailsRepository = salesInvoiceDetailsRepository;
            _salesRequisitioinRepository = salesRequisitioinRepository;

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

        [HttpPost("createSalesInvoiceWithDetails")]
        public IActionResult CreateSalesInvoiceWithDetails([FromBody] SalesInvoiceDto salesInvoiceObj)
        {
            if (salesInvoiceObj == null)
                return BadRequest();

            var createSalesInvoice = new SalesInvoice();
            var invoiceDetailsList = new List<SalesInvoiceDetails>();
            var totalQuantity = 0.0;
            var totalPrice = 0.0;
            foreach(var details in salesInvoiceObj.SalesInvoiceDetails)
            {
                totalQuantity += (double)details.Quantity;
                totalPrice += (double)details.Price;
            }

            //var ttotalQuantity = salesInvoiceObj.SalesInvoiceDetails.Sum(s => s.Quantity);
            //var ttotalPrice = salesInvoiceObj.SalesInvoiceDetails.Sum(s => s.Price);

            createSalesInvoice.TotalPrice = totalPrice;
            createSalesInvoice.Quantity = totalQuantity;
            createSalesInvoice.UserId = (int)salesInvoiceObj?.UserId;
            createSalesInvoice.SalesRequisitionId = (int)salesInvoiceObj?.SalesRequisitionId;
            createSalesInvoice.DiscountAmount = 0;
            createSalesInvoice.DiscountPercentage = 0;
            createSalesInvoice.CollectionAmount = 0;
            createSalesInvoice.CreatedDateTime = DateTime.UtcNow;

            _salesInvoiceRepository.Add(createSalesInvoice);
            _salesInvoiceRepository.Commit();

            if(createSalesInvoice?.Id != null)
            {
                foreach (var details in salesInvoiceObj.SalesInvoiceDetails)
                {
                    var invoiceDetails = new SalesInvoiceDetails();
                    invoiceDetails.SalesInvoiceId = createSalesInvoice.Id;
                    invoiceDetails.FinishedGoodsId = (int)details?.FinishedGoodsId;
                    invoiceDetails.CreatedDateTime = DateTime.UtcNow;
                    invoiceDetails.Quantity = (double)details?.Quantity;
                    invoiceDetails.Price = (double)details?.Price;
                    invoiceDetails.DiscountAmount = 0;
                    invoiceDetails.DiscountPercentage = 0;

                    invoiceDetailsList.Add(invoiceDetails);
                }

                _salesInvoiceDetailsRepository.AddRange(invoiceDetailsList);
                _salesInvoiceDetailsRepository.Commit();

                var requisition = _salesRequisitioinRepository.GetSingle((int)createSalesInvoice?.SalesRequisitionId);
                requisition.IsActive = false;
                _salesRequisitioinRepository.Update(requisition);
                _salesRequisitioinRepository.Commit();
            }
            

            return Ok(new
            {
                Status = 200,
                Message = "Sales Invoice Created!"
            });
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
