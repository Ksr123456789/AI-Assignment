using RentalManagementSystem.Domain.Enums;

namespace RentalManagementSystem.Domain.Entities
{
    public class Booking
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
        public decimal AdditionalServiceAmount { get; set; } = 0;
        public decimal TotalAmount { get; set; }
        public DateTime BookedOn { get; set; }
        public Vehicle Vehicle { get; set; } = null!;
        public int VehicleId { get; set; }
        public ApplicationUser Customer { get; set; } = null!;
        public Guid CustomerId { get; set; }
        public ICollection<BookingExtraService> BookingExtraServices { get; set; } = new List<BookingExtraService>();
    }
}
