using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LNUBookShare.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<AccountController> _logger;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IEmailService _emailService; // Додано сервіс пошти

    public AccountController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ILogger<AccountController> logger,
        IFacultyRepository facultyRepository,
        IEmailService emailService) // Ін'єкція сервісу
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _facultyRepository = facultyRepository;
        _emailService = emailService;
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

        if (!model.Email.EndsWith("@lnu.edu.ua", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Спроба реєстрації з лівого домену: {Email}", model.Email);
            ModelState.AddModelError("Email", "Дозволена тільки пошта @lnu.edu.ua");
            await LoadFacultiesToViewBag();
            return View(model);
        }

        var user = new User
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            FacultyId = model.FacultyId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            RoleId = 2,
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            _logger.LogInformation("Успішна реєстрація в БД: {Email}", model.Email);

            // --- ГЕНЕРАЦІЯ ЛІНКА ТА ВІДПРАВКА ЛИСТА ---
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token }, Request.Scheme);

            var emailBody = $@"
                <div style='font-family: Arial, sans-serif; border: 1px solid #ddd; padding: 20px; border-radius: 10px;'>
                    <h2 style='color: #007bff;'>Вітаємо у LNU Book Share!</h2>
                    <p>Щоб завершити реєстрацію та почати ділитися книгами, підтвердіть свою пошту:</p>
                    <a href='{confirmationLink}' style='display: inline-block; padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px;'>Підтвердити пошту</a>
                    <p style='margin-top: 20px; font-size: 0.8em; color: #666;'>Якщо ви не реєструвалися на нашому сайті, просто ігноруйте цей лист.</p>
                </div>";

            try
            {
                await _emailService.SendEmailAsync(model.Email, "Підтвердження реєстрації - LNU Book Share", emailBody);
                _logger.LogInformation("Лист підтвердження надіслано на {Email}", model.Email);
                TempData["SuccessMessage"] = "Реєстрація успішна! Перевірте свою пошту @lnu.edu.ua для підтвердження акаунту.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при відправці пошти на {Email}", model.Email);
                TempData["ErrorMessage"] = "Користувач створений, але не вдалося надіслати лист. Зверніться до адміністратора.";
            }

            return RedirectToAction("Login", "Account");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        await LoadFacultiesToViewBag();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (userId == null || token == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound("Користувача не знайдено.");
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Дякуємо! Пошту підтверджено. Тепер ви можете увійти.";
            return RedirectToAction("Login", "Account");
        }

        TempData["ErrorMessage"] = "Помилка підтвердження пошти або лінк застарів.";
        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            // Ручна перевірка підтвердження пошти для виведення зрозумілої помилки
            if (user != null && !await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError(string.Empty, "Пошта ще не підтверджена. Перевірте свою скриньку.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("Користувач {Email} увійшов.", model.Email);
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Неправильна пошта або пароль.");
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("Користувач вийшов із системи.");
        return RedirectToAction("Index", "Home");
    }

    private async Task LoadFacultiesToViewBag()
    {
        var faculties = await _facultyRepository.GetAllAsync();
        ViewBag.Faculties = new SelectList(faculties, "FacultyId", "FacultyName");
    }
}