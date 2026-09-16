using MediatR;

namespace RentalManagementSystem.Application.Features.Auth.Commands.Logout
{
    public class LogoutCommand : IRequest<LogoutCommandResponse>
    {
        public Guid? UserId { get; set; }
        public string? Email { get; set; }
    }
}
