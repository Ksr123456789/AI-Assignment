using MediatR;

namespace RentalManagementSystem.Application.Features.Booking.Commands.BookVehicle
{
    public class BookVehicleCommand : IRequest<BookVehicleCommandResponse>
    {
        public DateTime PickUpDateTime { get; set; }
        public DateTime ReturnDateTime { get; set; }
        public string PickupLocation { get; set; } = string.Empty;
        public string ReturnLocation { get; set; } = string.Empty;
        public int VehicleId { get; set; }
        public bool Insurance { get; set; }
        public List<int> ExtraServiceIds { get; set; } = new();
    }
}
