using TalukdarSalesAPI.Context;
using TalukdarSalesAPI.Interfaces;
using TalukdarSalesAPI.Models;

namespace TalukdarSalesAPI.Repositories
{
    public class ApplicationModuleRepository: RepositoryBase<ApplicationModule>, IApplicationModuleRepository
    {
        public ApplicationModuleRepository(ApplicationDbContext databaseContext) : base(databaseContext)
        {

        }
    }
}
