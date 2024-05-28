using TalukdarSalesAPI.Context;
using TalukdarSalesAPI.Interfaces;
using TalukdarSalesAPI.Models;

namespace TalukdarSalesAPI.Repositories
{
    public class FinishedGoodTypeRepository : RepositoryBase<FinishGoodType>, IFinishedGoodTypeRepository
    {
        public FinishedGoodTypeRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
