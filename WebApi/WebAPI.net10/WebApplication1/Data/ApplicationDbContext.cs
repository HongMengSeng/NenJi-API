using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.Entities;

namespace WebApplication1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Commodity> Commodities { get; set; } = null!;
        public DbSet<ShippingAddress> ShippingAddresses { get; set; } = null!;
        public DbSet<OrderMain> Orders { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("user");
                b.HasKey(x => x.UserId);
                b.Property(x => x.UserId).HasColumnName("user_id");
                b.Property(x => x.UserNo).HasColumnName("user_no").IsRequired();
                b.Property(x => x.PhoneNumber).HasColumnName("phone_number");
                b.Property(x => x.RegisterTime).HasColumnName("register_time");
                b.Property(x => x.WxOpenId).HasColumnName("wx_openid");
                b.Property(x => x.WxImage).HasColumnName("wx_image");
                b.Property(x => x.WxNickname).HasColumnName("wx_nickname");
                b.Property(x => x.RoleId).HasColumnName("role_id");
            });

            modelBuilder.Entity<Role>(b =>
            {
                b.ToTable("role");
                b.HasKey(x => x.RoleId);
                b.Property(x => x.RoleId).HasColumnName("role_id");
                b.Property(x => x.RoleName).HasColumnName("role_name");
            });

            modelBuilder.Entity<Category>(b =>
            {
                b.ToTable("category");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasColumnName("id");
                b.Property(x => x.CategoryName).HasColumnName("category_name");
                b.Property(x => x.CategoryDescription).HasColumnName("category_description");
                b.Property(x => x.CategoryStatus).HasColumnName("category_status");
                b.Property(x => x.SortOrder).HasColumnName("sort_order");
            });

            modelBuilder.Entity<Commodity>(b =>
            {
                b.ToTable("commodity");
                b.HasKey(x => x.CommodityId);
                b.Property(x => x.CommodityId).HasColumnName("commodity_id");
                b.Property(x => x.ProductName).HasColumnName("product_name");
                b.Property(x => x.SpecDescription).HasColumnName("spec_description");
                b.Property(x => x.InStock).HasColumnName("in_stock");
                b.Property(x => x.Quantity).HasColumnName("quantity");
                b.Property(x => x.ProductStatus).HasColumnName("product_status");
                b.Property(x => x.CategoryId).HasColumnName("category_id");
                b.Property(x => x.ImageUrl).HasColumnName("image_url");

                // the new column holding raw image bytes; make sure the database
                // schema has been updated (e.g. ALTER TABLE ADD image_data LONGBLOB).
                b.Property(x => x.ImageData)
                    .HasColumnName("image_data")
                    .HasColumnType("longblob");
            });

            modelBuilder.Entity<ShippingAddress>(b =>
            {
                b.ToTable("shipping_address");
                b.HasKey(x => x.AddressId);
                b.Property(x => x.AddressId).HasColumnName("address_id");
                b.Property(x => x.UserId).HasColumnName("user_id");
                b.Property(x => x.ContentName).HasColumnName("content_name");
                b.Property(x => x.Province).HasColumnName("province");
                b.Property(x => x.City).HasColumnName("city");
                b.Property(x => x.MunicipalDistricts).HasColumnName("municipal_districts");
                b.Property(x => x.Town).HasColumnName("town");
                b.Property(x => x.HouseNumber).HasColumnName("house_number");
            });

            modelBuilder.Entity<OrderMain>(b =>
            {
                b.ToTable("order");
                b.HasKey(x => x.OrderId);
                b.Property(x => x.OrderId).HasColumnName("order_id");
                b.Property(x => x.OrderNumber).HasColumnName("order_number");
                b.Property(x => x.UserId).HasColumnName("user_id");
                b.Property(x => x.ActualPayment).HasColumnName("actual_payment");
                b.Property(x => x.AddressId).HasColumnName("address_id");
                b.Property(x => x.OrderType).HasColumnName("order_type");
                b.Property(x => x.TotalAmount).HasColumnName("total_amount");
                b.Property(x => x.OrderStatus).HasColumnName("order_status");
                b.Property(x => x.PaymentStatus).HasColumnName("payment_status");
                b.Property(x => x.DeliveryMethods).HasColumnName("delivery_methods");
                b.Property(x => x.ShippingAddress).HasColumnName("shipping_address");
                b.Property(x => x.ContactPerson).HasColumnName("contact_person");
                b.Property(x => x.ContactNumber).HasColumnName("contact_number");
                b.Property(x => x.OrderCreateTime).HasColumnName("order_create_time");
                b.Property(x => x.PaymentTime).HasColumnName("payment_time");
                b.Property(x => x.PaymentMethods).HasColumnName("payment_methods");
                b.Property(x => x.OrderFormId).HasColumnName("order_form_id");
                b.Property(x => x.SnapshotReceiverName).HasColumnName("snapshot_receiver_name");
                b.Property(x => x.SnapshotReceiverPhone).HasColumnName("snapshot_receiver_phone");
                b.Property(x => x.SnapshotDeliveryAddress).HasColumnName("snapshot_delivery_address");
                b.Property(x => x.SnapshotUserNickname).HasColumnName("snapshot_user_nickname");
            });
        }
    }
}
