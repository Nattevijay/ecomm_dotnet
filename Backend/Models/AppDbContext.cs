using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data
{
    /// <summary>
    /// AppDbContext is the bridge between your C# models and Oracle DB.
    ///
    /// HOW IT WORKS:
    ///   - Each DbSet<T> = one table in Oracle
    ///   - OnModelCreating() configures indexes, relationships, constraints
    ///   - EF Core Migrations reads this file to create/update Oracle tables
    ///
    /// AFTER ANY MODEL CHANGE run these 2 commands:
    ///   dotnet ef migrations add YourMigrationName
    ///   dotnet ef database update
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ─── Tables (DbSets) ─────────────────────────────────────────────
        // Each property below = one table in your Oracle database

        public DbSet<User>      Users      { get; set; }
        public DbSet<Product>   Products   { get; set; }
        public DbSet<Category>  Categories { get; set; }
        public DbSet<Order>     Orders     { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review>    Reviews    { get; set; }

        // ─── Model Configuration ─────────────────────────────────────────
        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            // ── USER ──────────────────────────────────────────────────────
            mb.Entity<User>(entity =>
            {
                // Email must be unique across all users
                entity.HasIndex(u => u.Email)
                      .IsUnique()
                      .HasDatabaseName("UX_USERS_EMAIL");

                // Default role for new users
                entity.Property(u => u.Role)
                      .HasDefaultValue("Customer");

                entity.Property(u => u.IsBanned)
                      .HasDefaultValue(false);

                entity.Property(u => u.CreatedAt)
                      .HasDefaultValueSql("SYSTIMESTAMP");
            });

            // ── CATEGORY ─────────────────────────────────────────────────
            mb.Entity<Category>(entity =>
            {
                // Slug must be unique: "electronics", "clothing"
                entity.HasIndex(c => c.Slug)
                      .IsUnique()
                      .HasDatabaseName("UX_CATEGORIES_SLUG");

                entity.Property(c => c.IsActive)
                      .HasDefaultValue(true);
            });

            // ── PRODUCT ───────────────────────────────────────────────────
            mb.Entity<Product>(entity =>
            {
                // One Seller (User) has many Products
                entity.HasOne(p => p.Seller)
                      .WithMany(u => u.Products)
                      .HasForeignKey(p => p.SellerId)
                      .OnDelete(DeleteBehavior.Restrict); // don't delete products if seller deleted

                // One Category has many Products
                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.SetNull); // set null if category deleted

                entity.Property(p => p.IsActive)
                      .HasDefaultValue(true);

                entity.Property(p => p.IsApproved)
                      .HasDefaultValue(true);

                entity.Property(p => p.DiscountPercent)
                      .HasPrecision(5, 2);
    

                entity.Property(p => p.CreatedAt)
                      .HasDefaultValueSql("SYSTIMESTAMP");
            });

            // ── ORDER ─────────────────────────────────────────────────────
            mb.Entity<Order>(entity =>
            {
                // One User (Customer) has many Orders
                entity.HasOne(o => o.User)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(o => o.Status)
                      .HasDefaultValue("Pending");

                entity.Property(o => o.PaymentMethod)
                      .HasDefaultValue("COD");

                entity.Property(o => o.PaymentStatus)
                      .HasDefaultValue("Pending");

                entity.Property(o => o.CreatedAt)
                      .HasDefaultValueSql("SYSTIMESTAMP");

                // Index to quickly find all orders by a user
                entity.HasIndex(o => o.UserId)
                      .HasDatabaseName("IX_ORDERS_USER_ID");

                // Index to filter by status quickly
                entity.HasIndex(o => o.Status)
                      .HasDatabaseName("IX_ORDERS_STATUS");
            });

            // ── ORDER ITEM ────────────────────────────────────────────────
            mb.Entity<OrderItem>(entity =>
            {
                // One Order has many OrderItems
                entity.HasOne(oi => oi.Order)
                      .WithMany(o => o.Items)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade); // delete items when order deleted

                // One Product appears in many OrderItems
                entity.HasOne(oi => oi.Product)
                      .WithMany(p => p.OrderItems)
                      .HasForeignKey(oi => oi.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── REVIEW ────────────────────────────────────────────────────
            mb.Entity<Review>(entity =>
            {
                // One User has many Reviews
                entity.HasOne(r => r.User)
                      .WithMany(u => u.Reviews)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // One Product has many Reviews
                entity.HasOne(r => r.Product)
                      .WithMany(p => p.Reviews)
                      .HasForeignKey(r => r.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                // A user can only review a product ONCE
                entity.HasIndex(r => new { r.UserId, r.ProductId })
                      .IsUnique()
                      .HasDatabaseName("UX_REVIEWS_USER_PRODUCT");

                entity.Property(r => r.CreatedAt)
                      .HasDefaultValueSql("SYSTIMESTAMP");
            });
        }
    }
}