using LNUBookShare.Application.Common;

namespace LNUBookShare.Application.Interfaces
{
    public interface IBookStatusService
    {
        Task<Result> ConfirmReturnAsync(int bookId, int ownerId);
        Task<Result> IssueBookAsync(int bookId, int ownerId);
    }
}