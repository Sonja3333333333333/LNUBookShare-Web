using LNUBookShare.Infrastructure.ExternalServices;
using LNUBookShare.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace LNUBookShare.Web.Controllers;

public class GlobalSearchController : Controller
{
    private readonly OpenLibraryService _openLibraryService;

    public GlobalSearchController(OpenLibraryService openLibraryService)
    {
        _openLibraryService = openLibraryService;
    }

    [HttpGet]
    [IpRateLimit(5)] // Наш кастомний фільтр: макс. 5 пошуків на хвилину
    public async Task<IActionResult> Index(string query)
    {
        // Якщо запит порожній — просто показуємо порожню сторінку пошуку
        if (string.IsNullOrWhiteSpace(query))
        {
            return View(new List<LNUBookShare.Application.DTOs.GlobalBookDto>());
        }

        try
        {
            // Стукаємо в Open Library через наш Typed Client
            var results = await _openLibraryService.SearchEbooksAsync(query);

            // Передаємо результати у в'юшку
            return View(results);
        }
        catch (Exception)
        {
            // Якщо зовнішній сервіс «ліг», редіректимо на помилку, як того вимагає ТЗ
            return RedirectToAction("Error", "Home", new { message = "Зовнішня бібліотека тимчасово недоступна." });
        }
    }
}