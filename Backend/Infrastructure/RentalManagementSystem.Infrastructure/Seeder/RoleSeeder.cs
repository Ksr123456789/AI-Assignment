using Microsoft.AspNetCore.Identity;
using RentalManagementSystem.Domain.Constants;

namespace RentalManagementSystem.Infrastructure.Seeder
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
        {
            string[] roles = { UserRoles.Admin, UserRoles.Customer };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid> { Name = role });
                }
            }
        }
    }
}
