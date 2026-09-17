using MediatR;
using RentalManagementSystem.Domain.Common;
using RentalManagementSystem.Domain.Enums;

namespace RentalManagementSystem.Application.Features.RentalCompany.Queries.GetPagedRentalCompany
{
    public class GetPagedRentalCompanyQuery : IRequest<PagedResult<GetPagedRentalCompanyQueryResponse>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "asc";
        public string? SearchBy { get; set; }
        public CompanyType? CompanyType { get; set; }
        public CompanyStatus? CompanyStatus { get; set; }
    }
}
