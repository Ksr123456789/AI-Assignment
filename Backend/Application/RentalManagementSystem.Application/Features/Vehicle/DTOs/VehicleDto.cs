using RentalManagementSystem.Domain.Enums;

namespace RentalManagementSystem.Application.Features.Vehicle.DTOs
{
    public class VehicleDto
    {
        public int Id { get; set; }
        public Guid RentalCompanyId { get; set; }
        public string RentalCompanyName { get; set; } = string.Empty;
        public int VehicleCategoryId { get; set; }
        public string VehicleCategoryName { get; set; } = string.Empty;
        public string MakeAndModel { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public int YearOfManufacture { get; set; }
        public int SeatingCapacity { get; set; }
        public decimal DailyRentalRate { get; set; }
        public VehicleAvailabilityStatus AvailabilityStatus { get; set; }
        public double Mileage { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
