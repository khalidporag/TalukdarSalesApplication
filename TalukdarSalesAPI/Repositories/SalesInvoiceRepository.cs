using TalukdarSalesAPI.Context;
using TalukdarSalesAPI.Interfaces;
using TalukdarSalesAPI.Models;

namespace TalukdarSalesAPI.Repositories
{
    public class SalesInvoiceRepository : RepositoryBase<SalesInvoice>, ISalesInvoiceRepository
    {
        public SalesInvoiceRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
