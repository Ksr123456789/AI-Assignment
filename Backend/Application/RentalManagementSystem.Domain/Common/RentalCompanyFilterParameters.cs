using RentalManagementSystem.Domain.Enums;

namespace RentalManagementSystem.Domain.Common
{
    public class RentalCompanyFilterParameters
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "asc";
        public string? SearchBy { get; set; }
        public CompanyStatus? CompanyStatus { get; set; }
        public CompanyType? CompanyType { get; set; }
    }
}
