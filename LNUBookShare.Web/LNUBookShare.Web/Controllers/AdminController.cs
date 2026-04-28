using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IAdminUserService _adminService;
    private readonly IAdminBookService _adminBookService;
    private readonly IAdminReviewService _adminReviewService;

    public AdminController(
        IAdminUserService adminService,
        IAdminBookService adminBookService,
        IAdminReviewService adminReviewService)
    {
        _adminService = adminService;
        _adminBookService = adminBookService;
        _adminReviewService = adminReviewService;
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
    public async Task<IActionResult> BlockUser(int userId)
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
    public async Task<IActionResult> UnblockUser(int userId)
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


    public async Task<IActionResult> Reviews(
        string? searchBy = null, string? query = null)
    {
        var result = await _adminReviewService.GetAllReviewsAsync(searchBy, query);

        ViewBag.CurrentQuery = query;
        ViewBag.CurrentSearchBy = searchBy ?? "comment";

        if (result.IsFailure)
        {
            ViewBag.Error = result.Error;
            return View(new List<BookReview>());
        }

        return View(result.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReview(int reviewId)
    {
        var result = await _adminReviewService.DeleteReviewAsync(reviewId);

        if (result.IsSuccess)
            TempData["SuccessMessage"] = "Коментар успішно видалено.";
        else
            TempData["ErrorMessage"] = result.Error;

        return RedirectToAction(nameof(Reviews));
    }

    [HttpGet]
    public async Task<IActionResult> AdminSearchReviews(
        string searchBy, string query)
    {
        var result = await _adminReviewService.GetAllReviewsAsync(searchBy, query);

        ViewBag.CurrentQuery = query;
        ViewBag.CurrentSearchBy = searchBy;

        if (result.IsFailure)
            return RedirectToAction(nameof(Reviews));

        return View("Reviews", result.Value);
    }
}