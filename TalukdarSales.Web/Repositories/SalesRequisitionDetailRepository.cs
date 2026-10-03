using TalukdarSales.Web.Context;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;

namespace TalukdarSales.Web.Repositories
{
    public class SalesRequisitionDetailRepository : RepositoryBase<SalesRequisitionDetail>, ISalesRequisitionDetailRepository
    {
        public SalesRequisitionDetailRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
