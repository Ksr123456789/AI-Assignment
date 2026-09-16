namespace RentalManagementSystem.Application.Features.Auth.Commands.Logout
{
    public class LogoutCommandResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public IEnumerable<string> Errors { get; set; } = new List<string>();
    }
}
