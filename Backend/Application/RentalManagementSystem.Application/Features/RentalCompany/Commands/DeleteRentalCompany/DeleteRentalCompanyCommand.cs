using MediatR;

namespace RentalManagementSystem.Application.Features.RentalCompany.Commands.DeleteRentalCompany
{
    public class DeleteRentalCompanyCommand : IRequest<DeleteRentalCompanyCommandResponse>
    {
        public Guid Id { get; set; }

       
    }

   
}
