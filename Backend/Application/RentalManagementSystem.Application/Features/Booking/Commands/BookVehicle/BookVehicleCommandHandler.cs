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
            try
            {
                return await bookingService.BookVehicle(request, cancellationToken);
            }
            catch (Exception ex)
            {
                return new BookVehicleCommandResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = [ex.Message]
                };
            }
        }
    }
}
