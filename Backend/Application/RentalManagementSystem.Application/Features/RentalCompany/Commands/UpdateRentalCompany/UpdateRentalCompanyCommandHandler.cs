using MediatR;
using RentalManagementSystem.Application.ServiceContracts;

namespace RentalManagementSystem.Application.Features.RentalCompany.Commands.UpdateRentalCompany
{
    public class UpdateRentalCompanyCommandHandler(IRentalCompanyService rentalCompanyService) 
        : IRequestHandler<UpdateRentalCompanyCommand, UpdateRentalCompanyCommandResponse>
    {
        public async Task<UpdateRentalCompanyCommandResponse> Handle(
            UpdateRentalCompanyCommand request, 
            CancellationToken cancellationToken)
        {
            return await rentalCompanyService.UpdateRentalCompanyAsync(request, cancellationToken);
        }
    }
}
