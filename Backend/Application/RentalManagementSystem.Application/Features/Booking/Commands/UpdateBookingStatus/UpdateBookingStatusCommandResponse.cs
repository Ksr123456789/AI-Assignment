using RentalManagementSystem.Application.Features.Booking.DTOs;

namespace RentalManagementSystem.Application.Features.Booking.Commands.UpdateBookingStatus
{
    public class UpdateBookingStatusCommandResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public BookingDto? Data { get; set; }
        public decimal? ForfeitedAmount { get; set; }
        public decimal? RefundAmount { get; set; }
        public IEnumerable<string> Errors { get; set; } = new List<string>();
    }
}
