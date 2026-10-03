using TalukdarSales.Web.Context;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;

namespace TalukdarSales.Web.Repositories
{
    public class SalesRequisitionRepository : RepositoryBase<SalesRequisition>, ISalesRequisitionRepository
    {
        public SalesRequisitionRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
