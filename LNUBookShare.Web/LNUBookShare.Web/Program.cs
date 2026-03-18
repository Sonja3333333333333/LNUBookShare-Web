// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Infrastructure.Repositories;
using LNUBookShare.Infrastructure;
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

    // Add services to the container.
    builder.Services.AddControllersWithViews();
    builder.Services.AddScoped<IBookRepository, BookRepository>();
    builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
    var app = builder.Build();

    // ДОДАНО: Додаємо middleware для гарного логування всіх HTTP-запитів
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

    app.UseAuthorization();

    app.MapStaticAssets();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

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