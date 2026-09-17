using MediatR;
using RentalManagementSystem.Application.ServiceContracts;
using RentalManagementSystem.Domain.Common;

namespace RentalManagementSystem.Application.Features.Booking.Queries.MyPagedRentals
{
    public class MyPagedRentalsQueryHandler(IBookingService bookingService) 
        : IRequestHandler<MyPagedRentalsQuery, PagedResult<MyPagedRentalsQueryResponse>>
    {
        public async Task<PagedResult<MyPagedRentalsQueryResponse>> Handle(
            MyPagedRentalsQuery request, 
            CancellationToken cancellationToken)
        {
            return await bookingService.MyPagedRentals(request, cancellationToken);
        }
    }
}
