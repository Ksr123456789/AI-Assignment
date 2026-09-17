using System.ComponentModel.DataAnnotations;
using MediatR;
using RentalManagementSystem.Domain.Enums;

namespace RentalManagementSystem.Application.Features.Vehicle.Commands.UpdateVehicle
{
    public class UpdateVehicleCommand : IRequest<UpdateVehicleCommandResponse>
    {
        [Required(ErrorMessage = "Vehicle ID is required.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Rental company ID is required.")]
        public Guid RentalCompanyId { get; set; }

        [Required(ErrorMessage = "Vehicle category ID is required.")]
        public int VehicleCategoryId { get; set; }

        [Required(ErrorMessage = "Make and model is required.")]
        [MaxLength(100, ErrorMessage = "Make and model cannot exceed 100 characters.")]
        public string MakeAndModel { get; set; } = string.Empty;

        [Required(ErrorMessage = "License plate is required.")]
        [MaxLength(20, ErrorMessage = "License plate cannot exceed 20 characters.")]
        public string LicensePlate { get; set; } = string.Empty;

        [Required(ErrorMessage = "Registration number is required.")]
        [MaxLength(50, ErrorMessage = "Registration number cannot exceed 50 characters.")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Year of manufacture is required.")]
        [Range(1900, 2100, ErrorMessage = "Year of manufacture must be between 1900 and 2100.")]
        public int YearOfManufacture { get; set; }

        [Required(ErrorMessage = "Seating capacity is required.")]
        [Range(1, 20, ErrorMessage = "Seating capacity must be between 1 and 20.")]
        public int SeatingCapacity { get; set; }

        [Required(ErrorMessage = "Daily rental rate is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Daily rental rate must be greater than 0.")]
        public decimal DailyRentalRate { get; set; }

        public VehicleAvailabilityStatus AvailabilityStatus { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Mileage must be non-negative.")]
        public double Mileage { get; set; }
    }
}
