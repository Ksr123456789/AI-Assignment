namespace RentalManagementSystem.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public IEnumerable<string> Errors { get; set; } = new List<string>();
    }
}
