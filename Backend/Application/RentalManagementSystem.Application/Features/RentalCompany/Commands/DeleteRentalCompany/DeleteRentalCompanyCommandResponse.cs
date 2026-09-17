using RentalManagementSystem.Application.Features.RentalCompany.DTOs;

namespace RentalManagementSystem.Application.Features.RentalCompany.Commands.DeleteRentalCompany
{
    public class DeleteRentalCompanyCommandResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public RentalCompanyDto? Data { get; set; }
        public IEnumerable<string> Errors { get; set; } = new List<string>();
    }
}
