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
    [Authorize(Roles = $"{UserRoles.Admin}")]
    public class VehicleController(ISender mediator) : ControllerBase
    {
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

        [HttpGet("GetPagedVehicle")]
        public async Task<IActionResult> GetPagedVehicle(
            [FromQuery] GetPagedVehicleQuery query,
            CancellationToken cancellationToken)
        {
            var response = await mediator.Send(query, cancellationToken);
            return Ok(response);
        }

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
