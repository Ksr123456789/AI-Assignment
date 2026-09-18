using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RentalManagementSystem.Application.Features.Auth.Commands.Login;
using RentalManagementSystem.Application.Features.Auth.Commands.Logout;
using RentalManagementSystem.Application.Features.Auth.Commands.Refresh;
using RentalManagementSystem.Application.Features.Auth.Commands.Register;
using RentalManagementSystem.Application.ServiceContracts;
using RentalManagementSystem.Domain.Constants;
using RentalManagementSystem.Domain.Entities;

namespace RentalManagementSystem.Application.Services
{
    public class AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration) : IAuthService
    {
       
        public async Task<RegisterCommandResponse> RegisterAsync(RegisterCommand command, CancellationToken cancellationToken = default)
        {
            if (command.LicenseExpiryDate <= DateTime.UtcNow.Date)
            {
                return new RegisterCommandResponse
                {
                    Success = false,
                    Message = "License Expiry Date must be a future date.",
                    Errors = new[] { "License expiry date must be in the future." }
                };
            }

            var existingUser = await userManager.FindByEmailAsync(command.Email);
            if (existingUser != null)
            {
                return new RegisterCommandResponse
                {
                    Success = false,
                    Message = "A user with this email address already exists.",
                    Errors = new[] { "Email already registered." }
                };
            }

            var existingLicense = await userManager.Users.AnyAsync(u => u.DrivingLicenseNumber == command.DrivingLicenseNumber, cancellationToken);
            if (existingLicense)
            {
                return new RegisterCommandResponse
                {
                    Success = false,
                    Message = "A user with this driving license number already exists.",
                    Errors = new[] { "Driving license number already registered." }
                };
            }

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = command.Email,
                Email = command.Email,
                FullName = command.FullName,
                PhoneNumber = command.PhoneNumber,
                DrivingLicenseNumber = command.DrivingLicenseNumber,
                LicenseExpiryDate = command.LicenseExpiryDate,
                Address = command.Address,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, command.Password);
            if (!result.Succeeded)
            {
                return new RegisterCommandResponse
                {
                    Success = false,
                    Message = "User registration failed.",
                    Errors = result.Errors.Select(e => e.Description)
                };
            }

            await userManager.AddToRoleAsync(user, UserRoles.Customer);

            return new RegisterCommandResponse
            {
                Success = true,
                Message = "User registered successfully.",
                UserId = user.Id
            };
        }

        public async Task<LoginCommandResponse> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByEmailAsync(command.Email);
            if (user == null)
            {
                return new LoginCommandResponse
                {
                    Success = false,
                    Message = "Invalid email or password.",
                    Errors = new[] { "Invalid credentials." }
                };
            }

            var passwordValid = await userManager.CheckPasswordAsync(user, command.Password);
            if (!passwordValid)
            {
                return new LoginCommandResponse
                {
                    Success = false,
                    Message = "Invalid email or password.",
                    Errors = new[] { "Invalid credentials." }
                };
            }

            var roles = await userManager.GetRolesAsync(user);
            var (accessToken, expiresAt) = GenerateJwtToken(user, roles);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await userManager.UpdateAsync(user);

            return new LoginCommandResponse
            {
                Success = true,
                Message = "Login successful.",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                UserId = user.Id,
                Email = user.Email,
                Roles = roles
            };
        }

        public async Task<LogoutCommandResponse> LogoutAsync(LogoutCommand command, CancellationToken cancellationToken = default)
        {
            ApplicationUser? user = null;

            if (command.UserId.HasValue && command.UserId.Value != Guid.Empty)
            {
                user = await userManager.FindByIdAsync(command.UserId.Value.ToString());
            }
            else if (!string.IsNullOrWhiteSpace(command.Email))
            {
                user = await userManager.FindByEmailAsync(command.Email);
            }

            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await userManager.UpdateAsync(user);
            }

            return new LogoutCommandResponse
            {
                Success = true,
                Message = "Logout successful."
            };
        }

        public async Task<RefreshCommandResponse> RefreshAsync(RefreshCommand command, CancellationToken cancellationToken = default)
        {
            var principal = GetPrincipalFromExpiredToken(command.AccessToken);
            if (principal == null)
            {
                return new RefreshCommandResponse
                {
                    Success = false,
                    Message = "Invalid access token.",
                    Errors = new[] { "Could not parse principal from access token." }
                };
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var emailClaim = principal.FindFirst(ClaimTypes.Email)?.Value;

            ApplicationUser? user = null;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                user = await userManager.FindByIdAsync(userId.ToString());
            }
            else if (!string.IsNullOrWhiteSpace(emailClaim))
            {
                user = await userManager.FindByEmailAsync(emailClaim);
            }

            if (user == null || user.RefreshToken != command.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return new RefreshCommandResponse
                {
                    Success = false,
                    Message = "Invalid or expired refresh token.",
                    Errors = new[] { "Refresh token is invalid or has expired." }
                };
            }

            var roles = await userManager.GetRolesAsync(user);
            var (newAccessToken, expiresAt) = GenerateJwtToken(user, roles);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await userManager.UpdateAsync(user);

            return new RefreshCommandResponse
            {
                Success = true,
                Message = "Token refreshed successfully.",
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = expiresAt
            };
        }

        private (string token, DateTime expiresAt) GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var jwtKey = configuration["Jwt:Key"] ?? "Default_Secret_Key_Rental_Management_System_2026_Secure_Key!";
            var jwtIssuer = configuration["Jwt:Issuer"] ?? "RentalManagementSystem";
            var jwtAudience = configuration["Jwt:Audience"] ?? "RentalManagementSystemClient";
            var durationInMinutes = double.TryParse(configuration["Jwt:DurationInMinutes"], out var minutes) ? minutes : 60;

            var expiresAt = DateTime.UtcNow.AddMinutes(durationInMinutes);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName ?? user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresAt,
                Issuer = jwtIssuer,
                Audience = jwtAudience,
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return (tokenHandler.WriteToken(token), expiresAt);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var jwtKey = configuration["Jwt:Key"] ?? "Default_Secret_Key_Rental_Management_System_2026_Secure_Key!";
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
