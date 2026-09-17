using MediatR;
using RentalManagementSystem.Application.ServiceContracts;

namespace RentalManagementSystem.Application.Features.RentalCompany.Commands.AddRentalCompany
{
    public class AddRentalCompanyCommandHandler(IRentalCompanyService rentalCompanyService) 
        : IRequestHandler<AddRentalCompanyCommand, AddRentalCompanyCommandResponse>
    {
        public async Task<AddRentalCompanyCommandResponse> Handle(
            AddRentalCompanyCommand request, 
            CancellationToken cancellationToken)
        {
            return await rentalCompanyService.AddRentalCompanyAsync(request, cancellationToken);
        }
    }
}
