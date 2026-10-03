using TalukdarSales.Web.Context;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;

namespace TalukdarSales.Web.Repositories
{
    public class ApplicationRoleRepository: RepositoryBase<ApplicationRole>, IApplicationRoleRepository
    {
        public ApplicationRoleRepository(ApplicationDbContext databaseContext) : base(databaseContext)
        {

        }
    }
}
