using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Domain.Entities;

namespace RentalManagementSystem.Infrastructure.DbContext
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<RentalCompany> RentalCompanies { get; set; } = null!;
        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<VehicleCategory> VehicleCategories { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;
        public DbSet<ExtraService> ExtraServices { get; set; } = null!;
        public DbSet<BookingExtraService> BookingExtraServices { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Decimal Precisions
            builder.Entity<Vehicle>()
                .Property(v => v.DailyRentalRate)
                .HasPrecision(18, 2);

            builder.Entity<Booking>()
                .Property(b => b.RentalAmount)
                .HasPrecision(18, 2);

            builder.Entity<Booking>()
                .Property(b => b.AdditionalServiceAmount)
                .HasPrecision(18, 2);

            builder.Entity<Booking>()
                .Property(b => b.TotalAmount)
                .HasPrecision(18, 2);

            builder.Entity<ExtraService>()
                .Property(es => es.Price)
                .HasPrecision(18, 2);

            builder.Entity<BookingExtraService>()
                .Property(bes => bes.Price)
                .HasPrecision(18, 2);

            // Unique Constraints
            builder.Entity<RentalCompany>()
                .HasIndex(rc => rc.LicenseNumber)
                .IsUnique();

            builder.Entity<Vehicle>()
                .HasIndex(v => v.LicensePlate)
                .IsUnique();

            builder.Entity<Vehicle>()
                .HasIndex(v => v.RegistrationNumber)
                .IsUnique();

            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.DrivingLicenseNumber)
                .IsUnique();

            // Relationships
            builder.Entity<Booking>()
                .HasOne(b => b.Customer)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasOne(b => b.Vehicle)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BookingExtraService>()
                .HasOne(bes => bes.Booking)
                .WithMany(b => b.BookingExtraServices)
                .HasForeignKey(bes => bes.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<BookingExtraService>()
                .HasOne(bes => bes.ExtraService)
                .WithMany(es => es.BookingExtraServices)
                .HasForeignKey(bes => bes.ExtraServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
