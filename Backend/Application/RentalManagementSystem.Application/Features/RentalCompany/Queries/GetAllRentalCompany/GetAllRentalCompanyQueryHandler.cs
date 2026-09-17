using MediatR;
using RentalManagementSystem.Application.ServiceContracts;

namespace RentalManagementSystem.Application.Features.RentalCompany.Queries.GetAllRentalCompany
{
    public class GetAllRentalCompanyQueryHandler(IRentalCompanyService rentalCompanyService) 
        : IRequestHandler<GetAllRentalCompanyQuery, GetAllRentalCompanyQueryResponse>
    {
        public async Task<GetAllRentalCompanyQueryResponse> Handle(
            GetAllRentalCompanyQuery request, 
            CancellationToken cancellationToken)
        {
            return await rentalCompanyService.GetAllRentalCompanyAsync(request, cancellationToken);
        }
    }
}
