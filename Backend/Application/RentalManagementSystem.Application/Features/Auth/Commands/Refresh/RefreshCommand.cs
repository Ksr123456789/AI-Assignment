using System.ComponentModel.DataAnnotations;
using MediatR;

namespace RentalManagementSystem.Application.Features.Auth.Commands.Refresh
{
    public class RefreshCommand : IRequest<RefreshCommandResponse>
    {
        [Required(ErrorMessage = "Access token is required.")]
        public string AccessToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "Refresh token is required.")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
