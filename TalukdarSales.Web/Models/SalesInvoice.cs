namespace TalukdarSales.Web.Models
{
    public class SalesInvoice : EntityBase
    {
        public string InvoiceSerialNo { get; set; }
        public int SalesRequisitionId { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public double Quantity { get; set; }
        public int UserId { get; set; }
        public double TotalPrice { get; set; }
        public double DiscountAmount { get; set; }
        public double DiscountPercentage { get; set; }
        public double CollectionAmount { get; set; }
        public int CreatedByUserId { get; set; }
        /// <summary>Value of goods returned by credit notes. TotalPrice already has it taken off.</summary>
        public double ReturnedAmount { get; set; }
    }
}
