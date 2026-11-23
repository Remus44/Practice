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

            // Customer table definition based on model
            modelBuilder.Entity<Customer>(
                customer =>
                {
                    customer.HasKey(c => c.Id);
                    customer.Property(c => c.Name)
                        .HasMaxLength(100)
                        .IsRequired();
                    customer.HasIndex(c => c.Email).IsUnique();
                    customer.Property(c => c.Phone).HasMaxLength(30);
                }
             );

            // Vehicle table definition based on model
            modelBuilder.Entity<Vehicle>(
                vehicle =>
                {
                    vehicle.HasKey(v => v.Id);
                    vehicle.HasIndex(v => v.LicencePlate)
                        .IsUnique();
                    vehicle.Property(v => v.Brand)
                        .HasMaxLength(50)
                        .IsRequired();
                    vehicle.Property(v => v.Model)
                        .HasMaxLength(50)
                        .IsRequired();
                }
             );

            // ServiceJob table definition based on model
            modelBuilder.Entity<ServiceJob>(
                serviceJob =>
                {
                    serviceJob.HasKey(v => v.Id);
                    serviceJob.Property(v => v.Description)
                        .HasMaxLength(500)
                        .IsRequired();
                    serviceJob.Property(v => v.Status)
                    .HasConversion<int>()
                    .IsRequired();
                    serviceJob.Property(v => v.LaborCost)
                        .HasPrecision(18, 2);
                }
             );

            // PartUsage table definition based on model
            modelBuilder.Entity<PartUsage>(
                part =>
                {
                    part.HasKey(p => p.Id);
                    part.Property(p => p.PartName)
                        .HasMaxLength(100)
                        .IsRequired();
                    part.Property(p => p.Quantity);
                    part.Property(p => p.UnitPrice)
                        .HasPrecision(18, 2);
                }
             );


            // Customer - Vehicle relation definition
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Customer)
                .WithMany(c => c.Vehicles)
                .HasForeignKey(c => c.CustomerId);

            // Vehicle - ServiceJob relation definition
            modelBuilder.Entity<ServiceJob>()
                .HasOne(s => s.Vehicle)
                .WithMany(v => v.ServiceJobs)
                .HasForeignKey(s => s.VehicleId);

            // ServiceJob - PartsUsage relation definition
            modelBuilder.Entity<PartUsage>()
                .HasOne(p => p.ServiceJob)
                .WithMany(s => s.PartUsages)
                .HasForeignKey(p => p.ServiceJobId);
        }
    }
}
