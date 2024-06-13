using Microsoft.EntityFrameworkCore;
using TalukdarSalesAPI.Models;

namespace TalukdarSalesAPI.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserType> UserTypes { get; set; }
        public DbSet<ApplicationRole> ApplicationRoles { get; set; }
        public DbSet<ApplicationModule> ApplicationModules { get; set; }
        public DbSet<RoleWisePermission> RoleWisePermissions { get; set; }
        public DbSet<UserRoleMapping> UserRoleMappings { get; set; }
        public DbSet<FinishGoodType> FinishGoodTypes { get; set; }
        public DbSet<FinishedGood> FinishedGoods { get; set; }
        public DbSet<SalesRequisition> SalesRequisitions { get; set; }
        public DbSet<SalesRequisitionDetail> SalesRequisitionDetails { get; set; }
        public DbSet<CollectionLedger> CollectionLedgers { get; set; }
        public DbSet<SalesInvoice> SalesInvoices { get; set; }
        public DbSet<SalesInvoiceDetails> SalesInvoiceDetails { get; set; }
        public DbSet<TimeSetting> TimeSettings { get; set; }
    }
}
