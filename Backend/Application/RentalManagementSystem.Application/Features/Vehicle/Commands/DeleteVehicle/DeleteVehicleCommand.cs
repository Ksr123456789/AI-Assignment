using MediatR;

namespace RentalManagementSystem.Application.Features.Vehicle.Commands.DeleteVehicle
{
    public class DeleteVehicleCommand : IRequest<DeleteVehicleCommandResponse>
    {
        public int Id { get; set; }
    }
}
