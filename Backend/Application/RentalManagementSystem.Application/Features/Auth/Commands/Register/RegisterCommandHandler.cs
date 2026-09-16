using MediatR;
using RentalManagementSystem.Application.ServiceContracts;

namespace RentalManagementSystem.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler(IAuthService authService) : IRequestHandler<RegisterCommand, RegisterCommandResponse>
    {

        public async Task<RegisterCommandResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            return await authService.RegisterAsync(request, cancellationToken);
        }
    }
}
