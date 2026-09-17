using RentalManagementSystem.Domain.Common;
using RentalManagementSystem.Domain.Entities;

namespace RentalManagementSystem.Domain.RepositoryContracts
{
    public interface IBookingRepository
    {
        Task<PagedResult<Booking>> GetPagedBookingAsync(BookingFilterParameters query);
        Task<Booking?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task UpdateAsync(Booking booking, CancellationToken cancellationToken = default);
        Task<List<Booking>> GetCustomerBookings(Guid customerId, CancellationToken cancellationToken = default);
        Task<List<Booking>> GetBookingsByVehicleId(int vehicleId, CancellationToken cancellationToken = default);
        Task BookVehicleAsync(Booking newBooking, List<int> extraServiceIds, CancellationToken cancellationToken = default);
        Task<PagedResult<Booking>> MyPagedRentalsAsync(MyRentalFilterParameters query, CancellationToken cancellationToken = default);
    }
}
