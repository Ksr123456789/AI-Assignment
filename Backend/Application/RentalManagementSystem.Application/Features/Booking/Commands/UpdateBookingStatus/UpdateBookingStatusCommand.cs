using System.ComponentModel.DataAnnotations;
using MediatR;
using RentalManagementSystem.Domain.Enums;

namespace RentalManagementSystem.Application.Features.Booking.Commands.UpdateBookingStatus
{
    public class UpdateBookingStatusCommand : IRequest<UpdateBookingStatusCommandResponse>
    {
        public int BookingId { get; set; }
        public BookingStatus Status { get; set; }

    }
}
