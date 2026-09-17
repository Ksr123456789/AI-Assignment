using MediatR;
using RentalManagementSystem.Application.ServiceContracts;
using RentalManagementSystem.Domain.Common;

namespace RentalManagementSystem.Application.Features.Booking.Queries.GetPagedBooking
{
    public class GetPagedBookingQueryHandler(IBookingService bookingService)
        : IRequestHandler<GetPagedBookingQuery, PagedResult<GetPagedBookingQueryResponse>>
    {
        public async Task<PagedResult<GetPagedBookingQueryResponse>> Handle(
            GetPagedBookingQuery request,
            CancellationToken cancellationToken)
        {
            return await bookingService.GetPagedBooking(request);
        }
    }
}
