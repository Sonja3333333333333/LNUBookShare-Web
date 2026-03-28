using System.Threading.Tasks;
using LNUBookShare.Application.Common;
using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface IProfileService
    {
        Task<Result> AddBookToProfileAsync(Book book);
    }
}