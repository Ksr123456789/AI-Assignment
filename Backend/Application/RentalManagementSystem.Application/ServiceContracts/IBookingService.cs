using RentalManagementSystem.Application.Features.Booking.Commands.BookVehicle;
using RentalManagementSystem.Application.Features.Booking.Commands.UpdateBookingStatus;
using RentalManagementSystem.Application.Features.Booking.Queries.GetPagedBooking;
using RentalManagementSystem.Application.Features.Booking.Queries.MyPagedRentals;
using RentalManagementSystem.Domain.Common;

namespace RentalManagementSystem.Application.ServiceContracts
{
    public interface IBookingService
    {
        Task<PagedResult<GetPagedBookingQueryResponse>> GetPagedBooking(GetPagedBookingQuery query);
        Task<UpdateBookingStatusCommandResponse> UpdateBookingStatusAsync(UpdateBookingStatusCommand command, CancellationToken cancellationToken = default);
        Task<BookVehicleCommandResponse> BookVehicle(BookVehicleCommand command, CancellationToken cancellationToken = default);
        Task<PagedResult<MyPagedRentalsQueryResponse>> MyPagedRentals(MyPagedRentalsQuery query, CancellationToken cancellationToken = default);
    }
}
