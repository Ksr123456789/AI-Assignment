using RentalManagementSystem.Domain.Enums;

namespace RentalManagementSystem.Application.Features.Booking.Queries.GetPagedBooking
{
    public class GetPagedBookingQueryResponse
    {
        public int Id { get; set; }
        public DateTime PickupDateTime { get; set; }
        public DateTime ReturnDateTime { get; set; }
        public string PickupLocation { get; set; } = string.Empty;
        public string ReturnLocation { get; set; } = string.Empty;
        public bool Insurance { get; set; }
        public BookingStatus BookingStatus { get; set; }
        public int TotalDays { get; set; }
        public decimal RentalAmount { get; set; }
        public decimal AdditionalServiceAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime BookedOn { get; set; }

        public int VehicleId { get; set; }
        public string VehicleMakeAndModel { get; set; } = string.Empty;
        public string VehicleLicensePlate { get; set; } = string.Empty;

        public Guid RentalCompanyId { get; set; }
        public string RentalCompanyName { get; set; } = string.Empty;

        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
    }
}
