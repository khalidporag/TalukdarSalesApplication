using TalukdarSalesAPI.Context;
using TalukdarSalesAPI.Interfaces;
using TalukdarSalesAPI.Models;

namespace TalukdarSalesAPI.Repositories
{
    public class CollectionLedgerRepository : RepositoryBase<CollectionLedger>, ICollectionLedgerRepository
    {
        public CollectionLedgerRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
