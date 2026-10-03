using TalukdarSales.Web.Context;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;

namespace TalukdarSales.Web.Repositories
{
    public class SalesInvoiceRepository : RepositoryBase<SalesInvoice>, ISalesInvoiceRepository
    {
        public SalesInvoiceRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
