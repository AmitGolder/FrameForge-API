using FrameForge.Models;
using Microsoft.EntityFrameworkCore;

namespace FrameForge.Data
{
    public class FrameForgeDbContext : DbContext
    {
        public FrameForgeDbContext(
            DbContextOptions<FrameForgeDbContext> options
        ) : base(options)
        {
        }

        // Existing tables
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        // Catalog tables
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Scale> Scales { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Series> Series { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================
            // Decimal configurations
            // =========================

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasColumnType("decimal(18,2)");


            // =========================
            // Brand → Series
            // =========================

            modelBuilder.Entity<Series>()
                .HasOne(s => s.Brand)
                .WithMany(b => b.Series)
                .HasForeignKey(s => s.BrandId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================
            // Brand → Products
            // =========================

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================
            // Scale → Products
            // =========================

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Scale)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.ScaleId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================
            // Category → Products
            // =========================

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================
            // Series → Products
            // =========================

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Series)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SeriesId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}