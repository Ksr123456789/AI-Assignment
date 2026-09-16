using MediatR;
using RentalManagementSystem.Application.ServiceContracts;

namespace RentalManagementSystem.Application.Features.Auth.Commands.Refresh
{
    public class RefreshCommandHandler(IAuthService authService) : IRequestHandler<RefreshCommand, RefreshCommandResponse>
    {
        public async Task<RefreshCommandResponse> Handle(RefreshCommand request, CancellationToken cancellationToken)
        {
            return await authService.RefreshAsync(request, cancellationToken);
        }
    }
}
