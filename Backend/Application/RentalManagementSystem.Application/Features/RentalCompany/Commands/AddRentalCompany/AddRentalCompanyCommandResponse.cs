using RentalManagementSystem.Application.Features.RentalCompany.DTOs;

namespace RentalManagementSystem.Application.Features.RentalCompany.Commands.AddRentalCompany
{
    public class AddRentalCompanyCommandResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public RentalCompanyDto? Data { get; set; }
        public IEnumerable<string> Errors { get; set; } = new List<string>();
    }
}
