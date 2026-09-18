using RentalManagementSystem.Domain.Enums;

namespace RentalManagementSystem.Application.Features.RentalCompany.Queries.GetPagedRentalCompany
{
    public class GetPagedRentalCompanyQueryResponse
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public CompanyType CompanyType { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string HeadquartersLocation { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public CompanyStatus Status { get; set; }
        public int VehicleCount { get; set; }
    }
}
