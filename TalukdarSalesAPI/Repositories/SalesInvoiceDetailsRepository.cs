using TalukdarSalesAPI.Context;
using TalukdarSalesAPI.Interfaces;
using TalukdarSalesAPI.Models;

namespace TalukdarSalesAPI.Repositories
{
    public class SalesInvoiceDetailsRepository : RepositoryBase<SalesInvoiceDetails>, ISalesInvoiceDetailsRepository
    {
        public SalesInvoiceDetailsRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
