using System.ComponentModel.DataAnnotations;

namespace LNUBookShare.Web.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Введіть ім'я")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть прізвище")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Оберіть факультет")]
    public int FacultyId { get; set; }

    [Required(ErrorMessage = "Введіть пошту")]
    [EmailAddress(ErrorMessage = "Некоректний формат пошти")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть пароль")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Підтвердіть пароль")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Паролі не співпадають")]
    public string ConfirmPassword { get; set; } = string.Empty;
}