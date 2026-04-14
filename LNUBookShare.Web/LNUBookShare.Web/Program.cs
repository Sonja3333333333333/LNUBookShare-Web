// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Infrastructure;
using LNUBookShare.Infrastructure.Repositories;
using LNUBookShare.Infrastructure.Services;
using LNUBookShare.Web.Hubs;
using LNUBookShare.Web.Services; // Додав для нашого нового сервісу сповіщень
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

// --- SERILOG CONFIGURATION ---
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Запуск веб-додатка LNU Book Share!");

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    // --- DATABASE ---
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseNpgsql(connectionString);
    });

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

    // --- REPOSITORIES & SERVICES REGISTRATION ---

    // Infrastructure & Core
    builder.Services.AddScoped<IFacultyRepository, FacultyRepository>();
    builder.Services.AddScoped<IFacultyService, FacultyService>();
    builder.Services.AddTransient<IEmailService, EmailService>();
    builder.Services.AddScoped<IPhotoService, PhotoService>();

    // Books logic
    builder.Services.AddScoped<IBookRepository, BookRepository>();
    builder.Services.AddScoped<IBookSearchService, BookSearchService>();
    builder.Services.AddScoped<IBookDetailsService, BookDetailsService>();
    builder.Services.AddScoped<IBookStatusService, BookStatusService>();

    // Favorites
    builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
    builder.Services.AddScoped<IFavoriteService, FavoriteService>();

    // Reviews & Rating
    builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
    builder.Services.AddScoped<IReviewService, ReviewService>();

    // Reservation Queue
    builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
    builder.Services.AddScoped<IReservationService, ReservationService>();

    // Profiles
    builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
    builder.Services.AddScoped<IProfileService, ProfileService>();
    builder.Services.AddScoped<IOtherProfileRepository, OtherProfileRepository>();
    builder.Services.AddScoped<IOtherProfileService, OtherProfileService>();

    // Notifications
    builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
    builder.Services.AddScoped<INotificationService, NotificationService>();

    // Системні повідомлення (Чат)
    builder.Services.AddScoped<IChatRepository, ChatRepository>();
    builder.Services.AddScoped<IChatService, ChatService>();

    // ОСЬ ТУТ: Реєстрація архітектурно-правильного сервісу сповіщень
    builder.Services.AddScoped<IChatNotificationService, ChatNotificationService>();

    // --- SIGNALR REGISTRATION ---
    builder.Services.AddSignalR();

    // Додаткові налаштування
    builder.Services.Configure<LNUBookShare.Application.Common.CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

    builder.Services.AddControllersWithViews();

    var app = builder.Build();

    // --- MIDDLEWARE ---
    app.UseSerilogRequestLogging();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }
    else
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseHttpsRedirection();
    app.MapStaticAssets();
    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    // --- SIGNALR ENDPOINTS ---
    app.MapHub<ChatHub>("/chatHub");

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Catalog}/{action=Search}/{id?}")
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