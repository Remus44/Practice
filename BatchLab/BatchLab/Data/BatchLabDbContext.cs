using BatchLab.Models;
using Microsoft.EntityFrameworkCore;

namespace BatchLab.Data;

public class BatchLabDbContext : DbContext
{
    public BatchLabDbContext(
        DbContextOptions<BatchLabDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Sku)
                .HasMaxLength(64)
                .IsRequired();

            entity.HasIndex(p => p.Sku)
                .IsUnique();

            entity.Property(p => p.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(p => p.Price)
                .HasPrecision(18, 2);

            entity.Property(p => p.Version)
                .IsConcurrencyToken()
                .HasDefaultValue(0L);
        });

        modelBuilder.Entity<StockMovement>()
                .HasOne(sm => sm.Product)
                .WithMany(p => p.StockMovements)
                .HasForeignKey(sm => sm.ProductId);
    }
}