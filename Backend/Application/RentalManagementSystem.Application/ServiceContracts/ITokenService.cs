using System.Security.Claims;
using RentalManagementSystem.Domain.Entities;

namespace RentalManagementSystem.Application.ServiceContracts
{
    public interface ITokenService
    {
        (string token, DateTime expiresAt) GenerateAccessToken(ApplicationUser user, IList<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token);
    }
}
