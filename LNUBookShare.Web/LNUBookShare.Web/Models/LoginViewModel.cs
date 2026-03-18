using System.ComponentModel.DataAnnotations;

namespace LNUBookShare.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Пошта є обов'язковою")]
    [EmailAddress(ErrorMessage = "Некоректний формат пошти")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Пароль є обов'язковим")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [Display(Name = "Запам'ятати мене?")]
    public bool RememberMe { get; set; }
}