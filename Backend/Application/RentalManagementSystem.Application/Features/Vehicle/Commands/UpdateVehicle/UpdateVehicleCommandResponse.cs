using RentalManagementSystem.Application.Features.Vehicle.DTOs;

namespace RentalManagementSystem.Application.Features.Vehicle.Commands.UpdateVehicle
{
    public class UpdateVehicleCommandResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public VehicleDto? Data { get; set; }
        public IEnumerable<string> Errors { get; set; } = new List<string>();
    }
}
