using RentalManagementSystem.Application.Features.Vehicle.Commands.AddVehicle;
using RentalManagementSystem.Application.Features.Vehicle.Commands.DeleteVehicle;
using RentalManagementSystem.Application.Features.Vehicle.Commands.UpdateVehicle;
using RentalManagementSystem.Application.Features.Vehicle.DTOs;
using RentalManagementSystem.Application.Features.Vehicle.Queries.GetPagedVehicle;
using RentalManagementSystem.Application.ServiceContracts;
using RentalManagementSystem.Domain.Common;
using RentalManagementSystem.Domain.Entities;
using RentalManagementSystem.Domain.RepositoryContracts;

namespace RentalManagementSystem.Application.Services
{
    public class VehicleService(
        IVehicleRepository vehicleRepository,
        ICurrentUser currentUser) : IVehicleService
    {
        public async Task<AddVehicleCommandResponse> AddVehicleAsync(
            AddVehicleCommand command,
            CancellationToken cancellationToken = default)
        {
            var errors = new List<string>();

            // 1. Verify Rental Company existence
            var companyExists = await vehicleRepository.RentalCompanyExistsAsync(command.RentalCompanyId, cancellationToken);
            if (!companyExists)
            {
                errors.Add($"Rental company with ID '{command.RentalCompanyId}' does not exist.");
            }

            // 2. Verify Vehicle Category existence
            var categoryExists = await vehicleRepository.VehicleCategoryExistsAsync(command.VehicleCategoryId, cancellationToken);
            if (!categoryExists)
            {
                errors.Add($"Vehicle category with ID '{command.VehicleCategoryId}' does not exist.");
            }

            // 3. Check License Plate uniqueness
            var licensePlateTrimmed = command.LicensePlate.Trim();
            var licensePlateExists = await vehicleRepository.ExistsByLicensePlateAsync(licensePlateTrimmed, cancellationToken);
            if (licensePlateExists)
            {
                errors.Add($"A vehicle with license plate '{licensePlateTrimmed}' already exists.");
            }

            // 4. Check Registration Number uniqueness
            var regNumberTrimmed = command.RegistrationNumber.Trim();
            var regNumberExists = await vehicleRepository.ExistsByRegistrationNumberAsync(regNumberTrimmed, cancellationToken);
            if (regNumberExists)
            {
                errors.Add($"A vehicle with registration number '{regNumberTrimmed}' already exists.");
            }

            if (errors.Any())
            {
                return new AddVehicleCommandResponse
                {
                    Success = false,
                    Message = "Vehicle validation failed.",
                    Errors = errors
                };
            }

            var entity = new Vehicle
            {
                RentalCompanyId = command.RentalCompanyId,
                VehicleCategoryId = command.VehicleCategoryId,
                MakeAndModel = command.MakeAndModel.Trim(),
                LicensePlate = licensePlateTrimmed.ToUpper(),
                RegistrationNumber = regNumberTrimmed.ToUpper(),
                YearOfManufacture = command.YearOfManufacture,
                SeatingCapacity = command.SeatingCapacity,
                DailyRentalRate = command.DailyRentalRate,
                AvailabilityStatus = command.AvailabilityStatus,
                Mileage = command.Mileage,
                CreatedBy = currentUser.Email ?? currentUser.UserId?.ToString() ?? "System",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            await vehicleRepository.AddVehicleAsync(entity, cancellationToken);

            var createdVehicle = await vehicleRepository.GetByIdAsync(entity.Id, cancellationToken) ?? entity;

            return new AddVehicleCommandResponse
            {
                Success = true,
                Message = "Vehicle added successfully.",
                Data = MapToDto(createdVehicle)
            };
        }

        public async Task<UpdateVehicleCommandResponse> UpdateVehicleAsync(
            UpdateVehicleCommand command,
            CancellationToken cancellationToken = default)
        {
            var vehicle = await vehicleRepository.GetByIdAsync(command.Id, cancellationToken);
            if (vehicle == null || vehicle.IsDeleted)
            {
                return new UpdateVehicleCommandResponse
                {
                    Success = false,
                    Message = "Vehicle not found.",
                    Errors = ["Vehicle does not exist."]
                };
            }

            var errors = new List<string>();

            // 1. Verify Rental Company existence
            var companyExists = await vehicleRepository.RentalCompanyExistsAsync(command.RentalCompanyId, cancellationToken);
            if (!companyExists)
            {
                errors.Add($"Rental company with ID '{command.RentalCompanyId}' does not exist.");
            }

            // 2. Verify Vehicle Category existence
            var categoryExists = await vehicleRepository.VehicleCategoryExistsAsync(command.VehicleCategoryId, cancellationToken);
            if (!categoryExists)
            {
                errors.Add($"Vehicle category with ID '{command.VehicleCategoryId}' does not exist.");
            }

            // 3. Check License Plate uniqueness if changed
            var licensePlateTrimmed = command.LicensePlate.Trim();
            var licensePlateExists = await vehicleRepository.ExistsByLicensePlateAsync(licensePlateTrimmed, cancellationToken);
            if (licensePlateExists && !string.Equals(vehicle.LicensePlate, licensePlateTrimmed, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"A vehicle with license plate '{licensePlateTrimmed}' already exists.");
            }

            // 4. Check Registration Number uniqueness if changed
            var regNumberTrimmed = command.RegistrationNumber.Trim();
            var regNumberExists = await vehicleRepository.ExistsByRegistrationNumberAsync(regNumberTrimmed, cancellationToken);
            if (regNumberExists && !string.Equals(vehicle.RegistrationNumber, regNumberTrimmed, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"A vehicle with registration number '{regNumberTrimmed}' already exists.");
            }

            if (errors.Any())
            {
                return new UpdateVehicleCommandResponse
                {
                    Success = false,
                    Message = "Vehicle validation failed.",
                    Errors = errors
                };
            }

            vehicle.RentalCompanyId = command.RentalCompanyId;
            vehicle.VehicleCategoryId = command.VehicleCategoryId;
            vehicle.MakeAndModel = command.MakeAndModel.Trim();
            vehicle.LicensePlate = licensePlateTrimmed.ToUpper();
            vehicle.RegistrationNumber = regNumberTrimmed.ToUpper();
            vehicle.YearOfManufacture = command.YearOfManufacture;
            vehicle.SeatingCapacity = command.SeatingCapacity;
            vehicle.DailyRentalRate = command.DailyRentalRate;
            vehicle.AvailabilityStatus = command.AvailabilityStatus;
            vehicle.Mileage = command.Mileage;
            vehicle.UpdatedBy = currentUser.Email ?? currentUser.UserId?.ToString() ?? "System";
            vehicle.UpdatedDate = DateTime.UtcNow;

            await vehicleRepository.UpdateAsync(vehicle, cancellationToken);

            var updatedVehicle = await vehicleRepository.GetByIdAsync(vehicle.Id, cancellationToken) ?? vehicle;

            return new UpdateVehicleCommandResponse
            {
                Success = true,
                Message = "Vehicle updated successfully.",
                Data = MapToDto(updatedVehicle)
            };
        }

        public async Task<PagedResult<GetPagedVehicleQueryResponse>> GetPagedVehicle(GetPagedVehicleQuery query)
        {
            var queryData = new VehicleFilterParameters
            {
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                SortBy = query.SortBy,
                SortDirection = query.SortDirection,
                SearchBy = query.SearchBy,
                VehicleCategoryId = query.VehicleCategoryId,
                VehicleAvailabilityStatus = query.VehicleAvailabilityStatus,
            };

            var result = await vehicleRepository.GetPagedVehicleAsync(queryData);

            return new PagedResult<GetPagedVehicleQueryResponse>
            {
                Items = result.Items.Select(x => new GetPagedVehicleQueryResponse
                {
                    Id = x.Id,
                    RentalCompanyId = x.RentalCompanyId,
                    RentalCompanyName = x.RentalCompany?.CompanyName ?? string.Empty,
                    VehicleCategoryId = x.VehicleCategoryId,
                    VehicleCategoryName = x.VehicleCategory?.CategoryName ?? string.Empty,
                    MakeAndModel = x.MakeAndModel,
                    LicensePlate = x.LicensePlate,
                    RegistrationNumber = x.RegistrationNumber,
                    YearOfManufacture = x.YearOfManufacture,
                    SeatingCapacity = x.SeatingCapacity,
                    DailyRentalRate = x.DailyRentalRate,
                    AvailabilityStatus = x.AvailabilityStatus,
                    Mileage = x.Mileage,
                }).ToList(),

                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
            };
        }

        public async Task<DeleteVehicleCommandResponse> DeleteVehicleAsync(
            DeleteVehicleCommand command,
            CancellationToken cancellationToken = default)
        {
            var vehicle = await vehicleRepository.GetByIdAsync(command.Id, cancellationToken);
            if (vehicle == null || vehicle.IsDeleted)
            {
                return new DeleteVehicleCommandResponse
                {
                    Success = false,
                    Message = "Vehicle not found.",
                    Errors = ["Vehicle does not exist."]
                };
            }

            var deletedBy = currentUser.Email ?? currentUser.UserId?.ToString() ?? "System";
            vehicle.IsDeleted = true;
            vehicle.DeletedBy = deletedBy;
            vehicle.DeletedDate = DateTime.UtcNow;

            await vehicleRepository.DeleteVehicleAsync(vehicle, cancellationToken);

            return new DeleteVehicleCommandResponse
            {
                Success = true,
                Message = "Vehicle deleted successfully.",
                Data = MapToDto(vehicle)
            };
        }

        private static VehicleDto MapToDto(Vehicle vehicle)
        {
            return new VehicleDto
            {
                Id = vehicle.Id,
                RentalCompanyId = vehicle.RentalCompanyId,
                RentalCompanyName = vehicle.RentalCompany?.CompanyName ?? string.Empty,
                VehicleCategoryId = vehicle.VehicleCategoryId,
                VehicleCategoryName = vehicle.VehicleCategory?.CategoryName ?? string.Empty,
                MakeAndModel = vehicle.MakeAndModel,
                LicensePlate = vehicle.LicensePlate,
                RegistrationNumber = vehicle.RegistrationNumber,
                YearOfManufacture = vehicle.YearOfManufacture,
                SeatingCapacity = vehicle.SeatingCapacity,
                DailyRentalRate = vehicle.DailyRentalRate,
                AvailabilityStatus = vehicle.AvailabilityStatus,
                Mileage = vehicle.Mileage,
                CreatedDate = vehicle.CreatedDate
            };
        }
    }
}
