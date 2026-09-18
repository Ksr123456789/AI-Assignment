using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Domain.Common;
using RentalManagementSystem.Domain.Entities;
using RentalManagementSystem.Domain.Enums;
using RentalManagementSystem.Domain.RepositoryContracts;
using RentalManagementSystem.Infrastructure.DbContext;

namespace RentalManagementSystem.Infrastructure.Repository
{
    public class VehicleRepository(AppDbContext context) : IVehicleRepository
    {
        public async Task<Vehicle> AddVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
        {
            await context.Vehicles.AddAsync(vehicle, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return vehicle;
        }

        public async Task<bool> ExistsByLicensePlateAsync(string licensePlate, CancellationToken cancellationToken = default)
        {
            return await context.Vehicles.AnyAsync(
                v => v.LicensePlate.ToLower() == licensePlate.ToLower() && !v.IsDeleted,
                cancellationToken);
        }

        public async Task<bool> ExistsByRegistrationNumberAsync(string registrationNumber, CancellationToken cancellationToken = default)
        {
            return await context.Vehicles.AnyAsync(
                v => v.RegistrationNumber.ToLower() == registrationNumber.ToLower() && !v.IsDeleted,
                cancellationToken);
        }

        public async Task<bool> RentalCompanyExistsAsync(Guid rentalCompanyId, CancellationToken cancellationToken = default)
        {
            return await context.RentalCompanies.AnyAsync(
                rc => rc.Id == rentalCompanyId && !rc.IsDeleted,
                cancellationToken);
        }

        public async Task<bool> VehicleCategoryExistsAsync(int vehicleCategoryId, CancellationToken cancellationToken = default)
        {
            return await context.VehicleCategories.AnyAsync(
                vc => vc.Id == vehicleCategoryId,
                cancellationToken);
        }

        public async Task<Vehicle?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await context.Vehicles
                .Include(v => v.RentalCompany)
                .Include(v => v.VehicleCategory)
                .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted, cancellationToken);
        }

        public async Task<Vehicle?> GetVehicleById(int id, CancellationToken cancellationToken = default)
        {
            return await GetByIdAsync(id, cancellationToken);
        }

        public async Task UpdateAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
        {
            context.Vehicles.Update(vehicle);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<PagedResult<Vehicle>> GetPagedVehicleAsync(VehicleFilterParameters query)
        {
            var vehicles = context.Vehicles
                .Include(v => v.RentalCompany)
                .Include(v => v.VehicleCategory)
                .Include(v => v.Bookings)
                .Where(x => !x.IsDeleted);

            if (query.VehicleCategoryId.HasValue)
            {
                vehicles = vehicles.Where(x => x.VehicleCategoryId == query.VehicleCategoryId.Value);
            }

            if (query.VehicleAvailabilityStatus.HasValue)
            {
                vehicles = vehicles.Where(x => x.AvailabilityStatus == query.VehicleAvailabilityStatus.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchBy))
            {
                var search = query.SearchBy.Trim();
                vehicles = vehicles.Where(x => x.MakeAndModel.Contains(search) ||
                                               x.LicensePlate.Contains(search) ||
                                               x.RegistrationNumber.Contains(search) ||
                                               x.RentalCompany.CompanyName.Contains(search) ||
                                               x.VehicleCategory.CategoryName.Contains(search));
            }

            vehicles = query.SortBy?.ToLower() switch
            {
                "id" => query.SortDirection == "desc" ? vehicles.OrderByDescending(x => x.Id) : vehicles.OrderBy(x => x.Id),
                "makeandmodel" or "make" or "model" => query.SortDirection == "desc" ? vehicles.OrderByDescending(x => x.MakeAndModel) : vehicles.OrderBy(x => x.MakeAndModel),
                "licenseplate" => query.SortDirection == "desc" ? vehicles.OrderByDescending(x => x.LicensePlate) : vehicles.OrderBy(x => x.LicensePlate),
                "registrationnumber" => query.SortDirection == "desc" ? vehicles.OrderByDescending(x => x.RegistrationNumber) : vehicles.OrderBy(x => x.RegistrationNumber),
                "yearofmanufacture" or "year" => query.SortDirection == "desc" ? vehicles.OrderByDescending(x => x.YearOfManufacture) : vehicles.OrderBy(x => x.YearOfManufacture),
                "seatingcapacity" => query.SortDirection == "desc" ? vehicles.OrderByDescending(x => x.SeatingCapacity) : vehicles.OrderBy(x => x.SeatingCapacity),
                "dailyrentalrate" or "rate" => query.SortDirection == "desc" ? vehicles.OrderByDescending(x => x.DailyRentalRate) : vehicles.OrderBy(x => x.DailyRentalRate),
                "availabilitystatus" or "status" => query.SortDirection == "desc" ? vehicles.OrderByDescending(x => x.AvailabilityStatus.ToString()) : vehicles.OrderBy(x => x.AvailabilityStatus.ToString()),
                "mileage" => query.SortDirection == "desc" ? vehicles.OrderByDescending(x => x.Mileage) : vehicles.OrderBy(x => x.Mileage),
                "rentalcompany" or "rentalcompanyname" => query.SortDirection == "desc" ? vehicles.OrderByDescending(x => x.RentalCompany.CompanyName) : vehicles.OrderBy(x => x.RentalCompany.CompanyName),
                "category" or "categoryname" => query.SortDirection == "desc" ? vehicles.OrderByDescending(x => x.VehicleCategory.CategoryName) : vehicles.OrderBy(x => x.VehicleCategory.CategoryName),
                _ => vehicles.OrderByDescending(x => x.Id)
            };

            var totalCount = await vehicles.CountAsync();

            var items = await vehicles
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<Vehicle>
            {
                Items = items,
                TotalCount = totalCount,
                PageSize = query.PageSize,
                PageNumber = query.PageNumber,
            };
        }

        public async Task<bool> HasOngoingBookingsAsync(int vehicleId, CancellationToken cancellationToken = default)
        {
            return await context.Bookings.AnyAsync(
                b => b.VehicleId == vehicleId && 
                     (b.BookingStatus == BookingStatus.Pending || 
                      b.BookingStatus == BookingStatus.Confirmed || 
                      b.BookingStatus == BookingStatus.Active),
                cancellationToken);
        }

        public async Task DeleteVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
        {
            vehicle.IsDeleted = true;
            context.Vehicles.Update(vehicle);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
