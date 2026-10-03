using TalukdarSales.Web.Context;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;
using TalukdarSales.Web.Repositories;

namespace Project.Run.Repositories
{
    public class UserRepository: RepositoryBase<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext databaseContext) : base(databaseContext)
        {

        }
    }
}
