using RentalManagementSystem.Domain.Enums;

namespace RentalManagementSystem.Application.Features.RentalCompany.DTOs
{
    public class RentalCompanyDto
    {
        public Guid Id { get; set; }
        public string CompanyCode { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public CompanyType CompanyType { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string HeadquartersLocation { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public CompanyStatus Status { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int VehicleCount { get; set; }
    }
}
