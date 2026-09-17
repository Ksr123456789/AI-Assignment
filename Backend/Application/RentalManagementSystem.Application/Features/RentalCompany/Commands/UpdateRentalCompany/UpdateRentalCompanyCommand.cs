using System.ComponentModel.DataAnnotations;
using MediatR;
using RentalManagementSystem.Domain.Enums;

namespace RentalManagementSystem.Application.Features.RentalCompany.Commands.UpdateRentalCompany
{
    public class UpdateRentalCompanyCommand : IRequest<UpdateRentalCompanyCommandResponse>
    {
        public Guid Id { get; set; }        
        public string CompanyName { get; set; } = string.Empty;
        public CompanyType CompanyType { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string HeadquartersLocation { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public CompanyStatus Status { get; set; }

    }
}
