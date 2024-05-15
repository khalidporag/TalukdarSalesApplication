using TalukdarSalesAPI.Context;
using TalukdarSalesAPI.Interfaces;
using TalukdarSalesAPI.Models;

namespace TalukdarSalesAPI.Repositories
{
    public class UserRoleMappingRepository: RepositoryBase<UserRoleMapping>, IUserRoleMappingRepository
    {
        public UserRoleMappingRepository(ApplicationDbContext databaseContext) : base(databaseContext)
        {

        }
    }
}
