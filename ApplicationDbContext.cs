using E_CommerceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderProducts> OrderProducts { get; set; }
        public DbSet<Review> Reviews { get; set; }

        public DbSet<Category> Categories { get; set; } = default!;
        public DbSet<Supplier> Suppliers { get; set; } = default!;

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                        .HasIndex(u => u.Email)
                        .IsUnique();

            modelBuilder.Entity<Category>()
                        .HasIndex(c => c.Name)
                        .IsUnique();

            modelBuilder.Entity<Supplier>()
                        .HasIndex(s => s.Name)
                        .IsUnique();
            modelBuilder.Entity<Order>()
                        .Property(o => o.Status)
                        .HasConversion<string>()
                        .HasMaxLength(20);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => new { rt.UID, rt.TokenHash })
                .IsUnique();

            modelBuilder.Entity<Review>()
                .HasIndex(r => new { r.PID, r.UID })
                .IsUnique();   // one review per user per product

            
        }
    }
}
