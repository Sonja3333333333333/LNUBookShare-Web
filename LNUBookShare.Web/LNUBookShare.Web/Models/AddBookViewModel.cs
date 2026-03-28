using System.ComponentModel.DataAnnotations;

namespace LNUBookShare.Web.Models
{
    public class AddBookViewModel
    {
        [Required(ErrorMessage = "Назва книги обов'язкова")]
        [Display(Name = "Назва")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Автор обов'язковий")]
        [Display(Name = "Автор")]
        public string Author { get; set; } = null!;

        [Display(Name = "ISBN")]
        public string? Isbn { get; set; }

        [Required(ErrorMessage = "Рік випуску обов'язковий")]
        [Display(Name = "Рік випуску")]
        [Range(1000, 2100, ErrorMessage = "Некоректний рік")]
        public int Year { get; set; }

        [Display(Name = "Видавництво")]
        public string? Publisher { get; set; }

        [Display(Name = "Мова")]
        public string? Language { get; set; }

        [Required(ErrorMessage = "Оберіть категорію")]
        [Display(Name = "Категорія")]
        public int CategoryId { get; set; }
    }
}