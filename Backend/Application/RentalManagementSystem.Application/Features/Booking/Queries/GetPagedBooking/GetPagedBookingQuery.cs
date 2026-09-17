using MediatR;
using RentalManagementSystem.Domain.Common;
using RentalManagementSystem.Domain.Enums;

namespace RentalManagementSystem.Application.Features.Booking.Queries.GetPagedBooking
{
    public class GetPagedBookingQuery : IRequest<PagedResult<GetPagedBookingQueryResponse>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "asc";
        public string? SearchBy { get; set; }
        public BookingStatus? BookingStatus { get; set; }
        public int? RentalCompanyId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
