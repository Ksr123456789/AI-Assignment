using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManagementSystem.Application.Features.Booking.Commands.BookVehicle;
using RentalManagementSystem.Application.Features.Booking.Commands.UpdateBookingStatus;
using RentalManagementSystem.Application.Features.Booking.Queries.GetPagedBooking;
using RentalManagementSystem.Application.Features.Booking.Queries.MyPagedRentals;
using RentalManagementSystem.Domain.Constants;

namespace RentalManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingController(ISender mediator) : ControllerBase
    {
        [HttpGet("GetPagedBooking")]
        [Authorize(Roles = $"{UserRoles.Admin}")]
        public async Task<IActionResult> GetPagedBooking(
            [FromQuery] GetPagedBookingQuery query,
            CancellationToken cancellationToken)
        {
            var response = await mediator.Send(query, cancellationToken);
            return Ok(response);
        }

        [HttpGet("GetMyBookings")]
        [Authorize(Roles = $"{UserRoles.Customer},{UserRoles.Admin}")]
        public async Task<IActionResult> GetMyBookings(
            [FromQuery] MyPagedRentalsQuery query,
            CancellationToken cancellationToken)
        {
            var response = await mediator.Send(query, cancellationToken);
            return Ok(response);
        }

        [HttpPut("UpdateBookingStatus")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Customer}")]
        public async Task<IActionResult> UpdateBookingStatus(
            [FromBody] UpdateBookingStatusCommand command,
            CancellationToken cancellationToken)
        {
            var response = await mediator.Send(command, cancellationToken);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("BookVehicle")]
        [Authorize(Roles = $"{UserRoles.Customer},{UserRoles.Admin}")]
        public async Task<IActionResult> BookVehicle(
            [FromBody] BookVehicleCommand command,
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
