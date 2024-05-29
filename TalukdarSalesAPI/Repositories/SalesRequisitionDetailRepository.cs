using TalukdarSalesAPI.Context;
using TalukdarSalesAPI.Interfaces;
using TalukdarSalesAPI.Models;

namespace TalukdarSalesAPI.Repositories
{
    public class SalesRequisitionDetailRepository : RepositoryBase<SalesRequisitionDetail>, ISalesRequisitionDetailRepository
    {
        public SalesRequisitionDetailRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
