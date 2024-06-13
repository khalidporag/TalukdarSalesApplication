using TalukdarSalesAPI.Context;
using TalukdarSalesAPI.Interfaces;
using TalukdarSalesAPI.Models;

namespace TalukdarSalesAPI.Repositories
{
    public class TimeSettingRepository : RepositoryBase<TimeSetting>, ITimeSettingRepository
    {
        public TimeSettingRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
