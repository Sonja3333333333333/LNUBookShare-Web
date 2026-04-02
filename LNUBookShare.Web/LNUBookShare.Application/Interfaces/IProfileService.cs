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

        Task<Result> UpdateBookAsync(Book book);

        Task<Result> DeleteBookAsync(int bookId);

        Task<Result> UpdateProfileAsync(int userId, string firstName, string lastName, int facultyId, string? avatarPath);
    }
}