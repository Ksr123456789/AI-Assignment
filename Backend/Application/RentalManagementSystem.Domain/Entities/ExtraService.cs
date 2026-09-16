namespace RentalManagementSystem.Domain.Entities
{
    public class ExtraService
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public ICollection<BookingExtraService> BookingExtraServices { get; set; } = new List<BookingExtraService>();
    }
}
