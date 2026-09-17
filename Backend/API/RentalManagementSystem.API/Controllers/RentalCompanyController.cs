using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManagementSystem.Application.Features.RentalCompany.Commands.AddRentalCompany;
using RentalManagementSystem.Application.Features.RentalCompany.Commands.DeleteRentalCompany;
using RentalManagementSystem.Application.Features.RentalCompany.Commands.UpdateRentalCompany;
using RentalManagementSystem.Application.Features.RentalCompany.Queries.GetAllRentalCompany;
using RentalManagementSystem.Application.Features.RentalCompany.Queries.GetPagedRentalCompany;
using RentalManagementSystem.Domain.Constants;

namespace RentalManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RentalCompanyController(ISender mediator) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> AddRentalCompany(
            [FromBody] AddRentalCompanyCommand command, 
            CancellationToken cancellationToken)
        {
            var response = await mediator.Send(command, cancellationToken);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return CreatedAtAction(nameof(GetAllRentalCompany), new { id = response.Data?.Id }, response);
        }

        [HttpGet("GetPagedRentalCompany")]
        public async Task<IActionResult> GetPagedRentalCompany(
            [FromQuery] GetPagedRentalCompanyQuery query, 
            CancellationToken cancellationToken)
        {
            var response = await mediator.Send( query, cancellationToken);
            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRentalCompany(CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new GetAllRentalCompanyQuery(), cancellationToken);
            return Ok(response);
        }

        [HttpPut()]
        public async Task<IActionResult> UpdateRentalCompany(
             
            [FromQuery] UpdateRentalCompanyCommand command, 
            CancellationToken cancellationToken)
        {
            

            var response = await mediator.Send(command, cancellationToken);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCompany( 
            [FromQuery] DeleteRentalCompanyCommand command,
            CancellationToken cancellationToken)
        {
            var response = await mediator.Send(command , cancellationToken);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
