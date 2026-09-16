using RentalManagementSystem.Application.Features.Auth.Commands.Login;
using RentalManagementSystem.Application.Features.Auth.Commands.Logout;
using RentalManagementSystem.Application.Features.Auth.Commands.Refresh;
using RentalManagementSystem.Application.Features.Auth.Commands.Register;

namespace RentalManagementSystem.Application.ServiceContracts
{
    public interface IAuthService
    {
        Task<RegisterCommandResponse> RegisterAsync(RegisterCommand command, CancellationToken cancellationToken = default);
        Task<LoginCommandResponse> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default);
        Task<LogoutCommandResponse> LogoutAsync(LogoutCommand command, CancellationToken cancellationToken = default);
        Task<RefreshCommandResponse> RefreshAsync(RefreshCommand command, CancellationToken cancellationToken = default);
    }
}
