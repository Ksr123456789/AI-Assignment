namespace RentalManagementSystem.Application.ServiceContracts
{
    public interface ICurrentUser
    {
        Guid? UserId { get; }
        string? Email { get; }
        bool IsAuthenticated { get; }
        string? Role { get; }
        bool IsInRole(string role);
    }
}
