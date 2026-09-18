using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Domain.Common;
using RentalManagementSystem.Domain.Entities;
using RentalManagementSystem.Domain.RepositoryContracts;
using RentalManagementSystem.Infrastructure.DbContext;

namespace RentalManagementSystem.Infrastructure.Repository
{
    public class BookingRepository(AppDbContext context) : IBookingRepository
    {
        public async Task<PagedResult<Booking>> GetPagedBookingAsync(BookingFilterParameters query)
        {
            var bookings = context.Bookings
                .Include(b => b.Vehicle)
                    .ThenInclude(v => v.RentalCompany)
                .Include(b => b.Customer)
                .AsQueryable();

            if (query.BookingStatus.HasValue)
            {
                bookings = bookings.Where(b => b.BookingStatus == query.BookingStatus.Value);
            }

            if (query.RentalCompanyId.HasValue)
            {
                var code = $"CMP-{query.RentalCompanyId.Value:D3}";
                var strVal = query.RentalCompanyId.Value.ToString();
                bookings = bookings.Where(b => b.Vehicle.RentalCompany.CompanyCode == code || 
                                               b.Vehicle.RentalCompany.CompanyCode.Contains(strVal));
            }

            if (query.FromDate.HasValue)
            {
                var fromDate = query.FromDate.Value.Date;
                bookings = bookings.Where(b => b.BookedOn >= fromDate);
            }

            if (query.ToDate.HasValue)
            {
                var toDateExclusive = query.ToDate.Value.Date.AddDays(1);
                bookings = bookings.Where(b => b.BookedOn < toDateExclusive);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchBy))
            {
                var search = query.SearchBy.Trim();
                bookings = bookings.Where(b => b.PickupLocation.Contains(search) ||
                                               b.ReturnLocation.Contains(search) ||
                                               b.Vehicle.MakeAndModel.Contains(search) ||
                                               b.Vehicle.LicensePlate.Contains(search) ||
                                               b.Vehicle.RentalCompany.CompanyName.Contains(search) ||
                                               b.Customer.FullName.Contains(search) ||
                                               (b.Customer.Email != null && b.Customer.Email.Contains(search)));
            }

            bookings = query.SortBy?.ToLower() switch
            {
                "id" => query.SortDirection == "desc" ? bookings.OrderByDescending(b => b.Id) : bookings.OrderBy(b => b.Id),
                "pickupdatetime" or "pickupdate" => query.SortDirection == "desc" ? bookings.OrderByDescending(b => b.PickupDateTime) : bookings.OrderBy(b => b.PickupDateTime),
                "returndatetime" or "returndate" => query.SortDirection == "desc" ? bookings.OrderByDescending(b => b.ReturnDateTime) : bookings.OrderBy(b => b.ReturnDateTime),
                "totalamount" or "amount" => query.SortDirection == "desc" ? bookings.OrderByDescending(b => b.TotalAmount) : bookings.OrderBy(b => b.TotalAmount),
                "bookingstatus" or "status" => query.SortDirection == "desc" ? bookings.OrderByDescending(b => b.BookingStatus.ToString()) : bookings.OrderBy(b => b.BookingStatus.ToString()),
                "bookedon" => query.SortDirection == "desc" ? bookings.OrderByDescending(b => b.BookedOn) : bookings.OrderBy(b => b.BookedOn),
                "pickuplocation" => query.SortDirection == "desc" ? bookings.OrderByDescending(b => b.PickupLocation) : bookings.OrderBy(b => b.PickupLocation),
                "returnlocation" => query.SortDirection == "desc" ? bookings.OrderByDescending(b => b.ReturnLocation) : bookings.OrderBy(b => b.ReturnLocation),
                _ => bookings.OrderByDescending(b => b.Id)
            };

            var totalCount = await bookings.CountAsync();

            var items = await bookings
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<Booking>
            {
                Items = items,
                TotalCount = totalCount,
                PageSize = query.PageSize,
                PageNumber = query.PageNumber,
            };
        }

        public async Task<Booking?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await context.Bookings
                .Include(b => b.Vehicle)
                    .ThenInclude(v => v.RentalCompany)
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }

        public async Task UpdateAsync(Booking booking, CancellationToken cancellationToken = default)
        {
            context.Bookings.Update(booking);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Booking>> GetCustomerBookings(Guid customerId, CancellationToken cancellationToken = default)
        {
            return await context.Bookings
                .Where(b => b.CustomerId == customerId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Booking>> GetBookingsByVehicleId(int vehicleId, CancellationToken cancellationToken = default)
        {
            return await context.Bookings
                .Where(b => b.VehicleId == vehicleId)
                .ToListAsync(cancellationToken);
        }

        public async Task BookVehicleAsync(Booking newBooking, List<int> extraServiceIds, CancellationToken cancellationToken = default)
        {
            await context.Bookings.AddAsync(newBooking, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            int bookingId = newBooking.Id;

            if (extraServiceIds != null && extraServiceIds.Any())
            {
                var services = await context.ExtraServices
                    .Where(x => extraServiceIds.Contains(x.Id))
                    .ToListAsync(cancellationToken);

                var bookingServices = services.Select(service => new BookingExtraService
                {
                    BookingId = bookingId,
                    ExtraServiceId = service.Id,
                    Price = service.Price,
                }).ToList();

                foreach (var service in bookingServices)
                {
                    newBooking.TotalAmount += service.Price;
                    newBooking.AdditionalServiceAmount += service.Price;
                }

                await context.BookingExtraServices.AddRangeAsync(bookingServices, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<PagedResult<Booking>> MyPagedRentalsAsync(MyRentalFilterParameters query, CancellationToken cancellationToken = default)
        {
            var booking = context.Bookings
                .Include(x => x.Vehicle)
                .Where(x => x.CustomerId == query.CustomerId);

            if (query.BookingStatus.HasValue)
            {
                booking = booking.Where(x => x.BookingStatus == query.BookingStatus.Value);
            }

            if (query.SeatingCapacity.HasValue)
            {
                booking = booking.Where(x => x.Vehicle.SeatingCapacity == query.SeatingCapacity.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchBy))
            {
                var search = query.SearchBy.Trim();

                booking = booking.Where(x => x.PickupLocation.Contains(search) ||
                                             x.ReturnLocation.Contains(search) ||
                                             x.Vehicle.MakeAndModel.Contains(search) ||
                                             x.Vehicle.LicensePlate.Contains(search) ||
                                             x.Vehicle.RegistrationNumber.Contains(search));
            }

            booking = query.SortBy?.ToLower() switch
            {
                "id" => query.SortDirection == "asc" ? booking.OrderBy(x => x.Id) : booking.OrderByDescending(x => x.Id),
                "pickupdatetime" or "pickupdate" => query.SortDirection == "asc" ? booking.OrderBy(x => x.PickupDateTime) : booking.OrderByDescending(x => x.PickupDateTime),
                "returndatetime" or "returndate" => query.SortDirection == "asc" ? booking.OrderBy(x => x.ReturnDateTime) : booking.OrderByDescending(x => x.ReturnDateTime),
                "totalamount" or "amount" => query.SortDirection == "asc" ? booking.OrderBy(x => x.TotalAmount) : booking.OrderByDescending(x => x.TotalAmount),
                "bookingstatus" or "status" => query.SortDirection == "asc" ? booking.OrderBy(x => x.BookingStatus.ToString()) : booking.OrderByDescending(x => x.BookingStatus.ToString()),
                "seatingcapacity" => query.SortDirection == "asc" ? booking.OrderBy(x => x.Vehicle.SeatingCapacity) : booking.OrderByDescending(x => x.Vehicle.SeatingCapacity),
                _ => booking.OrderByDescending(x => x.Id)
            };

            var totalCount = await booking.CountAsync(cancellationToken);

            var items = await booking
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<Booking>
            {
                Items = items,
                TotalCount = totalCount,
                PageSize = query.PageSize,
                PageNumber = query.PageNumber,
            };
        }
    }
}
