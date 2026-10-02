using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Run.Repositories;
using TalukdarSalesAPI.Context;
using TalukdarSalesAPI.Interfaces;
using TalukdarSalesAPI.Models;
using TalukdarSalesAPI.Models.Dto;
using TalukdarSalesAPI.Repositories;

namespace TalukdarSalesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class SalesInvoiceController : ControllerBase
    {
        private readonly ICollectionLedgerRepository _collectionLedgerRepository;
        private readonly ISalesInvoiceRepository _salesInvoiceRepository;
        private readonly ISalesInvoiceDetailsRepository _salesInvoiceDetailsRepository;
        private readonly ISalesRequisitionRepository _salesRequisitioinRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISalesRequisitionDetailRepository _salesRequisitionDetailRepository;
        private readonly IFinishedGoodsRepository _finishGoodRepository;
        private readonly ApplicationDbContext _context;

        public SalesInvoiceController(ICollectionLedgerRepository collectionLedgerRepository,
            ISalesInvoiceRepository salesInvoiceRepository,
            ISalesInvoiceDetailsRepository salesInvoiceDetailsRepository,
            ISalesRequisitionRepository salesRequisitioinRepository,
            IUserRepository userRepository,
            ISalesRequisitionDetailRepository salesRequisitionDetailRepository,
            IFinishedGoodsRepository finishGoodRepository,
            ApplicationDbContext context)
        {
            _collectionLedgerRepository = collectionLedgerRepository;
            _salesInvoiceRepository = salesInvoiceRepository;
            _salesInvoiceDetailsRepository = salesInvoiceDetailsRepository;
            _salesRequisitioinRepository = salesRequisitioinRepository;
            _userRepository = userRepository;
            _salesRequisitionDetailRepository = salesRequisitionDetailRepository;
            _finishGoodRepository = finishGoodRepository;
            _context = context;
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
        public ActionResult<CollectionLedgerDto> GetCollectionHistory(int? userId, int? salesInvoiceId, DateTime? from, DateTime? to)
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
            if(from != null && to != null)
            {
                to = to.Value.AddDays(1).AddSeconds(-1);
                allCollection = allCollection.Where(n => n.CreatedOn >= from && n.CreatedOn <= to);
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

            var totalCollectionAmount = collectionHistory
                .Where(n => n.CollectionAmount > 0)
                .Sum(n => n.CollectionAmount);

            var result = new CollectionHistoryDto
            {
                CollectionLedgerInfo = collectionHistory ,
                TotalCollectionHistory = (double)totalCollectionAmount
            };

            return Ok(result);
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
            using var bulkTransaction = _context.Database.BeginTransaction();
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
                    userInfo.DueAmount += (decimal)createSalesInvoice.TotalPrice;
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
            bulkTransaction.Commit();
            return Ok(new
            {
                Status = 200,
                Message = "Sales Invoice Created!"
            });
        }

        [HttpPost("ccc")]
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
                userInfo.DueAmount += (decimal)createSalesInvoice.TotalPrice;
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

            //Pdf

            var finishGoodList = _finishGoodRepository.GetAll().ToDictionary(n => n.Id);

            using (var memoryStream = new MemoryStream())
            {
                Document document = new Document();
                PdfWriter.GetInstance(document, memoryStream).CloseStream = false;
                document.Open();

                document.Add(new Paragraph("Sales Invoice"));
                document.Add(new Paragraph("Invoice No: " + createSalesInvoice.InvoiceSerialNo));
                document.Add(new Paragraph("Date: " + createSalesInvoice.CreatedDateTime.ToString("yyyy-MM-dd")));
                //document.Add(new Paragraph("User ID: " + createSalesInvoice.UserId));
                document.Add(new Paragraph("Total Price: BDT " + createSalesInvoice.TotalPrice.ToString("F2")));
                document.Add(new Paragraph("Total Quantity: " + createSalesInvoice.Quantity.ToString("F2")));
                //document.Add(new Paragraph("Sales Requisition ID: " + createSalesInvoice.SalesRequisitionId));
                document.Add(new Paragraph("\nInvoice Details:"));

                PdfPTable table = new PdfPTable(4); // 4 columns
                table.AddCell("Finished Goods");
                table.AddCell("Quantity");
                table.AddCell("Price");
                table.AddCell("Total");
                int i = 1;
                foreach (var details in invoiceDetailsList)
                {
                    
                    string fontPath = Path.Combine("wwwroot", "ttf", "kalpurus.ttf");
                    //var fontPath = Path.Combine("wwwroot", "ttf", "font");
                    //var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

                    // Create a Font object
                    //var font = new Font(baseFont, 12, Font.NORMAL);

                    var goodName = finishGoodList.ContainsKey(details.FinishedGoodsId) ? finishGoodList[details.FinishedGoodsId].Name : "";

                    // Add content to the table with the specified font
                    //table.AddCell(new Phrase(goodName, font));
                    //table.AddCell(new Phrase(details.Quantity.ToString("F2"), font));
                    //table.AddCell(new Phrase(details.Price.ToString("F2"), font));
                    //table.AddCell(new Phrase((details.Quantity * details.Price).ToString("F2"), font));

                    table.AddCell(i+ ". " + goodName);
                    table.AddCell(details.Quantity.ToString("F2"));
                    table.AddCell(details.Price.ToString("F2"));
                    table.AddCell((details.Quantity * details.Price).ToString("F2"));
                    i = i + 1;
                }

                document.Add(table);
                document.Close();

                memoryStream.Position = 0;

                var directoryPath = Path.Combine("wwwroot", "pdf", "invoices");
                var filePath = Path.Combine(directoryPath, "Invoice_" + createSalesInvoice.Id + ".pdf");

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    memoryStream.CopyTo(fileStream);
                }
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
            if (collectAmountObj == null || collectAmountObj.CollectionAmount == null || collectAmountObj.CollectionAmount <= 0)
                return BadRequest(new { Message = "Invalid collection amount" });

            var invoiceInfo = _salesInvoiceRepository.GetSingle(collectAmountObj.SalesInvoiceId);
            if (invoiceInfo == null)
                return NotFound(new { Message = "Invoice not found" });

            var userInfo = _userRepository.GetSingle(invoiceInfo.UserId);
            if (userInfo == null)
                return NotFound(new { Message = "User not found" });

            // Outstanding invoices of the billed user, oldest first
            var outstandingInvoices = _salesInvoiceRepository.GetAll()
                .Where(i => i.UserId == invoiceInfo.UserId && i.TotalPrice > i.CollectionAmount)
                .OrderBy(i => i.CreatedDateTime)
                .ToList();

            var remaining = collectAmountObj.CollectionAmount.Value;
            var totalOutstanding = outstandingInvoices.Sum(i => i.TotalPrice - i.CollectionAmount);
            if (remaining > totalOutstanding + 0.005)
                return BadRequest(new { Message = "Collection amount exceeds the outstanding amount" });

            using (var transaction = _context.Database.BeginTransaction())
            {
                var totalCollected = 0.0;
                foreach (var invoice in outstandingInvoices)
                {
                    if (remaining <= 0)
                        break;

                    var applied = Math.Min(remaining, invoice.TotalPrice - invoice.CollectionAmount);

                    invoice.CollectionAmount += applied;
                    _salesInvoiceRepository.Update(invoice);

                    var ledger = new CollectionLedger();
                    ledger.UserId = invoice.UserId;
                    ledger.SalesInvoiceId = invoice.Id;
                    ledger.CollectionAmount = applied;
                    ledger.PaymentMethod = collectAmountObj.PaymentMethod;
                    _collectionLedgerRepository.Add(ledger);

                    remaining -= applied;
                    totalCollected += applied;
                }

                userInfo.DueAmount -= (decimal)totalCollected;
                _userRepository.Update(userInfo);

                _salesInvoiceRepository.Commit();
                transaction.Commit();
            }

            return Ok(new
            {
                Status = 200,
                Message = "Amount Collected!"
            });
        }

        [HttpGet("getSalesInvoiceList")]
        public ActionResult<SalesInvoice> GetSalesInvoiceList(int? userId, DateTime? from, DateTime? to)
        {
            var userList = _userRepository.GetAll().ToDictionary(n => n.Id);
            var requisitionList = _salesRequisitioinRepository.GetAll().ToDictionary(n => n.Id);
            var invoiceList = _salesInvoiceRepository.GetAll();
            if (userId != null)
                invoiceList = invoiceList.Where(n => n.UserId == userId);
            if(from != null && to != null)
            {
                to = to.Value.AddDays(1).AddSeconds(-1);
                invoiceList = invoiceList.Where(n => n.CreatedDateTime >= from && n.CreatedDateTime <= to);
            }

            var info = invoiceList.AsEnumerable().Select(s => new SalesInvoiceDto
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

            var _totalCollectionAmount = info
                .Where(n => n.CollectionAmount > 0)
                .Sum(n => n.CollectionAmount);

            var _totalOfTotalPrice = info
                .Where(n => n.TotalPrice > 0)
                .Sum(n => n.TotalPrice);

            var _totalDue = _totalOfTotalPrice - _totalCollectionAmount;

            var result = new SalesInvoiceInfoDto
            {
                SalesInvoiceInfo = info,
                TotalCollectionAmount = _totalCollectionAmount,
                TotalOfTotalPrice = _totalOfTotalPrice,
                TotalDueAmount = _totalDue
            };

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
                result = result.Where(n => n.CreatedOn >= from && n.CreatedOn <= to.Value.AddDays(1).AddSeconds(-1));
            else
                result = result.Where(s => s.CreatedOn >= oneMonthAgo);

            var topSellers = result
            .GroupBy(s => s.UserId)
            .Select(g => new TopSellerDto
            {
                UserId = g.Key,
                UserName = userList.ContainsKey(g.Key)? userList[g.Key].FirstName + " " + userList[g.Key].LastName : "",
                ImageName = userList.ContainsKey(g.Key) ? userList[g.Key].ImageName : " ",
                TotalAmount = g.Sum(s => s.TotalPrice)
            })
            .OrderByDescending(g => g.TotalAmount)
            .Take(5)
            .ToList();

            return Ok(topSellers);
        }

        [HttpGet("GetInvoiceDetails")]
        public ActionResult<SalesInvoiceDto> GetInvoiceDetailsByInvoiceId(int invoiceId)
        {
            var invoiceHeader = _salesInvoiceRepository.GetSingle(invoiceId);
            var invoiceDetails = _salesInvoiceDetailsRepository.GetAll().Where(i => i.SalesInvoiceId == invoiceId).ToList();
            var userInfo = _userRepository.GetSingle(i => i.Id == invoiceHeader.UserId);

            List<SalesInvoiceDetailsDto> salesInvoiceDetailsDtos = new List<SalesInvoiceDetailsDto>();

            foreach(var item in invoiceDetails)
            {
                salesInvoiceDetailsDtos.Add(new SalesInvoiceDetailsDto
                {
                    Id = item.Id,
                    CreatedDateTime = item.CreatedDateTime,
                    DiscountAmount = item.DiscountAmount,
                    DiscountPercentage = item.DiscountPercentage,
                    FinishedGoodsId = item.FinishedGoodsId,
                    FinishGoodName = _finishGoodRepository.GetSingle(item.FinishedGoodsId).Name,
                    Price = item.Price,
                    Quantity = item.Quantity
                });
            }

            SalesInvoiceDto salesInvoiceDto = new SalesInvoiceDto()
            {
                Id = invoiceHeader.Id,
                Quantity = invoiceHeader.Quantity,
                CollectionAmount = invoiceHeader.CollectionAmount,
                CreatedDateTime = invoiceHeader.CreatedDateTime,
                DiscountAmount = invoiceHeader.DiscountAmount,
                DiscountPercentage = invoiceHeader.DiscountPercentage,
                InvoiceNumber = invoiceHeader.InvoiceSerialNo,
                TotalPrice = invoiceHeader.TotalPrice,
                UserId = invoiceHeader.UserId,
                UserName = userInfo.FirstName +" "+userInfo.LastName,
                SalesInvoiceDetails = salesInvoiceDetailsDtos,
                UserSequencialId = userInfo.SequencialUserId
            };
            
            return Ok(salesInvoiceDto);
        }

        [HttpGet("getTopFiveSellingProduct")]
        public ActionResult<SellingProductDto> GetTopFiveSellingProduct(DateTime? from, DateTime? to)
        {
            var productList = _finishGoodRepository.GetAll().ToDictionary(n => n.Id);
            var result = _salesInvoiceDetailsRepository.GetAll();
            var oneMonthAgo = DateTime.Now.AddMonths(-1);

            if (from != null && to != null)
                result = result.Where(n => n.CreatedOn >= from && n.CreatedOn <= to.Value.AddDays(1).AddSeconds(-1));
            else
                result = result.Where(s => s.CreatedOn >= oneMonthAgo);

            var topSellingProduct = result
            .GroupBy(s => s.FinishedGoodsId)
            .Select(g => new SellingProductDto
            {
                FinishedGoodId = g.Key,
                FinishGoodName = productList.ContainsKey(g.Key) ? productList[g.Key].Name : "",
                LogoName = productList.ContainsKey(g.Key) ? productList[g.Key].LogoName : "",
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
                result = result.Where(n => n.CreatedOn >= from && n.CreatedOn <= to.Value.AddDays(1).AddSeconds(-1));
            else
                result = result.Where(s => s.CreatedOn >= oneMonthAgo);

            var topSellingProduct = result
            .GroupBy(s => s.FinishedGoodsId)
            .Select(g => new SellingProductDto
            {
                FinishedGoodId = g.Key,
                FinishGoodName = productList.ContainsKey(g.Key) ? productList[g.Key].Name : "",
                LogoName = productList.ContainsKey(g.Key) ? productList[g.Key].LogoName : "",
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

            var oneMonthAgo = DateTime.Now.AddMonths(-1);
            var salesInvoices = _salesInvoiceRepository.GetAll();

            if(from!= null && to != null)
            {
                salesInvoices = salesInvoices.Where(x=>x.CreatedOn >= from && x.CreatedOn <= to.Value.AddDays(1).AddSeconds(-1));
            }
            else
            {
                salesInvoices = salesInvoices.Where(s => s.CreatedOn >= oneMonthAgo);
            }

            var temp = (from invoice in salesInvoices
                        group invoice by new { invoice.TotalPrice, invoice.CollectionAmount, invoice.UserId } into g
                        select new
                        {
                            DueAmount = g.Key.TotalPrice - g.Key.CollectionAmount,
                            g.Key.UserId
                        })
                        .OrderByDescending(x => x.DueAmount)
                        .Take(5)
                        .ToList();

            var result = (from user in _userRepository.GetAll()
                          join t in temp on user.Id equals t.UserId
                          select new
                          {
                              user.Id,
                              user.UserTypeId,
                              user.FirstName,
                              user.LastName,
                              user.PhoneNumber,
                              t.DueAmount,
                              user.ImageName,
                              user.Username
                          })
                          .ToList();

            return Ok(result);
        }

        [HttpGet("getDailyAccumulatedSalesSummary")]
        public ActionResult<ProductWiseRequisitionDto> GetDailyAccumulatedSalesSummary(DateTime? from, DateTime? to)
        {
            var oneMonthAgo = DateTime.Now.AddMonths(-1);
            var invoiceList = _salesInvoiceDetailsRepository.GetAll().ToList();

            if (from != null && to != null)
                invoiceList = invoiceList.Where(n => n.CreatedOn >= from && n.CreatedOn <= to.Value.AddDays(1).AddSeconds(-1)).ToList();
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
