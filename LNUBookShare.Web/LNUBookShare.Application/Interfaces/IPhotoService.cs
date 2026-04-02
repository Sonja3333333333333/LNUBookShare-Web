using LNUBookShare.Application.Common;
using Microsoft.AspNetCore.Http;

namespace LNUBookShare.Application.Interfaces
{
    public interface IPhotoService
    {
        Task<Result<string>> AddPhotoAsync(IFormFile file, string folderName);
    }
}