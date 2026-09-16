namespace RentalManagementSystem.Domain.Entities
{
    public class BookingExtraService
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public Booking Booking { get; set; } = null!;
        public int BookingId { get; set; }
        public ExtraService ExtraService { get; set; } = null!;
        public int ExtraServiceId { get; set; }
    }
}
