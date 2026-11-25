using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;
namespace Interview.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
   protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL("server=localhost;database=Interview;user=root;password=root");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();

            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired();
            });

            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.Id);
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