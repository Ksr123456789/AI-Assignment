using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManagementSystem.Application.Features.Vehicle.Commands.AddVehicle;
using RentalManagementSystem.Application.Features.Vehicle.Commands.DeleteVehicle;
using RentalManagementSystem.Application.Features.Vehicle.Commands.UpdateVehicle;
using RentalManagementSystem.Application.Features.Vehicle.Queries.GetPagedVehicle;
using RentalManagementSystem.Domain.Constants;

namespace RentalManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController(ISender mediator) : ControllerBase
    {
        [Authorize(Roles = $"{UserRoles.Admin}")]
        [HttpPost]
        public async Task<IActionResult> AddVehicle(
            [FromBody] AddVehicleCommand command,
            CancellationToken cancellationToken)
        {
            var response = await mediator.Send(command, cancellationToken);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [Authorize(Roles = $"{UserRoles.Admin}")]
        [HttpPut]
        public async Task<IActionResult> UpdateVehicle(
            [FromBody] UpdateVehicleCommand command,
            CancellationToken cancellationToken)
        {
            var response = await mediator.Send(command, cancellationToken);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [Authorize(Roles = $"{UserRoles.Admin}, {UserRoles.Customer}")]
        [HttpGet("GetPagedVehicle")]
        public async Task<IActionResult> GetPagedVehicle(
            [FromQuery] GetPagedVehicleQuery query,
            CancellationToken cancellationToken)
        {
            var response = await mediator.Send(query, cancellationToken);
            return Ok(response);
        }
        [Authorize(Roles = $"{UserRoles.Admin}")]

        [HttpDelete]
        public async Task<IActionResult> DeleteVehicle(
            [FromQuery] DeleteVehicleCommand command,
            CancellationToken cancellationToken)
        {
            var response = await mediator.Send(command, cancellationToken);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
