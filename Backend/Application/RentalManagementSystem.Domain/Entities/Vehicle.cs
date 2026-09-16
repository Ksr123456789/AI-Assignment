using System.ComponentModel.DataAnnotations;
using RentalManagementSystem.Domain.Enums;

namespace RentalManagementSystem.Domain.Entities
{
    public class Vehicle
    {
        public int Id { get; set; }

        public Guid RentalCompanyId { get; set; }
        public RentalCompany RentalCompany { get; set; } = null!;

        public int VehicleCategoryId { get; set; }
        public VehicleCategory VehicleCategory { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string MakeAndModel { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string LicensePlate { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required]
        public int YearOfManufacture { get; set; }

        [Required]
        [Range(1, 20, ErrorMessage = "Seating Capacity must be between 1 and 20.")]
        public int SeatingCapacity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Daily Rental Rate must be greater than 0.")]
        public decimal DailyRentalRate { get; set; }

        [Required]
        public VehicleAvailabilityStatus AvailabilityStatus { get; set; }

        [Required]
        public double Mileage { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
