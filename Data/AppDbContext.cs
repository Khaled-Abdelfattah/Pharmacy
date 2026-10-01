using Microsoft.EntityFrameworkCore;
using PharmacySystem.Models.Entities;

namespace PharmacySystem.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<ReorderItem> ReorderItems => Set<ReorderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Category ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Category>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).HasMaxLength(100).IsRequired();
            // Seed the 3 fixed categories
            e.HasData(
                new Category { Id = 1, Name = "Available Medicines", SortOrder = 1 },
                new Category { Id = 2, Name = "Deficient Medicines", SortOrder = 2 },
                new Category { Id = 3, Name = "Personal Care Products", SortOrder = 3 }
            );
        });

        // ── Product ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).HasMaxLength(200).IsRequired();
            e.Property(p => p.Barcode).HasMaxLength(100);
            e.Property(p => p.Notes).HasMaxLength(500);
            e.Property(p => p.PurchasePrice).HasColumnType("decimal(18,2)");
            e.Property(p => p.SellingPrice).HasColumnType("decimal(18,2)");
            // Unique barcode — only enforced when barcode is provided (filtered index)
            e.HasIndex(p => p.Barcode).IsUnique().HasFilter("[Barcode] IS NOT NULL AND [Barcode] != ''");
            // Global query filter: hide soft-deleted products from all queries
            e.HasQueryFilter(p => !p.IsDeleted);
            e.HasOne(p => p.Category)
             .WithMany(c => c.Products)
             .HasForeignKey(p => p.CategoryId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Sale ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<Sale>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.TotalAmount).HasColumnType("decimal(18,2)");
            e.Property(s => s.Notes).HasMaxLength(500);
        });

        // ── SaleItem ──────────────────────────────────────────────────────────
        modelBuilder.Entity<SaleItem>(e =>
        {
            e.HasKey(si => si.Id);
            e.Property(si => si.ProductName).HasMaxLength(200).IsRequired();
            e.Property(si => si.UnitPrice).HasColumnType("decimal(18,2)");
            e.Property(si => si.LineTotal).HasColumnType("decimal(18,2)");
            e.HasOne(si => si.Sale)
             .WithMany(s => s.SaleItems)
             .HasForeignKey(si => si.SaleId)
             .OnDelete(DeleteBehavior.Cascade);
            // Allow product to be soft-deleted while keeping sale history
            e.HasOne(si => si.Product)
             .WithMany(p => p.SaleItems)
             .HasForeignKey(si => si.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Order ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.SupplierName).HasMaxLength(200).IsRequired();
            e.Property(o => o.Notes).HasMaxLength(500);
            e.Property(o => o.Status).HasConversion<int>();
        });

        // ── OrderItem ─────────────────────────────────────────────────────────
        modelBuilder.Entity<OrderItem>(e =>
        {
            e.HasKey(oi => oi.Id);
            e.HasOne(oi => oi.Order)
             .WithMany(o => o.OrderItems)
             .HasForeignKey(oi => oi.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(oi => oi.Product)
             .WithMany(p => p.OrderItems)
             .HasForeignKey(oi => oi.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── ReorderItem ───────────────────────────────────────────────────────
        modelBuilder.Entity<ReorderItem>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasIndex(r => r.ProductId).IsUnique(); // One reorder entry per product
            e.HasOne(r => r.Product)
             .WithOne(p => p.ReorderItem)
             .HasForeignKey<ReorderItem>(r => r.ProductId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
