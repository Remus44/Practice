using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;
namespace Interview.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();

        public DbSet<Role> Roles => Set<Role>();

        public DbSet<Permission> Permissions => Set<Permission>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(50);

            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Code).IsRequired();
            });

            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Code).IsRequired();
            });

            modelBuilder.Entity<User>()
                    .HasMany(u => u.Roles)
                    .WithMany()
                    .UsingEntity(j => j.ToTable("UserRole"));

            modelBuilder.Entity<Role>()
                    .HasMany(r => r.Permissions)
                    .WithMany()
                    .UsingEntity(j => j.ToTable("RolePermission"));
        }
    }
}