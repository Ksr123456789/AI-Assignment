using Microsoft.AspNetCore.Identity;
using RentalManagementSystem.Domain.Constants;
using RentalManagementSystem.Domain.Entities;

namespace RentalManagementSystem.Infrastructure.Seeder
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager)
        {
            const string adminEmail = "admin@rentalmanagement.com";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var adminUser = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "System Administrator",
                    PhoneNumber = "9876543210",
                    PhoneNumberConfirmed = true,
                    DrivingLicenseNumber = "ADMIN12345",
                    LicenseExpiryDate = DateTime.UtcNow.AddYears(5),
                    Address = "123 Admin Headquarters, Suite 100"
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@12345");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
                }
            }
        }
    }
}
