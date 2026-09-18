using RentalManagementSystem.Domain.Common;
using RentalManagementSystem.Domain.Entities;

namespace RentalManagementSystem.Domain.RepositoryContracts
{
    public interface IRentalCompanyRepository
    {
        Task<string> GenerateCompanyCodeAsync(CancellationToken cancellationToken = default);
        Task<bool> ExistsByLicenseNumberAsync(string licenseNumber, CancellationToken cancellationToken = default);
        Task AddRentalCompanyAsync(RentalCompany company, CancellationToken cancellationToken = default);
        Task<PagedResult<RentalCompany>> GetPagedRentalCompanyAsync(RentalCompanyFilterParameters query);
        Task<IEnumerable<RentalCompany>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<RentalCompany?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> HasActiveVehiclesAsync(Guid companyId, CancellationToken cancellationToken = default);
        Task UpdateAsync(RentalCompany company, CancellationToken cancellationToken = default);
        Task DeleteRentalCompanyAsync(RentalCompany company, CancellationToken cancellationToken = default);
    }
}
