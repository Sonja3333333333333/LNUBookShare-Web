using LNUBookShare.Application.Common;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Domain.Models;

namespace LNUBookShare.Application.Interfaces
{
    public interface IProfileService
    {
        Task<Result<UserProfileDto>> GetUserProfileAsync(int userId);

        Task<Result> AddBookToProfileAsync(Book book);

        Task<Result<List<Book>>> GetUserBooksAsync(int userId);

        Task<Result> UpdateBookAsync(int bookId, int ownerId, string title, string author, int year, string? publisher, string? language, string? isbn, int categoryId, Image? newCover);

        Task<Result> DeleteBookAsync(int bookId);

        Task<Result> UpdateProfileAsync(int userId, string firstName, string lastName, int facultyId, string? avatarPath);

        Task<Result<List<Book>>> GetUserBooksAsync(int userId, string sortBy = "date", string statusFilter = "all");
    }
}