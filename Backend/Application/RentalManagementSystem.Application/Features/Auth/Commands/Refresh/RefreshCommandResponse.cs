namespace RentalManagementSystem.Application.Features.Auth.Commands.Refresh
{
    public class RefreshCommandResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public IEnumerable<string> Errors { get; set; } = new List<string>();
    }
}
