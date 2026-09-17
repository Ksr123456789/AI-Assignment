using MediatR;
using RentalManagementSystem.Application.ServiceContracts;

namespace RentalManagementSystem.Application.Features.Booking.Commands.BookVehicle
{
    public class BookVehicleCommandHandler(IBookingService bookingService)
        : IRequestHandler<BookVehicleCommand, BookVehicleCommandResponse>
    {
        public async Task<BookVehicleCommandResponse> Handle(
            BookVehicleCommand request,
            CancellationToken cancellationToken)
        {
            return await bookingService.BookVehicle(request, cancellationToken);
        }
    }
}
