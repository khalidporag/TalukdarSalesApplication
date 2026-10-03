using TalukdarSales.Web.Context;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;

namespace TalukdarSales.Web.Repositories
{
    public class SalesInvoiceDetailsRepository : RepositoryBase<SalesInvoiceDetails>, ISalesInvoiceDetailsRepository
    {
        public SalesInvoiceDetailsRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
