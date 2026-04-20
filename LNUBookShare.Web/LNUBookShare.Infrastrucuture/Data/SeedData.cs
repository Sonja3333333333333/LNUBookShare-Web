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
                Console.WriteLine("ПОПЕРЕДЖЕННЯ: Дані адміна (Email або Пароль) не знайдені в конфігурації. Адміна не створено.");
                return;
            }

            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new Role { Name = "Admin" });
            }

            var existingFaculty = await dbContext.Faculties.FirstOrDefaultAsync();

            if (existingFaculty == null)
            {
                Console.WriteLine("ПОМИЛКА: В базі немає жодного факультету! Додайте хоча б один.");
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
                    Console.WriteLine($"СУПЕР-АДМІНА ({adminEmail}) УСПІШНО СТВОРЕНО!");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"ПОМИЛКА СТВОРЕННЯ АДМІНА: {error.Description}");
                    }
                }
            }
        }
    }
}