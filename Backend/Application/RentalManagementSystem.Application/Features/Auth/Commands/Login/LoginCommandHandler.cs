using MediatR;
using RentalManagementSystem.Application.ServiceContracts;

namespace RentalManagementSystem.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler(IAuthService authService) : IRequestHandler<LoginCommand, LoginCommandResponse>
    {
        public async Task<LoginCommandResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            return await authService.LoginAsync(request, cancellationToken);
        }
    }
}
