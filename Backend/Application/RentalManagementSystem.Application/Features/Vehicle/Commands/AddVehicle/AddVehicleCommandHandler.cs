using MediatR;
using RentalManagementSystem.Application.ServiceContracts;

namespace RentalManagementSystem.Application.Features.Vehicle.Commands.AddVehicle
{
    public class AddVehicleCommandHandler(IVehicleService vehicleService)
        : IRequestHandler<AddVehicleCommand, AddVehicleCommandResponse>
    {
        public async Task<AddVehicleCommandResponse> Handle(
            AddVehicleCommand request,
            CancellationToken cancellationToken)
        {
            return await vehicleService.AddVehicleAsync(request, cancellationToken);
        }
    }
}
