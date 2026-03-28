// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Infrastructure;
using LNUBookShare.Infrastructure.Repositories;
using LNUBookShare.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Запуск веб-додатка LNU Book Share!");

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine($"DEBUG: Connection String is: {connectionString}");

    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseNpgsql(connectionString);
    });

    builder.Services.AddScoped<IBookRepository, BookRepository>();

    // --- IDENTITY ---
    builder.Services.AddIdentity<User, Role>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

    builder.Services.AddScoped<IFacultyRepository, FacultyRepository>();
    builder.Services.AddTransient<IEmailService, EmailService>();

    builder.Services.AddControllersWithViews();
    builder.Services.AddScoped<IBookSearchService, BookSearchService>();

    builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
    builder.Services.AddScoped<IFavoriteService, FavoriteService>();

    builder.Services.AddScoped<IBookDetailsService, BookDetailsService>();

    builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
    builder.Services.AddScoped<IReviewService, ReviewService>();

    builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
    builder.Services.AddScoped<IProfileService, ProfileService>();

    var app = builder.Build();

    // --- MIDDLEWARE ---
    app.UseSerilogRequestLogging();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.MapStaticAssets();
    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Catalog}/{action=Search}/{id?}")
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