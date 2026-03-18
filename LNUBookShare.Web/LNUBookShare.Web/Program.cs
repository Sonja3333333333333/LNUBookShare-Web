// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using LNUBookShare.Infrastructure;
using Microsoft.AspNetCore.Identity; // ДОДАНО: підключення Identity
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // Дозволяє зчитувати додаткові налаштування з appsettings.json
    .Enrich.FromLogContext()
    .WriteTo.Console() // Логи будуть виводитися в консоль
    .WriteTo.Seq("http://localhost:5341") // Відправка логів у Seq (стандартна адреса)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Запуск веб-додатка LNU Book Share!");

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseNpgsql(connectionString);
    });

    // ДОДАНО: Реєстрація Identity для роботи з користувачами та ролями
    builder.Services.AddIdentity<LNUBookShare.Domain.Entities.User, LNUBookShare.Domain.Entities.Role>(options =>
    {
        // Базові налаштування пароля
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

    // Add services to the container.
    builder.Services.AddControllersWithViews();

    var app = builder.Build();

    // Додаємо middleware для гарного логування всіх HTTP-запитів
    app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");

        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseRouting();

    // ДОДАНО: Аутентифікація має бути обов'язково ПЕРЕД авторизацією
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapStaticAssets();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Додаток не зміг запуститися");
}
finally
{
    await Log.CloseAndFlushAsync();
}