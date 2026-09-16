using MediatR;
using RentalManagementSystem.Application.ServiceContracts;

namespace RentalManagementSystem.Application.Features.Auth.Commands.Logout
{
    public class LogoutCommandHandler(IAuthService authService) : IRequestHandler<LogoutCommand, LogoutCommandResponse>
    {
        public async Task<LogoutCommandResponse> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            return await authService.LogoutAsync(request, cancellationToken);
        }
    }
}
