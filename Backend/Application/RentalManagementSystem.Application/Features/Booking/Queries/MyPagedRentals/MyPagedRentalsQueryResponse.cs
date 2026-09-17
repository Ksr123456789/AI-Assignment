using RentalManagementSystem.Domain.Enums;

namespace RentalManagementSystem.Application.Features.Booking.Queries.MyPagedRentals
{
    public class MyPagedRentalsQueryResponse
    {
        public int Id { get; set; }
        public string MakeModal { get; set; } = string.Empty;
        public int SeatingCapacity { get; set; }
        public DateTime PickupDateTime { get; set; }
        public DateTime ReturnDateTime { get; set; }
        public BookingStatus BookingStatus { get; set; }
        public decimal AdditionalServiceAmount { get; set; }
        public decimal RentalAmount { get; set; }
        public int TotalDays { get; set; }
        public decimal TotalAmount { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public int YearofManufacture { get; set; }
        public decimal DailyRentalRate { get; set; }
        public double Mileage { get; set; }
    }
}
