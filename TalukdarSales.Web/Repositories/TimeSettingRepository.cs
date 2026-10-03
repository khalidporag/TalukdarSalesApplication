using TalukdarSales.Web.Context;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;

namespace TalukdarSales.Web.Repositories
{
    public class TimeSettingRepository : RepositoryBase<TimeSetting>, ITimeSettingRepository
    {
        public TimeSettingRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
