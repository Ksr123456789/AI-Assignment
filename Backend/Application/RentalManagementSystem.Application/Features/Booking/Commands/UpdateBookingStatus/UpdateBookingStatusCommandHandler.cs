using MediatR;
using RentalManagementSystem.Application.ServiceContracts;

namespace RentalManagementSystem.Application.Features.Booking.Commands.UpdateBookingStatus
{
    public class UpdateBookingStatusCommandHandler(IBookingService bookingService)
        : IRequestHandler<UpdateBookingStatusCommand, UpdateBookingStatusCommandResponse>
    {
        public async Task<UpdateBookingStatusCommandResponse> Handle(
            UpdateBookingStatusCommand request,
            CancellationToken cancellationToken)
        {
            return await bookingService.UpdateBookingStatusAsync(request, cancellationToken);
        }
    }
}
