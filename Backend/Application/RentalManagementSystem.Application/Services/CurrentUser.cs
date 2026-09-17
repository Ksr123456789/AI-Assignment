using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using RentalManagementSystem.Application.ServiceContracts;

namespace RentalManagementSystem.Application.Services
{
    public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
    {
        private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

        public Guid? UserId
        {
            get
            {
                var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                              ?? User?.FindFirst("sub")?.Value 
                              ?? User?.FindFirst("uid")?.Value;

                return Guid.TryParse(idClaim, out var guid) ? guid : null;
            }
        }

        public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value 
                                ?? User?.FindFirst("email")?.Value;

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

        public string? Role => User?.FindFirst(ClaimTypes.Role)?.Value 
                               ?? User?.FindFirst("role")?.Value;

        public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
    }
}
