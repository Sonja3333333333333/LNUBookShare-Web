using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LNUBookShare.Web.Models
{
    public class AddBookViewModel
    {
        [Required(ErrorMessage = "Назва книги обов'язкова")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Автор обов'язковий")]
        public string Author { get; set; } = null!;

        public string? Isbn { get; set; }

        [Required(ErrorMessage = "Рік випуску обов'язковий")]
        [Range(1000, 2100, ErrorMessage = "Некоректний рік")]
        public int Year { get; set; }

        public string? Publisher { get; set; }

        public string? Language { get; set; }

        [Required(ErrorMessage = "Оберіть категорію")]
        public int CategoryId { get; set; }

        public IFormFile? CoverPhoto { get; set; }
    }
}