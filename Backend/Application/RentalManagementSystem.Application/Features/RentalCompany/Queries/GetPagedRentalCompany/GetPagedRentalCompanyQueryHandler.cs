using MediatR;
using RentalManagementSystem.Application.ServiceContracts;
using RentalManagementSystem.Domain.Common;

namespace RentalManagementSystem.Application.Features.RentalCompany.Queries.GetPagedRentalCompany
{
    public class GetPagedRentalCompanyQueryHandler(IRentalCompanyService rentalCompanyService) 
        : IRequestHandler<GetPagedRentalCompanyQuery, PagedResult<GetPagedRentalCompanyQueryResponse>>
    {
        public async Task<PagedResult<GetPagedRentalCompanyQueryResponse>> Handle(
            GetPagedRentalCompanyQuery request, 
            CancellationToken cancellationToken)
        {
            return await rentalCompanyService.GetPagedRentalCompany(request);
        }
    }
}
