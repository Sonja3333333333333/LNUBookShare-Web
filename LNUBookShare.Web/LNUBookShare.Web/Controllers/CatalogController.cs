using LNUBookShare.Application.Services;
using LNUBookShare.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Serilog.Core;

namespace LNUBookShare.Web.Controllers
{
    public class CatalogController : Controller
    {
        private readonly BookSearchService _searchService;
        private readonly ILogger<CatalogController> _logger;
        public CatalogController(BookSearchService searchService, ILogger<CatalogController> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        public async Task<IActionResult> Search(string query, string searchBy = "title")
        {
            var results = await _searchService.SearchAsync(query, searchBy);
            _logger.LogInformation("Користувач здійснив пошук за запитом: {Query}, критерій: {SearchBy}", query, searchBy);

            var model = new BookSearchViewModel
            {
                SearchQuery = query,
                Books = results,
                SearchBy = searchBy,
            };

            return View(model);
        }
    }
}
