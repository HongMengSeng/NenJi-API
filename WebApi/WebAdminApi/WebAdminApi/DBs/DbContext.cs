using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.Entities;


namespace WebAdminApi.DBs
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}
