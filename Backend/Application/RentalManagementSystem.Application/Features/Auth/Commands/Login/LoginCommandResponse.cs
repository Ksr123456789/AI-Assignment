namespace RentalManagementSystem.Application.Features.Auth.Commands.Login
{
    public class LoginCommandResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public Guid? UserId { get; set; }
        public string? Email { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public IEnumerable<string> Errors { get; set; } = new List<string>();
    }
}
