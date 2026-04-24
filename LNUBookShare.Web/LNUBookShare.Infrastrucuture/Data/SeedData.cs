using LNUBookShare.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LNUBookShare.Infrastructure.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var dbContext = serviceProvider.GetRequiredService<AppDbContext>();

            var config = serviceProvider.GetRequiredService<IConfiguration>();

            var adminEmail = config["AdminSettings:Email"];
            var adminPassword = config["AdminSettings:Password"];
            var adminFirstName = config["AdminSettings:FirstName"] ?? "Головний";
            var adminLastName = config["AdminSettings:LastName"] ?? "Адміністратор";

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                Console.WriteLine("Admin data not found");
                return;
            }

            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new Role { Name = "Admin" });
            }

            var existingFaculty = await dbContext.Faculties.FirstOrDefaultAsync();

            if (existingFaculty == null)
            {
                Console.WriteLine("No faculty was found");
                return;
            }

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = adminFirstName,
                    LastName = adminLastName,
                    EmailConfirmed = true,
                    FacultyId = existingFaculty.FacultyId,
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine($"admin ({adminEmail}) created succesfully");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error while creating admin: {error.Description}");
                    }
                }
            }
        }
    }
}