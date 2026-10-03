using TalukdarSales.Web.Context;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;

namespace TalukdarSales.Web.Repositories
{
    public class UserTypeRepository : RepositoryBase<UserType>, IUserTypeRepository
    {
        public UserTypeRepository(ApplicationDbContext databaseContext) : base(databaseContext)
        {

        }
    }
}
