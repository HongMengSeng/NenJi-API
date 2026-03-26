using Microsoft.EntityFrameworkCore;

using WebAdminApi.Entities;


namespace WebAdminApi.DBs
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<AdminStaffs> AdminStaffs { get; set; }
        public DbSet<WeChatUser> WeChatUsers { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Role_Staffs> Role_Staffs { get; set; }
    }
}
