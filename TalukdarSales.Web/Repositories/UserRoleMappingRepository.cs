using TalukdarSales.Web.Context;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;

namespace TalukdarSales.Web.Repositories
{
    public class UserRoleMappingRepository: RepositoryBase<UserRoleMapping>, IUserRoleMappingRepository
    {
        public UserRoleMappingRepository(ApplicationDbContext databaseContext) : base(databaseContext)
        {

        }
    }
}
