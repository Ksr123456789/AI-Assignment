using MediatR;
using RentalManagementSystem.Application.ServiceContracts;
using RentalManagementSystem.Domain.Common;

namespace RentalManagementSystem.Application.Features.Vehicle.Queries.GetPagedVehicle
{
    public class GetPagedVehicleQueryHandler(IVehicleService vehicleService)
        : IRequestHandler<GetPagedVehicleQuery, PagedResult<GetPagedVehicleQueryResponse>>
    {
        public async Task<PagedResult<GetPagedVehicleQueryResponse>> Handle(
            GetPagedVehicleQuery request,
            CancellationToken cancellationToken)
        {
            return await vehicleService.GetPagedVehicle(request);
        }
    }
}
