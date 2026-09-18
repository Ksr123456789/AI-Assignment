using System.Text.RegularExpressions;
using RentalManagementSystem.Application.Features.RentalCompany.Commands.AddRentalCompany;
using RentalManagementSystem.Application.Features.RentalCompany.Commands.DeleteRentalCompany;
using RentalManagementSystem.Application.Features.RentalCompany.Commands.UpdateRentalCompany;
using RentalManagementSystem.Application.Features.RentalCompany.DTOs;
using RentalManagementSystem.Application.Features.RentalCompany.Queries.GetAllRentalCompany;
using RentalManagementSystem.Application.Features.RentalCompany.Queries.GetPagedRentalCompany;
using RentalManagementSystem.Application.ServiceContracts;
using RentalManagementSystem.Domain.Common;
using RentalManagementSystem.Domain.Entities;
using RentalManagementSystem.Domain.RepositoryContracts;

namespace RentalManagementSystem.Application.Services
{
    public class RentalCompanyService(
        IRentalCompanyRepository rentalCompanyRepository,
        ICurrentUser currentUser) : IRentalCompanyService
    {
        public async Task<AddRentalCompanyCommandResponse> AddRentalCompanyAsync(
            AddRentalCompanyCommand command, 
            CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrWhiteSpace(command.Phone) && !Regex.IsMatch(command.Phone, @"^[6-9]\d{9}$"))
            {
                return new AddRentalCompanyCommandResponse
                {
                    Success = false,
                    Message = "Phone number must start with a digit greater than 5 (6-9) and contain 10 digits.",
                    Errors = ["Phone number must start with a digit greater than 5 (6-9) and contain 10 digits."]
                };
            }

            var licenseExists = await rentalCompanyRepository.ExistsByLicenseNumberAsync(command.LicenseNumber, cancellationToken);
            if (licenseExists)
            {
                return new AddRentalCompanyCommandResponse
                {
                    Success = false,
                    Message = "A company with this license number already exists.",
                    Errors = ["License number must be unique."]
                };
            }

            // Automatically generate unique company code in the backend
            var companyCode = await rentalCompanyRepository.GenerateCompanyCodeAsync(cancellationToken);

            var entity = new RentalCompany
            {
                Id = Guid.NewGuid(),
                CompanyCode = companyCode,
                CompanyName = command.CompanyName,
                CompanyType = command.CompanyType,
                OwnerName = command.OwnerName,
                Phone = command.Phone,
                HeadquartersLocation = command.HeadquartersLocation,
                LicenseNumber = command.LicenseNumber,
                Status = command.Status,
                CreatedBy = currentUser.Email ?? currentUser.UserId?.ToString() ?? "System",
                CreatedDate = DateTime.UtcNow
            };

            await rentalCompanyRepository.AddRentalCompanyAsync(entity, cancellationToken);

            return new AddRentalCompanyCommandResponse
            {
                Success = true,
                Message = "Rental company added successfully.",
                Data = MapToDto(entity)
            };
        }

        public async Task<UpdateRentalCompanyCommandResponse> UpdateRentalCompanyAsync(
            UpdateRentalCompanyCommand command, 
            CancellationToken cancellationToken = default)
        {
            var company = await rentalCompanyRepository.GetByIdAsync(command.Id, cancellationToken);
            if (company == null || company.IsDeleted)
            {
                return new UpdateRentalCompanyCommandResponse
                {
                    Success = false,
                    Message = "Rental company not found.",
                    Errors = ["Company does not exist."]
                };
            }

            if (!string.IsNullOrWhiteSpace(command.Phone) && !Regex.IsMatch(command.Phone, @"^[6-9]\d{9}$"))
            {
                return new UpdateRentalCompanyCommandResponse
                {
                    Success = false,
                    Message = "Phone number must start with a digit greater than 5 (6-9) and contain 10 digits.",
                    Errors = ["Phone number must start with a digit greater than 5 (6-9) and contain 10 digits."]
                };
            }

            var licenseExists = await rentalCompanyRepository.ExistsByLicenseNumberAsync(command.LicenseNumber, cancellationToken);
            if (licenseExists && !string.Equals(company.LicenseNumber, command.LicenseNumber, StringComparison.OrdinalIgnoreCase))
            {
                return new UpdateRentalCompanyCommandResponse
                {
                    Success = false,
                    Message = "A company with this license number already exists.",
                    Errors = ["License number must be unique."]
                };
            }

            company.CompanyName = command.CompanyName;
            company.CompanyType = command.CompanyType;
            company.OwnerName = command.OwnerName;
            company.Phone = command.Phone;
            company.HeadquartersLocation = command.HeadquartersLocation;
            company.LicenseNumber = command.LicenseNumber;
            company.Status = command.Status;
            company.UpdatedBy =  currentUser.UserId?.ToString() ??  "System";
            company.UpdatedDate = DateTime.UtcNow;

            await rentalCompanyRepository.UpdateAsync(company, cancellationToken);

            return new UpdateRentalCompanyCommandResponse
            {
                Success = true,
                Message = "Rental company updated successfully.",
                Data = MapToDto(company)
            };
        }

        public async Task<GetAllRentalCompanyQueryResponse> GetAllRentalCompanyAsync(
            GetAllRentalCompanyQuery query, 
            CancellationToken cancellationToken = default)
        {
            var companies = await rentalCompanyRepository.GetAllAsync(cancellationToken);

            return new GetAllRentalCompanyQueryResponse
            {
                Success = true,
                Message = "Rental companies retrieved successfully.",
                Data = companies.Select(MapToDto).ToList()
            };
        }

        public async Task<PagedResult<GetPagedRentalCompanyQueryResponse>> GetPagedRentalCompany(GetPagedRentalCompanyQuery query)
        {
            var queryData = new RentalCompanyFilterParameters
            {
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                SortBy = query.SortBy,
                SortDirection = query.SortDirection,
                SearchBy = query.SearchBy,
                CompanyStatus = query.CompanyStatus,
                CompanyType = query.CompanyType,
            };

            var result = await rentalCompanyRepository.GetPagedRentalCompanyAsync(queryData);

            return new PagedResult<GetPagedRentalCompanyQueryResponse>
            {
                Items = result.Items.Select(x => new GetPagedRentalCompanyQueryResponse
                {
                    Id = x.Id,
                    CompanyName = x.CompanyName,
                    CompanyType = x.CompanyType,
                    OwnerName = x.OwnerName,
                    PhoneNumber = x.PhoneNumber,
                    HeadquartersLocation = x.HeadquartersLocation,
                    LicenseNumber = x.LicenseNumber,
                    Status = x.Status,
                    VehicleCount = x.Vehicles?.Count(v => !v.IsDeleted) ?? 0,
                }).ToList(),

                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
            };
        }

        public async Task<DeleteRentalCompanyCommandResponse> DeleteRentalCompanyAsync(
            DeleteRentalCompanyCommand command, 
            CancellationToken cancellationToken = default)
        {
            var company = await rentalCompanyRepository.GetByIdAsync(command.Id, cancellationToken);
            if (company == null || company.IsDeleted)
            {
                return new DeleteRentalCompanyCommandResponse
                {
                    Success = false,
                    Message = "Rental company not found.",
                    Errors = ["Company does not exist."]
                };
            }

            var hasVehicles = await rentalCompanyRepository.HasActiveVehiclesAsync(command.Id, cancellationToken);
            if (hasVehicles)
            {
                return new DeleteRentalCompanyCommandResponse
                {
                    Success = false,
                    Message = "Cannot delete rental company with existing vehicle listings. Please remove or reassign all vehicles first.",
                    Errors = ["Cannot delete rental company with existing vehicle listings."]
                };
            }

            var deletedBy = currentUser.Email ?? currentUser.UserId?.ToString() ?? "System";
            company.IsDeleted = true;
            company.DeletedBy = deletedBy;
            company.DeletedDate = DateTime.UtcNow;

            await rentalCompanyRepository.DeleteRentalCompanyAsync(company, cancellationToken);

            return new DeleteRentalCompanyCommandResponse
            {
                Success = true,
                Message = "Rental company deleted successfully.",
                Data = MapToDto(company)
            };
        }

       

        private static RentalCompanyDto MapToDto(RentalCompany entity)
        {
            return new RentalCompanyDto
            {
                Id = entity.Id,
                CompanyCode = entity.CompanyCode,
                CompanyName = entity.CompanyName,
                CompanyType = entity.CompanyType,
                OwnerName = entity.OwnerName,
                Phone = entity.Phone,
                HeadquartersLocation = entity.HeadquartersLocation,
                LicenseNumber = entity.LicenseNumber,
                Status = entity.Status,
                CreatedBy = entity.CreatedBy,
                CreatedDate = entity.CreatedDate,
                UpdatedBy = entity.UpdatedBy,
                UpdatedDate = entity.UpdatedDate,
                VehicleCount = entity.Vehicles?.Count(v => !v.IsDeleted) ?? 0
            };
        }
    }
}
