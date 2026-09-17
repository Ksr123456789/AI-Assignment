using System.ComponentModel.DataAnnotations;
using MediatR;
using RentalManagementSystem.Domain.Enums;

namespace RentalManagementSystem.Application.Features.Vehicle.Commands.AddVehicle
{
    public class AddVehicleCommand : IRequest<AddVehicleCommandResponse>
    {
        public Guid RentalCompanyId { get; set; }
        public int VehicleCategoryId { get; set; }
        public string MakeAndModel { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public int YearOfManufacture { get; set; }

        [Required(ErrorMessage = "Seating capacity is required.")]
        [Range(1, 20, ErrorMessage = "Seating capacity must be between 1 and 20.")]
        public int SeatingCapacity { get; set; }

        [Required(ErrorMessage = "Daily rental rate is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Daily rental rate must be greater than 0.")]
        public decimal DailyRentalRate { get; set; }

        public VehicleAvailabilityStatus AvailabilityStatus { get; set; } = VehicleAvailabilityStatus.Available;

        [Range(0, double.MaxValue, ErrorMessage = "Mileage must be non-negative.")]
        public double Mileage { get; set; }
    }
}
