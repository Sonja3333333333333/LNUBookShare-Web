using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
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
}