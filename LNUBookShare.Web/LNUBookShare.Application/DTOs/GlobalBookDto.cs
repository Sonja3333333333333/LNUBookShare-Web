namespace LNUBookShare.Application.DTOs;

public class GlobalBookDto
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = "Невідомий автор";
    public string? CoverUrl { get; set; }
    public string? OpenLibraryUrl { get; set; }
    public bool HasFullText { get; set; }
    public string? PreviewUrl { get; set; } // Посилання на онлайн-рідер
}