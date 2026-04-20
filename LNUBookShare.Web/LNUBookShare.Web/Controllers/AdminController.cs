using LNUBookShare.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IAdminUserService _adminService;

    public AdminController(IAdminUserService adminService)
    {
        _adminService = adminService;
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
}