using TalukdarSalesAPI.Context;
using TalukdarSalesAPI.Interfaces;
using TalukdarSalesAPI.Models;

namespace TalukdarSalesAPI.Repositories
{
    public class RoleWisePermissionRepository : RepositoryBase<RoleWisePermission>, IRoleWisePermissionRepository
    {
        public RoleWisePermissionRepository(ApplicationDbContext databaseContext) : base(databaseContext)
        {

        }
    }
}
