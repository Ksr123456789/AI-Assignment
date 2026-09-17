using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Domain.Common;
using RentalManagementSystem.Domain.Entities;
using RentalManagementSystem.Domain.RepositoryContracts;
using RentalManagementSystem.Infrastructure.DbContext;

namespace RentalManagementSystem.Infrastructure.Repository
{
    public class RentalCompanyRepository(AppDbContext context) : IRentalCompanyRepository
    {
        public async Task<string> GenerateCompanyCodeAsync(CancellationToken cancellationToken = default)
        {
            var count = await context.RentalCompanies.CountAsync(cancellationToken);
            var code = $"CMP-{(count + 1):D3}";
            while (await context.RentalCompanies.AnyAsync(rc => rc.CompanyCode == code, cancellationToken))
            {
                count++;
                code = $"CMP-{(count + 1):D3}";
            }
            return code;
        }

        public async Task<bool> ExistsByLicenseNumberAsync(string licenseNumber, CancellationToken cancellationToken = default)
        {
            return await context.RentalCompanies.AnyAsync(
                rc => rc.LicenseNumber.ToLower() == licenseNumber.ToLower() && !rc.IsDeleted, 
                cancellationToken);
        }

        public async Task AddRentalCompanyAsync(RentalCompany company, CancellationToken cancellationToken = default)
        {
            await context.RentalCompanies.AddAsync(company, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<RentalCompany>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await context.RentalCompanies
                .AsNoTracking()
                .Where(rc => !rc.IsDeleted)
                .OrderByDescending(rc => rc.CreatedDate)
                .ToListAsync(cancellationToken);
        }


        public async Task<PagedResult<RentalCompany>> GetPagedRentalCompanyAsync(RentalCompanyFilterParameters query)
        {
            var RentalCompany = context.RentalCompanies.Where(x => !x.IsDeleted);

            if (query.CompanyStatus.HasValue)
            {
                RentalCompany = RentalCompany.Where(x => x.Status == query.CompanyStatus);
            }

            if (query.CompanyType.HasValue)
            {
                RentalCompany = RentalCompany.Where(x => x.CompanyType == query.CompanyType);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchBy))
            {
                var search = query.SearchBy.Trim();

                RentalCompany = RentalCompany.Where(x => x.CompanyName.Contains(search) ||
                                                         x.OwnerName.Contains(search) ||
                                                         x.Phone.Contains(search) ||
                                                         x.HeadquartersLocation.Contains(search) ||
                                                         x.CompanyType.ToString().Contains(search) ||
                                                         x.LicenseNumber.Contains(search) ||
                                                         x.Status.ToString().Contains(search));
            }

            RentalCompany = query.SortBy?.ToLower() switch
            {
                "id" => query.SortDirection == "desc" ? RentalCompany.OrderByDescending(x => x.Id) :
                RentalCompany.OrderBy(x => x.Id),
                "companyname" => query.SortDirection == "desc" ? RentalCompany.OrderByDescending(x => x.CompanyName) :
                RentalCompany.OrderBy(x => x.CompanyName),
                "companytype" => query.SortDirection == "desc" ? RentalCompany.OrderByDescending(x => x.CompanyType.ToString()) :
                RentalCompany.OrderBy(x => x.CompanyType.ToString()),
                "ownername" => query.SortDirection == "desc" ? RentalCompany.OrderByDescending(x => x.OwnerName) :
                RentalCompany.OrderBy(x => x.OwnerName),
                "phonenumber" or "phone" => query.SortDirection == "desc" ? RentalCompany.OrderByDescending(x => x.Phone) :
                RentalCompany.OrderBy(x => x.Phone),
                "headquarterslocation" => query.SortDirection == "desc" ? RentalCompany.OrderByDescending(x => x.HeadquartersLocation) :
                RentalCompany.OrderBy(x => x.HeadquartersLocation),
                "licensenumber" => query.SortDirection == "desc" ? RentalCompany.OrderByDescending(x => x.LicenseNumber) :
                RentalCompany.OrderBy(x => x.LicenseNumber),
                "companystatus" => query.SortDirection == "desc" ? RentalCompany.OrderByDescending(x => x.Status.ToString()) :
                RentalCompany.OrderBy(x => x.Status.ToString()),
                _ => RentalCompany.OrderByDescending(x => x.Id)
            };

            var totalCount = await RentalCompany.CountAsync();

            var items = await RentalCompany
                              .Skip((query.PageNumber - 1) * query.PageSize)
                              .Take(query.PageSize)
                              .ToListAsync();

            return new PagedResult<RentalCompany>
            {
                Items = items,
                TotalCount = totalCount,
                PageSize = query.PageSize,
                PageNumber = query.PageNumber,
            };
        }

        public async Task<RentalCompany?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await context.RentalCompanies.FirstOrDefaultAsync(rc => rc.Id == id && !rc.IsDeleted, cancellationToken);
        }

        public async Task UpdateAsync(RentalCompany company, CancellationToken cancellationToken = default)
        {
            context.RentalCompanies.Update(company);
            await context.SaveChangesAsync(cancellationToken);
        }
        public async Task DeleteRentalCompanyAsync(RentalCompany company, CancellationToken cancellationToken = default)
        {
            company.IsDeleted = true;
            context.RentalCompanies.Update(company);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
