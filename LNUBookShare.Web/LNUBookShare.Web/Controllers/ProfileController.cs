using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LNUBookShare.Web.Controllers
{
    [Authorize]
    public class ProfileController : BaseController
    {
        private readonly IProfileService _profileService;
        private readonly IFacultyRepository _facultyRepository;
        private readonly IPhotoService _photoService;

        public ProfileController(
            UserManager<User> userManager,
            IProfileService profileService,
            IFacultyRepository facultyRepository,
            IPhotoService photoService)
            : base(userManager)
        {
            _profileService = profileService;
            _facultyRepository = facultyRepository;
            _photoService = photoService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _profileService.GetUserProfileAsync(user.Id);
            if (result.IsFailure)
            {
                return NotFound(result.Error);
            }

            var dto = result.Value;
            var model = new UserProfileViewModel
            {
                UserId = dto.UserId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                FacultyName = dto.FacultyName,
                AvatarPath = dto.AvatarPath,
                MyBooks = (await _profileService.GetUserBooksAsync(user.Id)).Value,
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _profileService.GetUserProfileAsync(user.Id);
            if (result.IsFailure)
            {
                return NotFound(result.Error);
            }

            var dto = result.Value;
            await LoadFacultiesToViewBag();

            var model = new EditProfileViewModel
            {
                UserId = dto.UserId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                FacultyId = user.FacultyId,
                AvatarPath = dto.AvatarPath,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadFacultiesToViewBag();
                return View(model);
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string? newAvatarPath = null;

            if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            {
                var photoResult = await _photoService.AddPhotoAsync(model.AvatarFile, "avatars");
                if (photoResult.IsSuccess)
                {
                    newAvatarPath = photoResult.Value;
                }
                else
                {
                    TempData["ErrorMessage"] = "Помилка завантаження фото: " + photoResult.Error;
                }
            }

            var result = await _profileService.UpdateProfileAsync(
                userId.Value,
                model.FirstName,
                model.LastName,
                model.FacultyId,
                newAvatarPath);

            if (result.IsFailure)
            {
                TempData["ErrorMessage"] = result.Error;
                await LoadFacultiesToViewBag();
                return View(model);
            }

            TempData["SuccessMessage"] = "Профіль успішно оновлено!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddBook(AddBookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Будь ласка, заповніть усі обов'язкові поля.";
                return RedirectToAction("Index");
            }

            var userId = GetCurrentUserId();
            if (userId == null)
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
                OwnerId = userId.Value,
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

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var booksResult = await _profileService.GetUserBooksAsync(userId.Value);
            var books = booksResult.IsSuccess ? booksResult.Value : new List<Book>();
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
            book.Language = model.Language;
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

            var userId = GetCurrentUserId();
            if (userId == null)
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

        [HttpGet]
        public async Task<IActionResult> Queue(int bookId, [FromServices] IReservationService reservationService)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var myBooksResult = await _profileService.GetUserBooksAsync(userId.Value);
            var myBooks = myBooksResult.IsSuccess ? myBooksResult.Value : new List<Book>();

            var book = myBooks.FirstOrDefault(b => b.BookId == bookId);

            if (book == null)
            {
                TempData["ErrorMessage"] = "Книгу не знайдено, або ви не є її власником.";
                return RedirectToAction("Index");
            }

            var queueUsersResult = await reservationService.GetQueueUsersAsync(bookId);
            var queueUsers = queueUsersResult.IsSuccess ? queueUsersResult.Value : new List<User>();

            ViewBag.BookTitle = book.Title;

            return View(queueUsers);
        }

        private async Task LoadFacultiesToViewBag()
        {
            var faculties = await _facultyRepository.GetAllAsync();
            ViewBag.Faculties = new SelectList(faculties, "FacultyId", "FacultyName");
        }

        private async Task<LNUBookShare.Domain.Entities.Image> SaveImageAsync(Microsoft.AspNetCore.Http.IFormFile file)
        {
            var photoResult = await _photoService.AddPhotoAsync(file, "books");

            return new LNUBookShare.Domain.Entities.Image
            {
                ImagePath = photoResult.IsSuccess ? photoResult.Value : "/images/default-book.png",
                ImageType = "Book",
            };
        }
    }
}