using TalukdarSalesAPI.Context;
using TalukdarSalesAPI.Interfaces;
using TalukdarSalesAPI.Models;

namespace TalukdarSalesAPI.Repositories
{
    public class NoticeRepository : RepositoryBase<Notice>, INoticeRepository
    {
        public NoticeRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
