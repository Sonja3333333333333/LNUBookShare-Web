using System;
using System.IO;
using System.Threading.Tasks;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LNUBookShare.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IProfileService _profileService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(
            UserManager<User> userManager,
            IProfileService profileService,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _profileService = profileService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new UserProfileViewModel
            {
                UserId = user.Id,
                FirstName = user.FirstName ?? "Не вказано",
                LastName = user.LastName ?? "Не вказано",
                Email = user.Email ?? "Не вказано",
                FacultyName = "Прикладної математики та інформатики",
                AvatarPath = user.Avatar?.ImagePath,
                MyBooks = await _profileService.GetUserBooksAsync(user.Id),
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddBook(AddBookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Будь ласка, заповніть усі обов'язкові поля.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var book = new Book
            {
                Title = model.Title,
                Author = model.Author,
                Year = model.Year,
                Isbn = model.Isbn,
                Publisher = model.Publisher,
                Language = model.Language,
                CategoryId = model.CategoryId,
                OwnerId = user.Id,
                Status = "available",
                CreatedAt = DateTime.UtcNow,
            };

            if (model.CoverPhoto != null && model.CoverPhoto.Length > 0)
            {
                book.Cover = await SaveImageAsync(model.CoverPhoto);
            }

            var result = await _profileService.AddBookToProfileAsync(book);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Книгу успішно додано!";
            }
            else
            {
                TempData["ErrorMessage"] = result.Error;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> EditBook(int id, AddBookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Перевірте правильність введених даних.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var books = await _profileService.GetUserBooksAsync(user.Id);
            var book = books.Find(b => b.BookId == id);

            if (book == null)
            {
                return NotFound();
            }

            book.Title = model.Title;
            book.Author = model.Author;
            book.Publisher = model.Publisher;
            book.Year = model.Year;
            book.Isbn = model.Isbn;
            book.CategoryId = model.CategoryId;

            if (model.CoverPhoto != null && model.CoverPhoto.Length > 0)
            {
                book.Cover = await SaveImageAsync(model.CoverPhoto);
            }

            var result = await _profileService.UpdateBookAsync(book);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Книгу успішно оновлено!";
            }
            else
            {
                TempData["ErrorMessage"] = result.Error;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBook(int id)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _profileService.DeleteBookAsync(id);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Книгу видалено.";
            }
            else
            {
                TempData["ErrorMessage"] = result.Error;
            }

            return RedirectToAction("Index");
        }

        private async Task<LNUBookShare.Domain.Entities.Image> SaveImageAsync(Microsoft.AspNetCore.Http.IFormFile file)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "books");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return new LNUBookShare.Domain.Entities.Image
            {
                ImagePath = "/images/books/" + uniqueFileName,
                ImageType = "Book",
            };
        }
    }
}