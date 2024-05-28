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
    }
}
