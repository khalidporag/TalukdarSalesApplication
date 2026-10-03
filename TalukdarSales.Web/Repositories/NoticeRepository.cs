using TalukdarSales.Web.Context;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;

namespace TalukdarSales.Web.Repositories
{
    public class NoticeRepository : RepositoryBase<Notice>, INoticeRepository
    {
        public NoticeRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
