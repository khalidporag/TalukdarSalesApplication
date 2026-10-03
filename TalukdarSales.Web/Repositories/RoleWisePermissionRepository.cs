using TalukdarSales.Web.Context;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;

namespace TalukdarSales.Web.Repositories
{
    public class RoleWisePermissionRepository : RepositoryBase<RoleWisePermission>, IRoleWisePermissionRepository
    {
        public RoleWisePermissionRepository(ApplicationDbContext databaseContext) : base(databaseContext)
        {

        }
    }
}
