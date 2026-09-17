using MediatR;
using RentalManagementSystem.Application.ServiceContracts;

namespace RentalManagementSystem.Application.Features.Vehicle.Commands.UpdateVehicle
{
    public class UpdateVehicleCommandHandler(IVehicleService vehicleService)
        : IRequestHandler<UpdateVehicleCommand, UpdateVehicleCommandResponse>
    {
        public async Task<UpdateVehicleCommandResponse> Handle(
            UpdateVehicleCommand request,
            CancellationToken cancellationToken)
        {
            return await vehicleService.UpdateVehicleAsync(request, cancellationToken);
        }
    }
}
