namespace RentalManagementSystem.Domain.Entities
{
    public class VehicleCategory
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}
