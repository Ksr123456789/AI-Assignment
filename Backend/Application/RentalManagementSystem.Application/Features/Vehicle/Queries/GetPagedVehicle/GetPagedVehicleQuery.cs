using MediatR;
using RentalManagementSystem.Domain.Common;
using RentalManagementSystem.Domain.Enums;

namespace RentalManagementSystem.Application.Features.Vehicle.Queries.GetPagedVehicle
{
    public class GetPagedVehicleQuery : IRequest<PagedResult<GetPagedVehicleQueryResponse>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "asc";
        public string? SearchBy { get; set; }
        public int? VehicleCategoryId { get; set; }
        public VehicleAvailabilityStatus? VehicleAvailabilityStatus { get; set; }
    }
}
