using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Domain.Entities;
using RentalManagementSystem.Domain.Enums;
using RentalManagementSystem.Infrastructure.DbContext;

namespace RentalManagementSystem.Infrastructure.Seeder
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(
            AppDbContext context,
            RoleManager<IdentityRole<Guid>> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            // 1. Seed Roles and Admin
            await RoleSeeder.SeedRolesAsync(roleManager);
            await AdminSeeder.SeedAdminAsync(userManager);

            // 2. Seed Vehicle Categories
            if (!await context.VehicleCategories.AnyAsync())
            {
                var categories = new List<VehicleCategory>
                {
                    new VehicleCategory { CategoryName = "Economy" },
                    new VehicleCategory { CategoryName = "Sedan" },
                    new VehicleCategory { CategoryName = "SUV" },
                    new VehicleCategory { CategoryName = "Luxury" },
                    new VehicleCategory { CategoryName = "Van/Bus" }
                };

                await context.VehicleCategories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // 3. Seed Rental Companies (At least 5)
            if (!await context.RentalCompanies.AnyAsync())
            {
                var companies = new List<RentalCompany>
                {
                    new RentalCompany
                    {
                        Id = Guid.NewGuid(),
                        CompanyCode = "CMP-001",
                        CompanyName = "Swift Motion Rentals",
                        CompanyType = CompanyType.Budget,
                        OwnerName = "John Doe",
                        Phone = "9876543211",
                        HeadquartersLocation = "12 Downtown Blvd, New York, NY",
                        LicenseNumber = "LIC-NY-1001",
                        Status = CompanyStatus.Active,
                        CreatedBy = "System",
                        CreatedDate = DateTime.UtcNow
                    },
                    new RentalCompany
                    {
                        Id = Guid.NewGuid(),
                        CompanyCode = "CMP-002",
                        CompanyName = "Metro Drive Fleet",
                        CompanyType = CompanyType.Economy,
                        OwnerName = "Jane Smith",
                        Phone = "9876543212",
                        HeadquartersLocation = "45 Metro Center, Chicago, IL",
                        LicenseNumber = "LIC-IL-2002",
                        Status = CompanyStatus.Active,
                        CreatedBy = "System",
                        CreatedDate = DateTime.UtcNow
                    },
                    new RentalCompany
                    {
                        Id = Guid.NewGuid(),
                        CompanyCode = "CMP-003",
                        CompanyName = "Apex Prime Motors",
                        CompanyType = CompanyType.Premium,
                        OwnerName = "Robert Taylor",
                        Phone = "9876543213",
                        HeadquartersLocation = "78 Silicon Ave, San Jose, CA",
                        LicenseNumber = "LIC-CA-3003",
                        Status = CompanyStatus.Active,
                        CreatedBy = "System",
                        CreatedDate = DateTime.UtcNow
                    },
                    new RentalCompany
                    {
                        Id = Guid.NewGuid(),
                        CompanyCode = "CMP-004",
                        CompanyName = "Royal Elite Luxury Cars",
                        CompanyType = CompanyType.Luxury,
                        OwnerName = "Eleanor Vance",
                        Phone = "9876543214",
                        HeadquartersLocation = "90 Beverly Hills Way, Los Angeles, CA",
                        LicenseNumber = "LIC-CA-4004",
                        Status = CompanyStatus.Active,
                        CreatedBy = "System",
                        CreatedDate = DateTime.UtcNow
                    },
                    new RentalCompany
                    {
                        Id = Guid.NewGuid(),
                        CompanyCode = "CMP-005",
                        CompanyName = "Horizon Roadlines",
                        CompanyType = CompanyType.Economy,
                        OwnerName = "Michael Brown",
                        Phone = "9876543215",
                        HeadquartersLocation = "22 Coastal Hwy, Miami, FL",
                        LicenseNumber = "LIC-FL-5005",
                        Status = CompanyStatus.Active,
                        CreatedBy = "System",
                        CreatedDate = DateTime.UtcNow
                    }
                };

                await context.RentalCompanies.AddRangeAsync(companies);
                await context.SaveChangesAsync();
            }

            // 4. Seed Vehicles
            if (!await context.Vehicles.AnyAsync())
            {
                var companies = await context.RentalCompanies.ToListAsync();
                var categories = await context.VehicleCategories.ToDictionaryAsync(c => c.CategoryName, c => c.Id);

                var vehicles = new List<Vehicle>
                {
                    new Vehicle
                    {
                        RentalCompanyId = companies[0].Id,
                        VehicleCategoryId = categories["Economy"],
                        MakeAndModel = "Toyota Yaris",
                        LicensePlate = "NY-ECO-101",
                        RegistrationNumber = "REG-NY-101",
                        YearOfManufacture = 2022,
                        SeatingCapacity = 5,
                        DailyRentalRate = 35.00m,
                        AvailabilityStatus = VehicleAvailabilityStatus.Available,
                        Mileage = 25000,
                        CreatedBy = "System",
                        CreatedDate = DateTime.UtcNow
                    },
                    new Vehicle
                    {
                        RentalCompanyId = companies[1].Id,
                        VehicleCategoryId = categories["Sedan"],
                        MakeAndModel = "Honda Civic",
                        LicensePlate = "IL-SED-202",
                        RegistrationNumber = "REG-IL-202",
                        YearOfManufacture = 2023,
                        SeatingCapacity = 5,
                        DailyRentalRate = 50.00m,
                        AvailabilityStatus = VehicleAvailabilityStatus.Available,
                        Mileage = 18000,
                        CreatedBy = "System",
                        CreatedDate = DateTime.UtcNow
                    },
                    new Vehicle
                    {
                        RentalCompanyId = companies[2].Id,
                        VehicleCategoryId = categories["SUV"],
                        MakeAndModel = "Toyota RAV4",
                        LicensePlate = "CA-SUV-303",
                        RegistrationNumber = "REG-CA-303",
                        YearOfManufacture = 2023,
                        SeatingCapacity = 5,
                        DailyRentalRate = 75.00m,
                        AvailabilityStatus = VehicleAvailabilityStatus.Available,
                        Mileage = 15000,
                        CreatedBy = "System",
                        CreatedDate = DateTime.UtcNow
                    },
                    new Vehicle
                    {
                        RentalCompanyId = companies[3].Id,
                        VehicleCategoryId = categories["Luxury"],
                        MakeAndModel = "Mercedes-Benz S-Class",
                        LicensePlate = "CA-LUX-404",
                        RegistrationNumber = "REG-CA-404",
                        YearOfManufacture = 2024,
                        SeatingCapacity = 5,
                        DailyRentalRate = 220.00m,
                        AvailabilityStatus = VehicleAvailabilityStatus.Available,
                        Mileage = 5000,
                        CreatedBy = "System",
                        CreatedDate = DateTime.UtcNow
                    },
                    new Vehicle
                    {
                        RentalCompanyId = companies[4].Id,
                        VehicleCategoryId = categories["Van/Bus"],
                        MakeAndModel = "Ford Transit Passenger",
                        LicensePlate = "FL-VAN-505",
                        RegistrationNumber = "REG-FL-505",
                        YearOfManufacture = 2022,
                        SeatingCapacity = 12,
                        DailyRentalRate = 120.00m,
                        AvailabilityStatus = VehicleAvailabilityStatus.Available,
                        Mileage = 32000,
                        CreatedBy = "System",
                        CreatedDate = DateTime.UtcNow
                    }
                };

                await context.Vehicles.AddRangeAsync(vehicles);
                await context.SaveChangesAsync();
            }
        }
    }
}
