using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LNUBookShare.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        public async Task<Result<List<Category>>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _categoryRepository.GetAllAsync();

                var sortedCategories = categories.OrderBy(c => c.CategoryName).ToList();

                return sortedCategories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при отриманні списку категорій з бази даних.");
                return Result<List<Category>>.Failure("Не вдалося завантажити категорії.");
            }
        }
    }
}