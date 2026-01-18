using Microsoft.EntityFrameworkCore;
using BerberRandevuSistemi.Models;

namespace BerberRandevuSistemi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Barber> Barbers { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Barber entity
        modelBuilder.Entity<Barber>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
        });

        // Configure Service entity
        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ServiceName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasData(
                new Service { Id = 1, ServiceName = "Saç Kesimi", Price = 200, DurationMinutes = 30, Description = "Klasik veya modern saç kesimi" },
                new Service { Id = 2, ServiceName = "Sakal Tıraşı", Price = 100, DurationMinutes = 15, Description = "Sıcak havlu ve jiletli tıraş" },
                new Service { Id = 3, ServiceName = "Saç & Sakal", Price = 280, DurationMinutes = 45, Description = "Komple saç ve sakal bakımı" },
                new Service { Id = 4, ServiceName = "Çocuk Tıraşı", Price = 150, DurationMinutes = 25, Description = "0-12 yaş özel kesim" },
                new Service { Id = 5, ServiceName = "Yıkama ve Fön", Price = 80, DurationMinutes = 15, Description = "Yıkama ve şekillendirme" },
                new Service { Id = 6, ServiceName = "Cilt Bakımı", Price = 120, DurationMinutes = 20, Description = "Siyah maske ve buhar" },
                new Service { Id = 7, ServiceName = "Damat Tıraşı", Price = 750, DurationMinutes = 75, Description = "VIP özel gün paketi" }
            );
        });

        // Configure Appointment entity
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CustomerPhone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");

            // Configure foreign keys
            entity.HasOne(e => e.Barber)
                  .WithMany(b => b.Appointments)
                  .HasForeignKey(e => e.BarberId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Service)
                  .WithMany(s => s.Appointments)
                  .HasForeignKey(e => e.ServiceId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
