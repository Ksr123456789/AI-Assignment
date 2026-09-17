using RentalManagementSystem.Application.Features.Vehicle.Commands.AddVehicle;
using RentalManagementSystem.Application.Features.Vehicle.Commands.DeleteVehicle;
using RentalManagementSystem.Application.Features.Vehicle.Commands.UpdateVehicle;
using RentalManagementSystem.Application.Features.Vehicle.Queries.GetPagedVehicle;
using RentalManagementSystem.Domain.Common;

namespace RentalManagementSystem.Application.ServiceContracts
{
    public interface IVehicleService
    {
        Task<AddVehicleCommandResponse> AddVehicleAsync(AddVehicleCommand command, CancellationToken cancellationToken = default);
        Task<UpdateVehicleCommandResponse> UpdateVehicleAsync(UpdateVehicleCommand command, CancellationToken cancellationToken = default);
        Task<PagedResult<GetPagedVehicleQueryResponse>> GetPagedVehicle(GetPagedVehicleQuery query);
        Task<DeleteVehicleCommandResponse> DeleteVehicleAsync(DeleteVehicleCommand command, CancellationToken cancellationToken = default);
    }
}
