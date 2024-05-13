using TalukdarSalesAPI.Context;
using TalukdarSalesAPI.Interfaces;
using TalukdarSalesAPI.Models;
using TalukdarSalesAPI.Repositories;

namespace Project.Run.Repositories
{
    public class UserRepository: RepositoryBase<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext databaseContext) : base(databaseContext)
        {

        }
    }
}
