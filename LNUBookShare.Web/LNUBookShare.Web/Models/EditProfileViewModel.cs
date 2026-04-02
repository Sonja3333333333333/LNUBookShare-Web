using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LNUBookShare.Web.Models
{
    public class EditProfileViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "¬вед≥ть пр≥звище")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "¬вед≥ть ≥м'€")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "ќбер≥ть факультет")]
        public int FacultyId { get; set; }

        public string? AvatarPath { get; set; }

        public IFormFile? AvatarFile { get; set; }
    }
}