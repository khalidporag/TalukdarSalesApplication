using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project.Run.Repositories;
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
        private readonly IUserRepository _userRepository;
        private readonly ISalesRequisitionDetailRepository _salesRequisitionDetailRepository;
        private readonly IFinishedGoodsRepository _finishGoodRepository;

        public SalesInvoiceController(ICollectionLedgerRepository collectionLedgerRepository,
            ISalesInvoiceRepository salesInvoiceRepository,
            ISalesInvoiceDetailsRepository salesInvoiceDetailsRepository,
            ISalesRequisitionRepository salesRequisitioinRepository,
            IUserRepository userRepository,
            ISalesRequisitionDetailRepository salesRequisitionDetailRepository,
            IFinishedGoodsRepository finishGoodRepository)
        {
            _collectionLedgerRepository = collectionLedgerRepository;
            _salesInvoiceRepository = salesInvoiceRepository;
            _salesInvoiceDetailsRepository = salesInvoiceDetailsRepository;
            _salesRequisitioinRepository = salesRequisitioinRepository;
            _userRepository = userRepository;
            _salesRequisitionDetailRepository = salesRequisitionDetailRepository;
            _finishGoodRepository = finishGoodRepository;
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

        [HttpGet("getCollectionHistory")]
        public ActionResult<CollectionLedgerDto> GetCollectionHistory(int? userId, int? salesInvoiceId)
        {
            var invoiceList = _salesInvoiceRepository.GetAll().ToDictionary(n => n.Id);
            var allCollection = _collectionLedgerRepository.GetAll();
            if(userId != null)
            {
                allCollection = allCollection.Where(n => n.UserId == userId);
            }
            if (salesInvoiceId != null)
            {
                allCollection = allCollection.Where(n => n.SalesInvoiceId == salesInvoiceId);
            }
            var collectionHistory = allCollection.AsEnumerable().Select(s => new CollectionLedgerDto
            {
                Id = s.Id,
                SalesInvoiceId = s.SalesInvoiceId,
                InvoiceNumber = invoiceList.ContainsKey(s.SalesInvoiceId) ? invoiceList[s.SalesInvoiceId].InvoiceSerialNo : "",
                CollectionAmount = s.CollectionAmount,
                PaymentMethod = s.PaymentMethod,
                CollectionTime = s.CreatedOn,
                UserId = s.UserId
            }).ToList();
            return Ok(collectionHistory);
        }

        [HttpPost("createBulkInvoiceWithDetails")]
        public IActionResult CreateBulkInvoiceWithDetails(RequisitionDto obj)
        {
            List<int> requisitionId = obj.RequistionIds
            .Split(',')
            .Select(int.Parse)
            .ToList();

            var requisitionList = _salesRequisitioinRepository.GetAll().Where(n => n.IsActive == true).ToList();
            requisitionList = requisitionList.Where(n => requisitionId.Contains(n.Id)).ToList();
            if (requisitionList.Count == 0)
                return BadRequest();
            foreach (var requisition in requisitionList)
            {
                var requisitionDetails = _salesRequisitionDetailRepository.GetAll().ToList();
                requisitionDetails = requisitionDetails.Where(n => n.SalesRequisitionId == requisition.Id).ToList();
                var totalQuantity = 0.0;
                var totalPrice = 0.0;
                foreach (var details in requisitionDetails)
                {
                    totalQuantity += (double)details.Quantity;
                    totalPrice += (double)details.Price * (double)details.Quantity;
                }

                var createSalesInvoice = new SalesInvoice();
                var invoiceDetailsList = new List<SalesInvoiceDetails>();

                createSalesInvoice.TotalPrice = totalPrice;
                createSalesInvoice.Quantity = totalQuantity;
                createSalesInvoice.UserId = requisition.UserId;
                createSalesInvoice.SalesRequisitionId = requisition.Id;
                createSalesInvoice.DiscountAmount = 0;
                createSalesInvoice.DiscountPercentage = 0;
                createSalesInvoice.CollectionAmount = 0;
                createSalesInvoice.CreatedDateTime = DateTime.Now;

                _salesInvoiceRepository.Add(createSalesInvoice);
                _salesInvoiceRepository.Commit();

                if (createSalesInvoice?.Id != null)
                {
                    var userInfo = _userRepository.GetSingle(createSalesInvoice.UserId);
                    userInfo.DueAmount = (decimal)createSalesInvoice?.TotalPrice;
                    _userRepository.Update(userInfo);
                    _userRepository.Commit();

                    string invoiceSerialNo = "INV - " + createSalesInvoice.Id.ToString("D6");

                    createSalesInvoice.InvoiceSerialNo = invoiceSerialNo;
                    _salesInvoiceRepository.Update(createSalesInvoice);
                    _salesInvoiceRepository.Commit();

                    foreach (var details in requisitionDetails)
                    {
                        var invoiceDetails = new SalesInvoiceDetails();
                        invoiceDetails.SalesInvoiceId = createSalesInvoice.Id;
                        invoiceDetails.FinishedGoodsId = details.FinishedGoodId;
                        invoiceDetails.CreatedDateTime = DateTime.Now;
                        invoiceDetails.Quantity = (double)details?.Quantity;
                        invoiceDetails.Price = (double)details?.Price;
                        invoiceDetails.DiscountAmount = 0;
                        invoiceDetails.DiscountPercentage = 0;

                        invoiceDetailsList.Add(invoiceDetails);
                    }

                    _salesInvoiceDetailsRepository.AddRange(invoiceDetailsList);
                    _salesInvoiceDetailsRepository.Commit();

                    var requisitionInfo = _salesRequisitioinRepository.GetSingle((int)createSalesInvoice?.SalesRequisitionId);
                    requisitionInfo.IsActive = false;
                    _salesRequisitioinRepository.Update(requisitionInfo);
                    _salesRequisitioinRepository.Commit();
                }


            }
            return Ok(new
            {
                Status = 200,
                Message = "Sales Invoice Created!"
            });
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
                totalPrice += (double)details.Price * (double)details.Quantity;
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
            createSalesInvoice.CreatedDateTime = DateTime.Now;

            _salesInvoiceRepository.Add(createSalesInvoice);
            _salesInvoiceRepository.Commit();

            if(createSalesInvoice?.Id != null)
            {
                var userInfo = _userRepository.GetSingle(createSalesInvoice.UserId);
                userInfo.DueAmount = (decimal)createSalesInvoice?.TotalPrice;
                _userRepository.Update(userInfo);
                _userRepository.Commit();

                string invoiceSerialNo = "INV - " + createSalesInvoice.Id.ToString("D6");

                createSalesInvoice.InvoiceSerialNo = invoiceSerialNo;
                _salesInvoiceRepository.Update(createSalesInvoice);
                _salesInvoiceRepository.Commit();

                foreach (var details in salesInvoiceObj.SalesInvoiceDetails)
                {
                    var invoiceDetails = new SalesInvoiceDetails();
                    invoiceDetails.SalesInvoiceId = createSalesInvoice.Id;
                    invoiceDetails.FinishedGoodsId = (int)details?.FinishedGoodsId;
                    invoiceDetails.CreatedDateTime = DateTime.Now;
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

        [HttpPost("collectInvoiceAmount")]
        public IActionResult ColllectInvoiceAmount([FromBody] CollectAmountDto collectAmountObj)
        {
            if (collectAmountObj == null)
                return BadRequest();
            var invoiceInfo = _salesInvoiceRepository.GetSingle((int)collectAmountObj?.SalesInvoiceId);
            if (invoiceInfo.CollectionAmount + (double)collectAmountObj.CollectionAmount > invoiceInfo.TotalPrice)
                return BadRequest();
            invoiceInfo.CollectionAmount = invoiceInfo.CollectionAmount + (double)collectAmountObj.CollectionAmount;
            _salesInvoiceRepository.Update(invoiceInfo);
            _salesInvoiceRepository.Commit();

            var collectionLedger = new CollectionLedger();
            collectionLedger.UserId = (int)invoiceInfo?.UserId;
            collectionLedger.SalesInvoiceId = (int)collectAmountObj?.SalesInvoiceId;
            collectionLedger.CollectionAmount = (double)collectAmountObj?.CollectionAmount;
            collectionLedger.PaymentMethod = collectAmountObj?.PaymentMethod;
            _collectionLedgerRepository.Add(collectionLedger);
            _collectionLedgerRepository.Commit();

            if(collectionLedger?.Id != null)
            {
                var userInfo = _userRepository.GetSingle(collectionLedger.UserId);
                if (userInfo.DueAmount - (decimal)collectionLedger?.CollectionAmount < 0)
                    return BadRequest();
                userInfo.DueAmount = userInfo.DueAmount - (decimal)collectionLedger?.CollectionAmount;
                _userRepository.Update(userInfo);
                _userRepository.Commit();
            }
            return Ok(new
            {
                Status = 200,
                Message = "Amount Collected!"
            });
        }

        [HttpGet("getSalesInvoiceList")]
        public ActionResult<SalesInvoice> GetSalesInvoiceList(int? userId)
        {
            var userList = _userRepository.GetAll().ToDictionary(n => n.Id);
            var requisitionList = _salesRequisitioinRepository.GetAll().ToDictionary(n => n.Id);
            var invoiceList = _salesInvoiceRepository.GetAll();
            if (userId != null)
                invoiceList = invoiceList.Where(n => n.UserId == userId);

            var result = invoiceList.AsEnumerable().Select(s => new SalesInvoiceDto
            {
                Id = s.Id,
                InvoiceNumber = s.InvoiceSerialNo,
                SalesRequisitionId = s.SalesRequisitionId,
                SalesRequisitionNo = requisitionList.ContainsKey(s.SalesRequisitionId)? requisitionList[s.SalesRequisitionId].RequisitionSerial : " ",
                UserId = s.UserId,
                UserName = userList.ContainsKey(s.UserId) ? userList[s.UserId].FirstName + " " + userList[s.UserId].LastName : "",
                TotalPrice = s.TotalPrice,
                CollectionAmount = s.CollectionAmount,
                CreatedDateTime = s.CreatedDateTime,
            }).ToList();

            return Ok(result);
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

        [HttpGet("getTopFiveSeller")]
        public ActionResult<TopSellerDto> GetTopFiveSeller(DateTime? from, DateTime? to)
        {
            var userList = _userRepository.GetAll().ToDictionary(n => n.Id);
            var result = _salesInvoiceRepository.GetAll();
            var oneMonthAgo = DateTime.Now.AddMonths(-1);

            if (from != null && to != null)
                result = result.Where(n => n.CreatedOn >= from && n.CreatedOn <= to);
            else
                result = result.Where(s => s.CreatedOn >= oneMonthAgo);

            var topSellers = result
            .GroupBy(s => s.UserId)
            .Select(g => new TopSellerDto
            {
                UserId = g.Key,
                UserName = userList.ContainsKey(g.Key)? userList[g.Key].FirstName + " " + userList[g.Key].LastName : "",
                TotalAmount = g.Sum(s => s.TotalPrice)
            })
            .OrderByDescending(g => g.TotalAmount)
            .Take(5)
            .ToList();

            return Ok(topSellers);
        }

        [HttpGet("getTopFiveSellingProduct")]
        public ActionResult<SellingProductDto> GetTopFiveSellingProduct(DateTime? from, DateTime? to)
        {
            var productList = _finishGoodRepository.GetAll().ToDictionary(n => n.Id);
            var result = _salesInvoiceDetailsRepository.GetAll();
            var oneMonthAgo = DateTime.Now.AddMonths(-1);

            if (from != null && to != null)
                result = result.Where(n => n.CreatedOn >= from && n.CreatedOn <= to);
            else
                result = result.Where(s => s.CreatedOn >= oneMonthAgo);

            var topSellingProduct = result
            .GroupBy(s => s.FinishedGoodsId)
            .Select(g => new SellingProductDto
            {
                FinishedGoodId = g.Key,
                FinishGoodName = productList.ContainsKey(g.Key) ? productList[g.Key].Name : "",
                Quantity = g.Sum(s => s.Quantity)
            })
            .OrderByDescending(g => g.Quantity)
            .Take(5)
            .ToList();

            return Ok(topSellingProduct);
        }

        [HttpGet("getLessFiveSellingProduct")]
        public ActionResult<SellingProductDto> GetLessFiveSellingProduct(DateTime? from, DateTime? to)
        {
            var productList = _finishGoodRepository.GetAll().ToDictionary(n => n.Id);
            var result = _salesInvoiceDetailsRepository.GetAll();
            var oneMonthAgo = DateTime.Now.AddMonths(-1);

            if (from != null && to != null)
                result = result.Where(n => n.CreatedOn >= from && n.CreatedOn <= to);
            else
                result = result.Where(s => s.CreatedOn >= oneMonthAgo);

            var topSellingProduct = result
            .GroupBy(s => s.FinishedGoodsId)
            .Select(g => new SellingProductDto
            {
                FinishedGoodId = g.Key,
                FinishGoodName = productList.ContainsKey(g.Key) ? productList[g.Key].Name : "",
                Quantity = g.Sum(s => s.Quantity)
            })
            .OrderBy(g => g.Quantity)
            .Take(5)
            .ToList();

            return Ok(topSellingProduct);
        }

        [HttpGet("getTopFiveSellerWithDueAmount")]
        public ActionResult<User> GetTopFiveSellerWithDueAmount(DateTime? from, DateTime? to)
        {
            var oneMonthAgo = DateTime.Now.AddMonths(-2);
            var result = _userRepository.GetAll();
            if (from != null && to != null)
                result = result.Where(n => n.CreatedOn >= from && n.CreatedOn <= to);
            else
                result = result.Where(s => s.CreatedOn >= oneMonthAgo);

            var topUsersWithDueAmount = result.Where(s => s.DueAmount > 0)
            .OrderByDescending(u => u.DueAmount)
            .Take(5)
            .ToList();

            return Ok(topUsersWithDueAmount);
        }

        [HttpGet("getDailyAccumulatedSalesSummary")]
        public ActionResult<ProductWiseRequisitionDto> GetDailyAccumulatedSalesSummary(DateTime? from, DateTime? to)
        {
            var oneMonthAgo = DateTime.Now.AddMonths(-1);
            var invoiceList = _salesInvoiceDetailsRepository.GetAll().ToList();

            if (from != null && to != null)
                invoiceList = invoiceList.Where(n => n.CreatedOn >= from && n.CreatedOn <= to).ToList();
            else
                invoiceList = invoiceList.Where(s => s.CreatedOn >= oneMonthAgo).ToList();

            var finishedGoodList = _finishGoodRepository.GetAll().ToDictionary(n => n.Id);

            var dailyFinishedGoodsQuantities = invoiceList
           .GroupBy(r => new { Date = r.CreatedDateTime.Date, r.FinishedGoodsId })
           .Select(g => new ProductWiseRequisitionDto
           {
               RequisitionDate = g.Key.Date,
               FinishedGoodId = g.Key.FinishedGoodsId,
               FinishedGoodName = finishedGoodList.ContainsKey(g.Key.FinishedGoodsId) ? finishedGoodList[g.Key.FinishedGoodsId].Name : "",
               TotalQuantity = g.Sum(r => r.Quantity)
           })
           .OrderBy(result => result.RequisitionDate)
           .ThenBy(result => result.FinishedGoodId)
           .ToList();

            return Ok(dailyFinishedGoodsQuantities);
        }
    }
}
