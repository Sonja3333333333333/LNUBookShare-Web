using LNUBookShare.Application.Common;
using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<Result<List<Category>>> GetAllCategoriesAsync();
    }
}