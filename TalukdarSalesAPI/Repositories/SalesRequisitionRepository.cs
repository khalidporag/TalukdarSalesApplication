using TalukdarSalesAPI.Context;
using TalukdarSalesAPI.Interfaces;
using TalukdarSalesAPI.Models;

namespace TalukdarSalesAPI.Repositories
{
    public class SalesRequisitionRepository : RepositoryBase<SalesRequisition>, ISalesRequisitionRepository
    {
        public SalesRequisitionRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
