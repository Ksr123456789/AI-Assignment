using RentalManagementSystem.Application.Features.RentalCompany.Commands.AddRentalCompany;
using RentalManagementSystem.Application.Features.RentalCompany.Commands.DeleteRentalCompany;
using RentalManagementSystem.Application.Features.RentalCompany.Commands.UpdateRentalCompany;
using RentalManagementSystem.Application.Features.RentalCompany.Queries.GetAllRentalCompany;
using RentalManagementSystem.Application.Features.RentalCompany.Queries.GetPagedRentalCompany;
using RentalManagementSystem.Domain.Common;

namespace RentalManagementSystem.Application.ServiceContracts
{
    public interface IRentalCompanyService
    {
        Task<AddRentalCompanyCommandResponse> AddRentalCompanyAsync(AddRentalCompanyCommand command, CancellationToken cancellationToken = default);
        Task<UpdateRentalCompanyCommandResponse> UpdateRentalCompanyAsync(UpdateRentalCompanyCommand command, CancellationToken cancellationToken = default);
        Task<GetAllRentalCompanyQueryResponse> GetAllRentalCompanyAsync(GetAllRentalCompanyQuery query, CancellationToken cancellationToken = default);
        Task<PagedResult<GetPagedRentalCompanyQueryResponse>> GetPagedRentalCompany(GetPagedRentalCompanyQuery query);
        Task<DeleteRentalCompanyCommandResponse> DeleteRentalCompanyAsync(DeleteRentalCompanyCommand command, CancellationToken cancellationToken = default);
    }
}
