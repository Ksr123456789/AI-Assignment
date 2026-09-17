using RentalManagementSystem.Application.Features.RentalCompany.DTOs;

namespace RentalManagementSystem.Application.Features.RentalCompany.Queries.GetAllRentalCompany
{
    public class GetAllRentalCompanyQueryResponse
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public List<RentalCompanyDto> Data { get; set; } = new();
    }
}
