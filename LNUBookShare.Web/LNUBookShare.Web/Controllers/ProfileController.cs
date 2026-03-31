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
    public class ProfileController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IProfileService _profileService;
        private readonly IFacultyRepository _facultyRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(
            UserManager<User> userManager,
            IProfileService profileService,
            IFacultyRepository facultyRepository,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _profileService = profileService;
            _facultyRepository = facultyRepository;
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
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
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

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string? newAvatarPath = null;

            if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "avatars");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{user.Id}_{Guid.NewGuid()}{Path.GetExtension(model.AvatarFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.AvatarFile.CopyToAsync(stream);
                }

                newAvatarPath = $"/images/avatars/{uniqueFileName}";
            }

            var result = await _profileService.UpdateProfileAsync(
                user.Id,
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

        private async Task LoadFacultiesToViewBag()
        {
            var faculties = await _facultyRepository.GetAllAsync();
            ViewBag.Faculties = new SelectList(faculties, "FacultyId", "FacultyName");
        }
    }
}