using RentalManagementSystem.Domain.Enums;

namespace RentalManagementSystem.Domain.Common
{
    public class MyRentalFilterParameters
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "asc";
        public string? SearchBy { get; set; }
        public BookingStatus? BookingStatus { get; set; }
        public int? SeatingCapacity { get; set; }
        public Guid CustomerId { get; set; }
    }
}
