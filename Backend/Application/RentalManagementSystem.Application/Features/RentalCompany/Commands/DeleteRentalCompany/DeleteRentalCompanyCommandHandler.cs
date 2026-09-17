using MediatR;
using RentalManagementSystem.Application.ServiceContracts;

namespace RentalManagementSystem.Application.Features.RentalCompany.Commands.DeleteRentalCompany
{
    public class DeleteRentalCompanyCommandHandler(IRentalCompanyService rentalCompanyService)
        : IRequestHandler<DeleteRentalCompanyCommand, DeleteRentalCompanyCommandResponse>
    {
        public async Task<DeleteRentalCompanyCommandResponse> Handle(DeleteRentalCompanyCommand request, CancellationToken cancellationToken)
        {
            return await rentalCompanyService.DeleteRentalCompanyAsync(request, cancellationToken);
        }
    }
}
