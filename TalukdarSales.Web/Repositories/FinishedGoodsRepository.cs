using TalukdarSales.Web.Context;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;

namespace TalukdarSales.Web.Repositories
{
    public class FinishedGoodsRepository : RepositoryBase<FinishedGood>, IFinishedGoodsRepository
    {
        public FinishedGoodsRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
