using System.ComponentModel.DataAnnotations;
using MediatR;

namespace RentalManagementSystem.Application.Features.Auth.Commands.Login
{
    public class LoginCommand : IRequest<LoginCommandResponse>
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }
}
