using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RentalManagementSystem.Application.Features.Auth.Commands.Login;
using RentalManagementSystem.Application.Features.Auth.Commands.Logout;
using RentalManagementSystem.Application.Features.Auth.Commands.Refresh;
using RentalManagementSystem.Application.Features.Auth.Commands.Register;

namespace RentalManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(ISender mediator) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(command, cancellationToken);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(command, cancellationToken);
            if (!response.Success)
            {
                return Unauthorized(response);
            }

            return Ok(response);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutCommand? command, CancellationToken cancellationToken)
        {
            command ??= new LogoutCommand();

            if (!command.UserId.HasValue && User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdClaim, out var userId))
                {
                    command.UserId = userId;
                }
                command.Email ??= User.FindFirst(ClaimTypes.Email)?.Value;
            }

            var response = await mediator.Send(command, cancellationToken);
            return Ok(response);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshCommand command, CancellationToken cancellationToken)
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
