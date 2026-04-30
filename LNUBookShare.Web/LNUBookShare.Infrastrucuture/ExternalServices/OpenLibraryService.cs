using System.Text.Json;
using LNUBookShare.Application.DTOs;

namespace LNUBookShare.Infrastructure.ExternalServices;

public class OpenLibraryService
{
    private readonly HttpClient _httpClient;

    public OpenLibraryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://openlibrary.org/");
    }

    public async Task<List<GlobalBookDto>> SearchEbooksAsync(string query)
    {
        // Шукаємо тільки ті книги, де є електронна версія (ebooks=true)
        var response = await _httpClient.GetAsync($"search.json?q={Uri.EscapeDataString(query)}&ebooks=true&limit=10");

        if (!response.IsSuccessStatusCode)
        {
            return new List<GlobalBookDto>();
        }

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        var books = new List<GlobalBookDto>();

        // Парсимо результати (Open Library повертає масив "docs")
        if (root.TryGetProperty("docs", out var docs))
        {
            foreach (var item in docs.EnumerateArray())
            {
                books.Add(new GlobalBookDto
                {
                    Title = item.GetProperty("title").GetString() ?? "Без назви",
                    Author = (item.TryGetProperty("author_name", out var authors)
                        ? authors[0].GetString()
                        : "Невідомий автор") ?? "Невідомий автор",
                    CoverUrl = item.TryGetProperty("cover_i", out var coverId)
                        ? $"https://covers.openlibrary.org/b/id/{coverId}-M.jpg"
                        : null,
                    OpenLibraryUrl = $"https://openlibrary.org{item.GetProperty("key").GetString()}",
                    HasFullText = item.TryGetProperty("has_fulltext", out var ft) && ft.GetBoolean(),
                });
            }
        }

        return books;
    }
}