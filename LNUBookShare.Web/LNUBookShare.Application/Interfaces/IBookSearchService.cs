using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface IBookSearchService
    {
        Task<IEnumerable<Book>> SearchAsync(string query, string searchBy = "title", string sortBy = "title", string statusFilter = "all");
        Task<IEnumerable<Book>> GetRecommendationsAsync(int facultyId, int currentUserId, string sortBy = "title", string statusFilter = "all");
    }
}