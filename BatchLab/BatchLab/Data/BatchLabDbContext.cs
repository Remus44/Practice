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

    public DbSet<JobExecution> JobExecutions => Set<JobExecution>();

    public DbSet<ProcessedChunk> ProcessedChunks => Set<ProcessedChunk>();

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

        modelBuilder.Entity<Product>()
                .HasIndex(p => new
                {
                    p.StockQuantity,
                    p.Price
                })
                .HasDatabaseName("IX_Products_StockQuantity_Price");

        modelBuilder.Entity<Product>()
                .HasIndex(p => p.Price)
                .HasDatabaseName("IX_Products_Price");

        modelBuilder.Entity<Product>()
                .HasIndex(p => p.Price)
                .HasDatabaseName("IX_Products_Price");

        modelBuilder.Entity<StockMovement>()
                .HasOne(sm => sm.Product)
                .WithMany(p => p.StockMovements)
                .HasForeignKey(sm => sm.ProductId);

        modelBuilder.Entity<JobExecution>(entity =>
        {
            entity.HasKey(j => j.OperationId);

            entity.Property(j => j.OperationType)
                .HasMaxLength(40)
                .IsRequired();

            entity.Property(j => j.Parameters)
                .HasMaxLength(200)
                .IsRequired();
        });

        modelBuilder.Entity<ProcessedChunk>(entity =>
        {
            entity.HasKey(x => new
            {
                x.OperationId,
                x.ChunkIndex
            });

            entity.Property(x => x.Signature)
                .HasMaxLength(100)
                .IsRequired();
        });
    }
}