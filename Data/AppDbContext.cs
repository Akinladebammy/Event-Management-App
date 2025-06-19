using EventManagementApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
namespace EventManagementApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventBooking> EventBookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Event
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Location).IsRequired().HasMaxLength(300);
                entity.Ignore(e => e.Bookings); // Ignore the ICollection<string> property
            });

            // Configure User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
            });

            // Configure EventBooking
            modelBuilder.Entity<EventBooking>(entity =>
            {
                entity.HasKey(eb => eb.Id);
                entity.HasOne(eb => eb.Event)
                      .WithMany()
                      .HasForeignKey(eb => eb.EventId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(eb => eb.User)
                      .WithMany(u => u.Bookings)
                      .HasForeignKey(eb => eb.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
