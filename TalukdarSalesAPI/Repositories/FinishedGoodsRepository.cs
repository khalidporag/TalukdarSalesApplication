using TalukdarSalesAPI.Context;
using TalukdarSalesAPI.Interfaces;
using TalukdarSalesAPI.Models;

namespace TalukdarSalesAPI.Repositories
{
    public class FinishedGoodsRepository : RepositoryBase<FinishedGood>, IFinishedGoodsRepository
    {
        public FinishedGoodsRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
