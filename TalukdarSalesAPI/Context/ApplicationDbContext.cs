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
    }
}
