using MediatR;
using RentalManagementSystem.Domain.Common;
using RentalManagementSystem.Domain.Enums;

namespace RentalManagementSystem.Application.Features.Booking.Queries.MyPagedRentals
{
    public class MyPagedRentalsQuery : IRequest<PagedResult<MyPagedRentalsQueryResponse>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "asc";
        public string? SearchBy { get; set; }
        public BookingStatus? BookingStatus { get; set; }
        public int? SeatingCapacity { get; set; }
    }
}
