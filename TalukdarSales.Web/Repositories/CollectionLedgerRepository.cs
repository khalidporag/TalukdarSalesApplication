using TalukdarSales.Web.Context;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;

namespace TalukdarSales.Web.Repositories
{
    public class CollectionLedgerRepository : RepositoryBase<CollectionLedger>, ICollectionLedgerRepository
    {
        public CollectionLedgerRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
