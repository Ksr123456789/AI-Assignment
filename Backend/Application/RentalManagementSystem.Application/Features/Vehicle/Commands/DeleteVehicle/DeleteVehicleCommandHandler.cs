using MediatR;
using RentalManagementSystem.Application.ServiceContracts;

namespace RentalManagementSystem.Application.Features.Vehicle.Commands.DeleteVehicle
{
    public class DeleteVehicleCommandHandler(IVehicleService vehicleService)
        : IRequestHandler<DeleteVehicleCommand, DeleteVehicleCommandResponse>
    {
        public async Task<DeleteVehicleCommandResponse> Handle(
            DeleteVehicleCommand request,
            CancellationToken cancellationToken)
        {
            return await vehicleService.DeleteVehicleAsync(request, cancellationToken);
        }
    }
}
