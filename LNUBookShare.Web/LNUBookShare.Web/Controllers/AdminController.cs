using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

public class AdminController : Controller
{
    private readonly IAdminUserService _adminService;
    private readonly IAdminBookService _adminBookService;
    private readonly IAdminReportService _adminReportService;

    public AdminController(IAdminUserService adminService, IAdminBookService adminBookService, IAdminReportService adminReportService)
    {
        _adminService = adminService;
        _adminBookService = adminBookService;
        _adminReportService = adminReportService;
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
    public async Task<IActionResult> Reports(string query, string searchBy = "sender", string sortBy = "date", string statusFilter = "active")
    {
        var result = await _adminReportService.GetReportsAsync(query, searchBy, sortBy, statusFilter);

        if (result.IsFailure)
        {
            ViewBag.Error = result.Error;
            return View(new List<UserReport>());
        }

        ViewBag.CurrentQuery = query;
        ViewBag.CurrentSearchBy = searchBy;
        ViewBag.CurrentSortBy = sortBy;
        ViewBag.CurrentStatusFilter = statusFilter;

        return View(result.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBook(int bookId)
    {
        var result = await _adminBookService.DeleteBookAsync(bookId);

        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Оголошення успішно видалено.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error;
        }

        return RedirectToAction(nameof(Books));
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

    [HttpPost]
    public async Task<IActionResult> BlockUser(int userId) // Якщо у вас ID це рядок, зміни тип на string
    {
        var result = await _adminService.BlockUserAsync(userId);
        if (result.IsSuccess)
        {
            TempData["Success"] = "Користувача успішно заблоковано.";
        }
        else
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    public async Task<IActionResult> UnblockUser(int userId) // Якщо у вас ID це рядок, зміни тип на string
    {
        var result = await _adminService.UnblockUserAsync(userId);
        if (result.IsSuccess)
        {
            TempData["Success"] = "Користувача успішно розблоковано.";
        }
        else
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    public async Task<IActionResult> SearchReports(string searchBy, string query, string sortBy, string statusFilter)
    {
        var result = await _adminReportService.GetReportsAsync(query, searchBy, sortBy, statusFilter);

        if (result.IsFailure)
        {
            return RedirectToAction(nameof(Reports));
        }

        ViewBag.CurrentQuery = query;
        ViewBag.CurrentSearchBy = searchBy;
        ViewBag.CurrentSortBy = sortBy;
        ViewBag.CurrentStatusFilter = statusFilter;

        return View("Reports", result.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResolveReport(int reportId)
    {
        var result = await _adminReportService.ResolveReportAsync(reportId);

        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Скаргу успішно вирішено.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error;
        }

        return RedirectToAction(nameof(Reports));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReport(int reportId)
    {
        var result = await _adminReportService.DeleteReportAsync(reportId);

        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Скаргу успішно видалено.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error;
        }

        return RedirectToAction(nameof(Reports));
    }
}