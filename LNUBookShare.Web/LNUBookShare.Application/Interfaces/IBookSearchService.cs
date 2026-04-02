using LNUBookShare.Application.Common;
using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface IBookSearchService
    {
        Task<Result<IEnumerable<Book>>> SearchAsync(string query, string searchBy = "title", string sortBy = "title", string statusFilter = "all");

        Task<Result<IEnumerable<Book>>> GetRecommendationsAsync(int facultyId, int currentUserId, string sortBy = "title", string statusFilter = "all");
    }
}