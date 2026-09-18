using RentalManagementSystem.Domain.Common;
using RentalManagementSystem.Domain.Entities;

namespace RentalManagementSystem.Domain.RepositoryContracts
{
    public interface IVehicleRepository
    {
        Task<Vehicle> AddVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken = default);
        Task<bool> ExistsByLicensePlateAsync(string licensePlate, CancellationToken cancellationToken = default);
        Task<bool> ExistsByRegistrationNumberAsync(string registrationNumber, CancellationToken cancellationToken = default);
        Task<bool> RentalCompanyExistsAsync(Guid rentalCompanyId, CancellationToken cancellationToken = default);
        Task<bool> VehicleCategoryExistsAsync(int vehicleCategoryId, CancellationToken cancellationToken = default);
        Task<Vehicle?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Vehicle?> GetVehicleById(int id, CancellationToken cancellationToken = default);
        Task UpdateAsync(Vehicle vehicle, CancellationToken cancellationToken = default);
        Task<PagedResult<Vehicle>> GetPagedVehicleAsync(VehicleFilterParameters query);
        Task<bool> HasOngoingBookingsAsync(int vehicleId, CancellationToken cancellationToken = default);
        Task DeleteVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken = default);
    }
}
