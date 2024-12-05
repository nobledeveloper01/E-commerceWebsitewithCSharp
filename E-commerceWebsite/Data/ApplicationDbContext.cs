using E_commerceWebsite.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace E_commerceWebsite.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<AdminProduct> AdminProducts { get; set; }
        public DbSet<OrderAdmin> Orders { get; set; }
        public DbSet<CustomerEntity> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed initial admin user
            var adminUser = new User
            {
                Id = 1,
                FullName = "Admin User",
                Username = "admin",
                Email = "admin@example.com",
                Role = "Admin",
                PasswordHash = new PasswordHasher<User>().HashPassword(null, "Admin@123")
            };
            modelBuilder.Entity<User>().HasData(adminUser);

            // Configure precision for decimal properties
            modelBuilder.Entity<OrderAdmin>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<AdminProduct>()
                .Property(ap => ap.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<AdminProduct>()
                .Property(ap => ap.DiscountedPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.UnitPrice)
                .HasColumnType("decimal(18,2)");

            // Configure relationships
            modelBuilder.Entity<OrderAdmin>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderDetail>()
       .HasOne(od => od.Customer)
       .WithMany(c => c.OrderDetails)
       .HasForeignKey(od => od.CustomerId)
       .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CustomerEntity>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomerEntity>()
          .HasMany(c => c.OrderDetails)  
          .WithOne(od => od.Customer)
          .HasForeignKey(od => od.CustomerId); 
        }
    }
}
