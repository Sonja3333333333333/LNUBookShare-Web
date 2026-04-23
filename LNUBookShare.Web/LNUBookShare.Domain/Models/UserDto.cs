namespace LNUBookShare.Domain.Models
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FacultyName { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }

        public bool IsActive { get; set; }
    }
}
