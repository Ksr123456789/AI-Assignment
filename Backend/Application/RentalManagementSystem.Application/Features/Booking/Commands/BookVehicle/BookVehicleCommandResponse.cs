using RentalManagementSystem.Application.Features.Booking.DTOs;

namespace RentalManagementSystem.Application.Features.Booking.Commands.BookVehicle
{
    public class BookVehicleCommandResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public BookingDto? Data { get; set; }
        public IEnumerable<string> Errors { get; set; } = new List<string>();
    }
}
