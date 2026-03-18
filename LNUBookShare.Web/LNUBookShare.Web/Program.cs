using LNUBookShare.Domain.Entities; // ДОДАНО для доступу до User та Role
using LNUBookShare.Infrastructure;
using Microsoft.AspNetCore.Identity; // ДОДАНО
using Microsoft.EntityFrameworkCore;
using Serilog;
using LNUBookShare.Application.Interfaces; // Для IFacultyRepository
using LNUBookShare.Infrastructure.Repositories; // Для FacultyRepository
var builder = WebApplication.CreateBuilder(args);

// Налаштування Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();

builder.Host.UseSerilog();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"DEBUG: Connection String is: {connectionString}");

// Підключення до БД
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// --- ДОДАНО: Реєстрація Identity для твоїх сутностей ---
builder.Services.AddIdentity<User, Role>(options =>
{
    // Полегшуємо вимоги для тестів (ПМІ вимоги)
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// --- ДОДАНО: Реєстрація твоїх репозиторіїв ---
builder.Services.AddScoped<IFacultyRepository, FacultyRepository>();

builder.Services.AddControllersWithViews();

    var app = builder.Build();

app.UseSerilogRequestLogging();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Стандартний метод для статики

app.UseRouting();

// ВАЖЛИВО: Authentication має бути ПЕРЕД Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Додаток не зміг запуститися");
}
finally
{
    Log.CloseAndFlush();
}