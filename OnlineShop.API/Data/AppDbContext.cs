using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineShop.API.Models;
using OnlineShop.API.Models.Entities;

namespace OnlineShop.API.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

        // Added DbSets for orders and order items
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Rename Identity tables
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

            // Product configuration
            builder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Price)
                      .HasColumnType("decimal(18,2)");

                entity.Property(p => p.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(p => p.Description)
                      .HasMaxLength(1000);

                entity.Property(p => p.ImageUrl)
                      .HasMaxLength(500);

                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Category configuration
            builder.Entity<Category>(entity =>
            {
                entity.Property(c => c.Name)
                      .IsRequired()
                      .HasMaxLength(100);
            });

            // Order configuration
            builder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.Status).IsRequired();

                entity.HasMany(o => o.OrderItems)
                      .WithOne(oi => oi.Order)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // OrderItem configuration
            builder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(oi => oi.Id);

                entity.Property(oi => oi.UnitPrice)
                      .HasColumnType("decimal(18,2)");

                // Configure relationship to Product
                entity.HasOne(oi => oi.Product)
                      .WithMany() // If Product has no navigation property for OrderItems
                      .HasForeignKey(oi => oi.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
