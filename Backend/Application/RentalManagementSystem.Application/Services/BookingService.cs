using Microsoft.AspNetCore.Identity;
using RentalManagementSystem.Application.Features.Booking.Commands.BookVehicle;
using RentalManagementSystem.Application.Features.Booking.Commands.UpdateBookingStatus;
using RentalManagementSystem.Application.Features.Booking.DTOs;
using RentalManagementSystem.Application.Features.Booking.Queries.GetPagedBooking;
using RentalManagementSystem.Application.Features.Booking.Queries.MyPagedRentals;
using RentalManagementSystem.Application.ServiceContracts;
using RentalManagementSystem.Domain.Common;
using RentalManagementSystem.Domain.Constants;
using RentalManagementSystem.Domain.Entities;
using RentalManagementSystem.Domain.Enums;
using RentalManagementSystem.Domain.RepositoryContracts;

namespace RentalManagementSystem.Application.Services
{
    public class BookingService(
        IBookingRepository bookingRepository,
        IVehicleRepository vehicleRepository,
        UserManager<ApplicationUser> userManager,
        ICurrentUser currentUser) : IBookingService
    {
        public async Task<PagedResult<GetPagedBookingQueryResponse>> GetPagedBooking(GetPagedBookingQuery query)
        {
            var queryData = new BookingFilterParameters
            {
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                SortBy = query.SortBy,
                SortDirection = query.SortDirection,
                SearchBy = query.SearchBy,
                BookingStatus = query.BookingStatus,
                RentalCompanyId = query.RentalCompanyId,
                FromDate = query.FromDate,
                ToDate = query.ToDate,
            };

            var result = await bookingRepository.GetPagedBookingAsync(queryData);

            return new PagedResult<GetPagedBookingQueryResponse>
            {
                Items = result.Items.Select(b => new GetPagedBookingQueryResponse
                {
                    Id = b.Id,
                    PickupDateTime = b.PickupDateTime,
                    ReturnDateTime = b.ReturnDateTime,
                    PickupLocation = b.PickupLocation,
                    ReturnLocation = b.ReturnLocation,
                    Insurance = b.Insurance,
                    BookingStatus = b.BookingStatus,
                    TotalDays = b.TotalDays,
                    RentalAmount = b.RentalAmount,
                    AdditionalServiceAmount = b.AdditionalServiceAmount,
                    TotalAmount = b.TotalAmount,
                    BookedOn = b.BookedOn,
                    VehicleId = b.VehicleId,
                    VehicleMakeAndModel = b.Vehicle?.MakeAndModel ?? string.Empty,
                    VehicleLicensePlate = b.Vehicle?.LicensePlate ?? string.Empty,
                    RentalCompanyId = b.Vehicle?.RentalCompanyId ?? Guid.Empty,
                    RentalCompanyName = b.Vehicle?.RentalCompany?.CompanyName ?? string.Empty,
                    CustomerId = b.CustomerId,
                    CustomerName = b.Customer?.FullName ?? string.Empty,
                    CustomerEmail = b.Customer?.Email ?? string.Empty
                }).ToList(),

                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
            };
        }

        public async Task<UpdateBookingStatusCommandResponse> UpdateBookingStatusAsync(
            UpdateBookingStatusCommand command,
            CancellationToken cancellationToken = default)
        {
            var booking = await bookingRepository.GetByIdAsync(command.BookingId, cancellationToken);
            if (booking == null)
            {
                return new UpdateBookingStatusCommandResponse
                {
                    Success = false,
                    Message = "Booking not found.",
                    Errors = ["Booking does not exist."]
                };
            }

            var isAdmin = currentUser.IsInRole(UserRoles.Admin);
            var isCustomer = currentUser.IsInRole(UserRoles.Customer);
            var userId = currentUser.UserId;

            decimal? forfeitedAmount = null;
            decimal? refundAmount = null;
            string successMessage;

            if (isAdmin)
            {
                var isValidAdminTransition = (booking.BookingStatus, command.Status) switch
                {
                    (BookingStatus.Pending, BookingStatus.Confirmed) => true,
                    (BookingStatus.Pending, BookingStatus.Cancelled) => true,
                    (BookingStatus.Confirmed, BookingStatus.Active) => true,
                    (BookingStatus.Confirmed, BookingStatus.Cancelled) => true,
                    (BookingStatus.Active, BookingStatus.Completed) => true,
                    _ => false
                };

                if (!isValidAdminTransition)
                {
                    return new UpdateBookingStatusCommandResponse
                    {
                        Success = false,
                        Message = $"Invalid status transition from '{booking.BookingStatus}' to '{command.Status}'.",
                        Errors = [$"Allowed Admin flow: Pending -> Confirmed -> Active -> Completed, or Pending/Confirmed -> Cancelled."]
                    };
                }

                var oldStatus = booking.BookingStatus;
                booking.BookingStatus = command.Status;

                if (booking.Vehicle != null)
                {
                    if (command.Status == BookingStatus.Active)
                    {
                        booking.Vehicle.AvailabilityStatus = VehicleAvailabilityStatus.Unavailable;
                    }
                    else if (command.Status == BookingStatus.Completed || command.Status == BookingStatus.Cancelled)
                    {
                        booking.Vehicle.AvailabilityStatus = VehicleAvailabilityStatus.Available;
                    }
                }

                successMessage = $"Booking status successfully updated from '{oldStatus}' to '{command.Status}'.";
            }
            else if (isCustomer)
            {
                if (userId.HasValue && booking.CustomerId != userId.Value)
                {
                    return new UpdateBookingStatusCommandResponse
                    {
                        Success = false,
                        Message = "You are not authorized to update this booking.",
                        Errors = ["Customers can only update their own bookings."]
                    };
                }

                if (command.Status != BookingStatus.Cancelled)
                {
                    return new UpdateBookingStatusCommandResponse
                    {
                        Success = false,
                        Message = "Customers can only cancel bookings.",
                        Errors = ["Invalid action requested."]
                    };
                }

                if (booking.BookingStatus != BookingStatus.Pending)
                {
                    return new UpdateBookingStatusCommandResponse
                    {
                        Success = false,
                        Message = "Customers can cancel Pending rentals only.",
                        Errors = [$"Cannot cancel a booking with status '{booking.BookingStatus}'."]
                    };
                }

                var hoursUntilPickup = (booking.PickupDateTime - DateTime.UtcNow).TotalHours;
                if (hoursUntilPickup <= 24)
                {
                    forfeitedAmount = Math.Round(booking.TotalAmount * 0.50m, 2);
                    refundAmount = booking.TotalAmount - forfeitedAmount.Value;
                    successMessage = $"Booking cancelled successfully. Cancelling within 24 hours of scheduled pickup forfeits 50% ({forfeitedAmount.Value:C}). Refund amount: {refundAmount.Value:C}.";
                }
                else
                {
                    forfeitedAmount = 0m;
                    refundAmount = booking.TotalAmount;
                    successMessage = $"Booking cancelled successfully with full refund ({refundAmount.Value:C}).";
                }

                booking.BookingStatus = BookingStatus.Cancelled;

                if (booking.Vehicle != null)
                {
                    booking.Vehicle.AvailabilityStatus = VehicleAvailabilityStatus.Available;
                }
            }
            else
            {
                return new UpdateBookingStatusCommandResponse
                {
                    Success = false,
                    Message = "Unauthorized access.",
                    Errors = ["Only Admins and Customers are permitted to update booking statuses."]
                };
            }

            await bookingRepository.UpdateAsync(booking, cancellationToken);

            return new UpdateBookingStatusCommandResponse
            {
                Success = true,
                Message = successMessage,
                Data = MapToDto(booking),
                ForfeitedAmount = forfeitedAmount,
                RefundAmount = refundAmount
            };
        }

        public async Task<BookVehicleCommandResponse> BookVehicle(
            BookVehicleCommand command, 
            CancellationToken cancellationToken = default)
        {
            // 1. Pickup Date must be today or future
            if (command.PickUpDateTime < DateTime.UtcNow.AddMinutes(-5))
            {
                throw new Exception("Pickup Date must be today or a future date and time.");
            }

            // 2. Return Date must be after Pickup Date
            if (command.ReturnDateTime <= command.PickUpDateTime)
            {
                throw new Exception("Return Date must be after Pickup Date.");
            }

            Guid customerId;
            if (currentUser.UserId.HasValue)
            {
                customerId = currentUser.UserId.Value;
            }
            else
            {
                throw new Exception("Unable to access the current authenticated customer ID.");
            }

            var user = await userManager.FindByIdAsync(customerId.ToString());
            if (user == null)
                throw new Exception("Customer user account not found.");

            // 3. Customer must have valid (non-expired) driving license
            var effectiveLicenseExpiry = command.LicenseExpiryDate.HasValue && command.LicenseExpiryDate.Value != default
                ? command.LicenseExpiryDate.Value
                : user.LicenseExpiryDate;

            if (effectiveLicenseExpiry <= DateTime.UtcNow)
            {
                throw new Exception("Your driving license has expired. A valid (non-expired) driving license is required to book a vehicle.");
            }

            if (effectiveLicenseExpiry < command.ReturnDateTime)
            {
                throw new Exception("Your driving license will expire before the requested vehicle return date.");
            }

            if (!string.IsNullOrWhiteSpace(command.DriverLicenseNumber) && command.DriverLicenseNumber != user.DrivingLicenseNumber)
            {
                user.DrivingLicenseNumber = command.DriverLicenseNumber;
                user.LicenseExpiryDate = effectiveLicenseExpiry;
                await userManager.UpdateAsync(user);
            }

            var customerBookings = await bookingRepository.GetCustomerBookings(customerId, cancellationToken);

            int activeBookings = 0;
            bool sameVehicleBookedTwice = false;
            bool bookingAlreadyExistForTimePeriod = false;

            foreach (var booking in customerBookings)
            {
                // Customer active rentals: Pending + Confirmed + Active
                if (booking.BookingStatus == BookingStatus.Pending || 
                    booking.BookingStatus == BookingStatus.Confirmed || 
                    booking.BookingStatus == BookingStatus.Active)
                {
                    activeBookings++;
                }

                // Customer cannot book same vehicle twice with overlapping dates while status is Pending
                if (booking.VehicleId == command.VehicleId && 
                    booking.BookingStatus == BookingStatus.Pending && 
                    command.PickUpDateTime < booking.ReturnDateTime && 
                    booking.PickupDateTime < command.ReturnDateTime)
                {
                    sameVehicleBookedTwice = true;
                    break;
                }

                if (booking.BookingStatus != BookingStatus.Completed && booking.BookingStatus != BookingStatus.Cancelled && 
                    command.PickUpDateTime < booking.ReturnDateTime && booking.PickupDateTime < command.ReturnDateTime)
                {
                    bookingAlreadyExistForTimePeriod = true;
                }
            }

            // Customer cannot have more than 5 active rentals (Pending + Confirmed + Active)
            if (activeBookings >= 5)
            {
                throw new Exception("Customer cannot have more than 5 active rentals (Pending, Confirmed, or Active).");
            }

            if (sameVehicleBookedTwice) 
                throw new Exception("Customer cannot book the same vehicle twice with overlapping dates while the existing booking status is Pending.");

            if (bookingAlreadyExistForTimePeriod) 
                throw new Exception("Customer already has an active reservation during the requested time period.");

            var vehicle = await vehicleRepository.GetVehicleById(command.VehicleId, cancellationToken);
            if (vehicle == null) 
                throw new Exception("Vehicle not found.");

            if (vehicle.RentalCompany == null || vehicle.RentalCompany.IsDeleted || vehicle.RentalCompany.Status != CompanyStatus.Active)
            {
                throw new Exception("Cannot book this vehicle because its rental company is currently inactive.");
            }

            var vehicleBookings = await bookingRepository.GetBookingsByVehicleId(command.VehicleId, cancellationToken);

            bool vehicleAlreadyBookedForTimePeriod = false;
            foreach (var booking in vehicleBookings)
            {
                if ((booking.BookingStatus == BookingStatus.Pending || 
                     booking.BookingStatus == BookingStatus.Confirmed || 
                     booking.BookingStatus == BookingStatus.Active) 
                    && (command.PickUpDateTime < booking.ReturnDateTime) 
                    && (booking.PickupDateTime < command.ReturnDateTime))
                {
                    vehicleAlreadyBookedForTimePeriod = true;
                    break;
                }
            }

            if (vehicleAlreadyBookedForTimePeriod) 
                throw new Exception("This vehicle is already reserved for the requested time period.");

            // Auto-calculate: Total Days = Return Date - Pickup Date (minimum 1 day)
            int totalDays = (int)Math.Ceiling((command.ReturnDateTime - command.PickUpDateTime).TotalDays);
            if (totalDays <= 0) totalDays = 1;

            // Auto-calculate: Total Amount = Daily Rate * Total Days (plus optional insurance)
            decimal rentalAmount = totalDays * vehicle.DailyRentalRate;
            decimal additionalAmount = command.Insurance ? (15m * totalDays) : 0m;
            decimal totalAmount = rentalAmount + additionalAmount;

            // Auto-set: BookedOn = DateTime.UtcNow, Status = Pending
            var newBooking = new Booking
            {
                PickupDateTime = command.PickUpDateTime,
                ReturnDateTime = command.ReturnDateTime,
                PickupLocation = command.PickupLocation,
                ReturnLocation = command.ReturnLocation,
                VehicleId = command.VehicleId,
                Insurance = command.Insurance,
                CustomerId = customerId,
                BookedOn = DateTime.UtcNow,
                BookingStatus = BookingStatus.Pending,
                TotalDays = totalDays,
                RentalAmount = rentalAmount,
                AdditionalServiceAmount = additionalAmount,
                TotalAmount = totalAmount
            };

            await bookingRepository.BookVehicleAsync(newBooking, command.ExtraServiceIds, cancellationToken);

            var createdBooking = await bookingRepository.GetByIdAsync(newBooking.Id, cancellationToken) ?? newBooking;

            return new BookVehicleCommandResponse
            {
                Success = true,
                Message = "Vehicle booked successfully.",
                Data = MapToDto(createdBooking)
            };
        }

        public async Task<PagedResult<MyPagedRentalsQueryResponse>> MyPagedRentals(
            MyPagedRentalsQuery query,
            CancellationToken cancellationToken = default)
        {
            if (Guid.TryParse(currentUser.UserId?.ToString(), out Guid customerId))
            {
                Console.WriteLine("current user id accessed successfully");
            }
            else
            {
                throw new Exception("can't access current user id");
            }

            var queryData = new MyRentalFilterParameters
            {
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                SortBy = query.SortBy,
                SortDirection = query.SortDirection,
                SearchBy = query.SearchBy,
                BookingStatus = query.BookingStatus,
                SeatingCapacity = query.SeatingCapacity,
                CustomerId = customerId,
            };

            var result = await bookingRepository.MyPagedRentalsAsync(queryData, cancellationToken);

            return new PagedResult<MyPagedRentalsQueryResponse>
            {
                Items = result.Items.Select(x => new MyPagedRentalsQueryResponse
                {
                    Id = x.Id,
                    MakeModal = x.Vehicle.MakeAndModel,
                    SeatingCapacity = x.Vehicle.SeatingCapacity,
                    PickupDateTime = x.PickupDateTime,
                    ReturnDateTime = x.ReturnDateTime,
                    BookingStatus = x.BookingStatus,
                    AdditionalServiceAmount = x.AdditionalServiceAmount,
                    RentalAmount = x.RentalAmount,
                    TotalDays = x.TotalDays,
                    TotalAmount = x.TotalAmount,
                    LicensePlate = x.Vehicle.LicensePlate,
                    RegistrationNumber = x.Vehicle.RegistrationNumber,
                    YearofManufacture = x.Vehicle.YearOfManufacture,
                    DailyRentalRate = x.Vehicle.DailyRentalRate,
                    Mileage = x.Vehicle.Mileage
                }).ToList(),
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
            };
        }

        private static BookingDto MapToDto(Booking b)
        {
            return new BookingDto
            {
                Id = b.Id,
                PickupDateTime = b.PickupDateTime,
                ReturnDateTime = b.ReturnDateTime,
                PickupLocation = b.PickupLocation,
                ReturnLocation = b.ReturnLocation,
                Insurance = b.Insurance,
                BookingStatus = b.BookingStatus,
                TotalDays = b.TotalDays,
                RentalAmount = b.RentalAmount,
                AdditionalServiceAmount = b.AdditionalServiceAmount,
                TotalAmount = b.TotalAmount,
                BookedOn = b.BookedOn,
                VehicleId = b.VehicleId,
                VehicleMakeAndModel = b.Vehicle?.MakeAndModel ?? string.Empty,
                VehicleLicensePlate = b.Vehicle?.LicensePlate ?? string.Empty,
                RentalCompanyId = b.Vehicle?.RentalCompanyId ?? Guid.Empty,
                RentalCompanyName = b.Vehicle?.RentalCompany?.CompanyName ?? string.Empty,
                CustomerId = b.CustomerId,
                CustomerName = b.Customer?.FullName ?? string.Empty,
                CustomerEmail = b.Customer?.Email ?? string.Empty
            };
        }
    }
}
