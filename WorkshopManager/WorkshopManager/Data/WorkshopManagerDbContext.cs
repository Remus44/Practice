using Microsoft.EntityFrameworkCore;
using WorkshopManager.Models;

namespace WorkshopManager.Data
{
    public class WorkshopManagerDbContext : DbContext
    {
        public WorkshopManagerDbContext(
        DbContextOptions<WorkshopManagerDbContext> options)
        : base(options)
        {
        }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<PartUsage> Parts => Set<PartUsage>();
        public DbSet<ServiceJob> ServiceJobs => Set<ServiceJob>();
        public DbSet<Vehicle> Vehicles => Set<Vehicle>();

        protected override void OnModelCreating(
        ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>(
                customer =>
                {
                    customer.HasKey(c => c.Id);
                    customer.Property(c => c.Name)
                        .HasMaxLength(64)
                        .IsRequired();
                    customer.Property(c => c.Email)
                        .HasMaxLength(64)
                        .IsRequired();
                    modelBuilder.Entity<Vehicle>()
                        .HasOne(v => v.Customer)
                        .WithMany(c => c.Vehicles)
                        .HasForeignKey(v => v.Id);
                }
             );

            modelBuilder.Entity<Vehicle>(
                vehicle =>
                {
                    vehicle.HasKey(v => v.Id);
                    vehicle.Property(v => v.LicencePlate)
                        .HasMaxLength(12)
                        .IsRequired();
                    vehicle.Property(v => v.Brand)
                        .HasMaxLength(24)
                        .IsRequired();
                    vehicle.Property(v => v.Model)
                        .HasMaxLength(24)
                        .IsRequired();
                    modelBuilder.Entity<ServiceJob>()
                        .HasOne(s => s.Vehicle)
                        .WithMany(v => v.ServiceJobs)
                        .HasForeignKey(s => s.Id);
                }
             );

            modelBuilder.Entity<PartUsage>(
                vehicle =>
                {
                    vehicle.HasKey(p => p.Id);
                    vehicle.Property(v => v.Name)
                        .HasMaxLength(64)
                        .IsRequired();
                    vehicle.Property(v => v.Quantity)
                        .HasMaxLength(24)
                        .IsRequired();
                    vehicle.Property(v => v.UnitPrice)
                        .HasMaxLength(24)
                        .HasPrecision(18, 2);
                    modelBuilder.Entity<ServiceJob>()
                        .HasOne(s => s.Vehicle)
                        .WithMany(v => v.ServiceJobs)
                        .HasForeignKey(s => s.Id);
                }
             );

        }
    }
}
