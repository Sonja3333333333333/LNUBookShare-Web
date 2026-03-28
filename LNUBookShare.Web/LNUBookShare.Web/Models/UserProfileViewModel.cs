namespace LNUBookShare.Web.Models
{
    public class UserProfileViewModel
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FacultyName { get; set; } = string.Empty;
        public string? AvatarPath { get; set; }
    }
}