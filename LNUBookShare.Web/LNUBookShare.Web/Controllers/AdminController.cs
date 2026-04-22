using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class AdminController : Controller
{
    private readonly IAdminUserService _adminService;
    private readonly IAdminBookService _adminBookService;

    public AdminController(IAdminUserService adminService, IAdminBookService adminBookService)
    {
        _adminService = adminService;
        _adminBookService = adminBookService;
    }

    public async Task<IActionResult> Users()
    {
        var result = await _adminService.GetAllUsersAsync();

        if (result.IsFailure)
        {
            ViewBag.Error = result.Error;
            return View(new List<UserDto>());
        }

        return View(result.Value);
    }

    public async Task<IActionResult> Books()
    {
        var result = await _adminBookService.GetAllBooksAsync();

        if (result.IsFailure)
        {
            ViewBag.Error = result.Error;
            return View(new List<AdminBookDto>());
        }

        return View(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> AdminSearchBooks(string searchBy, string query)
    {
        var result = await _adminBookService.AdminSearchBooksAsync(searchBy, query);

        if (result.IsFailure)
        {
            return RedirectToAction(nameof(Books));
        }

        ViewBag.CurrentQuery = query;
        ViewBag.CurrentSearchBy = searchBy;

        return View("Books", result.Value);
    }
}