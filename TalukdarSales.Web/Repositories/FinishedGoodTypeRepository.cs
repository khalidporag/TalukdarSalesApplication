using TalukdarSales.Web.Context;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;

namespace TalukdarSales.Web.Repositories
{
    public class FinishedGoodTypeRepository : RepositoryBase<FinishGoodType>, IFinishedGoodTypeRepository
    {
        public FinishedGoodTypeRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
