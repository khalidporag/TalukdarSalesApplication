using TalukdarSales.Web.Context;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;

namespace TalukdarSales.Web.Repositories
{
    public class ApplicationModuleRepository: RepositoryBase<ApplicationModule>, IApplicationModuleRepository
    {
        public ApplicationModuleRepository(ApplicationDbContext databaseContext) : base(databaseContext)
        {

        }
    }
}
