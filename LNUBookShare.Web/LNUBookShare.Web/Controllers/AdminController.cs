using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IAdminUserService _adminService;
    private readonly IAdminBookService _adminBookService;
    private readonly IAdminReviewService _adminReviewService;
    private readonly IAdminReportService _adminReportService;
    private readonly IAdminTransactionService _adminTransactionService;

    public AdminController(IAdminUserService adminService, IAdminBookService adminBookService, IAdminReportService adminReportService, IAdminReviewService adminReviewService, IAdminTransactionService adminTransactionService)
    {
        _adminService = adminService;
        _adminBookService = adminBookService;
        _adminReportService = adminReportService;
        _adminReviewService = adminReviewService;
        _adminTransactionService = adminTransactionService;
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
    public async Task<IActionResult> Transactions(string? searchBy = "book", string? query = null, string? sortBy = "date_desc", string? statusFilter = "active", string? termFilter = "all")
    {
        var result = await _adminTransactionService.GetTransactionsAsync(searchBy, query, sortBy, statusFilter, termFilter);

        ViewBag.CurrentSearchBy = searchBy;
        ViewBag.CurrentQuery = query;
        ViewBag.CurrentSortBy = sortBy;
        ViewBag.CurrentStatusFilter = statusFilter;
        ViewBag.CurrentTermFilter = termFilter;

        if (result.IsFailure)
        {
            ViewBag.Error = result.Error;
            return View(new List<RentalTransaction>());
        }

        return View(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> Reports(string query, string searchBy = "sender", string sortBy = "date", string statusFilter = "active", string? reasonFilter = null)
    {
        var result = await _adminReportService.GetReportsAsync(query, searchBy, sortBy, statusFilter, reasonFilter);

        if (result.IsFailure)
        {
            ViewBag.Error = result.Error;
            return View(new List<UserReport>());
        }

        ViewBag.CurrentQuery = query;
        ViewBag.CurrentSearchBy = searchBy;
        ViewBag.CurrentSortBy = sortBy;
        ViewBag.CurrentStatusFilter = statusFilter;
        ViewBag.CurrentReasonFilter = reasonFilter;

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
        string? searchBy = null, string? query = null, int? ratingFilter = null)
    {
        var result = await _adminReviewService.GetAllReviewsAsync(searchBy, query, ratingFilter);

        ViewBag.CurrentQuery = query;
        ViewBag.CurrentSearchBy = searchBy ?? "comment";
        ViewBag.CurrentRatingFilter = ratingFilter;

        if (result.IsFailure)
        {
            ViewBag.Error = result.Error;
            return View(new List<BookReview>());
        }

        return View(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> SearchReports(string searchBy, string query, string sortBy, string statusFilter, string? reasonFilter)
    {
        var result = await _adminReportService.GetReportsAsync(query, searchBy, sortBy, statusFilter, reasonFilter);

        if (result.IsFailure)
        {
            return RedirectToAction(nameof(Reports));
        }

        ViewBag.CurrentQuery = query;
        ViewBag.CurrentSearchBy = searchBy;
        ViewBag.CurrentSortBy = sortBy;
        ViewBag.CurrentStatusFilter = statusFilter;
        ViewBag.CurrentReasonFilter = reasonFilter;

        return View("Reports", result.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReview(int reviewId)
    {
        var result = await _adminReviewService.DeleteReviewAsync(reviewId);

        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Коментар успішно видалено.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error;
        }

        return RedirectToAction(nameof(Reviews));
    }

    [HttpGet]
    public async Task<IActionResult> AdminSearchReviews(
        string searchBy, string query, int? ratingFilter = null)
    {
        var result = await _adminReviewService.GetAllReviewsAsync(searchBy, query, ratingFilter);

        ViewBag.CurrentQuery = query;
        ViewBag.CurrentSearchBy = searchBy;
        ViewBag.CurrentRatingFilter = ratingFilter;

        if (result.IsFailure)
        {
            return RedirectToAction(nameof(Reviews));
        }

        return View("Reviews", result.Value);
    }

    [HttpPost]
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