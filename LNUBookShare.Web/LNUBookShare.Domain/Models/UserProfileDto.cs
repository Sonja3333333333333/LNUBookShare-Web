namespace LNUBookShare.Domain.Models
{
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FacultyName { get; set; } = string.Empty;
        public string? AvatarPath { get; set; }
    }
}