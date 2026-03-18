using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using LNUBookShare.Web.Models;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Application.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LNUBookShare.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AccountController> _logger;
    private readonly IFacultyRepository _facultyRepository;

    public AccountController(
        UserManager<User> userManager,
        ILogger<AccountController> logger,
        IFacultyRepository facultyRepository)
    {
        _userManager = userManager;
        _logger = logger;
        _facultyRepository = facultyRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Register()
    {
        await LoadFacultiesToViewBag();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadFacultiesToViewBag();
            return View(model);
        }

        // --- ЛОГІКА ПЕРЕВІРКИ ПОШТИ (Вимога ПМІ) ---
        if (!model.Email.EndsWith("@lnu.edu.ua", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Спроба реєстрації з лівого домену: {Email}", model.Email);
            ModelState.AddModelError("Email", "Дозволена тільки пошта @lnu.edu.ua");
            await LoadFacultiesToViewBag();
            return View(model);
        }

        // Створюємо об'єкт користувача для БД
        var user = new User
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            FacultyId = model.FacultyId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            RoleId = 2 // Наприклад, 2 - це роль студента за замовчуванням
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            _logger.LogInformation("Успішна реєстрація: {Email}", model.Email);
            return RedirectToAction("Index", "Home");
        }

        // Обробка помилок Identity (наприклад, пароль надто простий)
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        await LoadFacultiesToViewBag();
        return View(model);
    }

    private async Task LoadFacultiesToViewBag()
    {
        var faculties = await _facultyRepository.GetAllAsync();
        ViewBag.Faculties = new SelectList(faculties, "FacultyId", "FacultyName");
    }
}