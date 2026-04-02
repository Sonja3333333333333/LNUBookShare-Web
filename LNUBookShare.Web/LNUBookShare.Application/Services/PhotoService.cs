using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace LNUBookShare.Application.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly Cloudinary _cloudinary;

        public PhotoService(IOptions<CloudinarySettings> config)
        {
            var acc = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret);

            _cloudinary = new Cloudinary(acc);
        }

        public async Task<Result<string>> AddPhotoAsync(IFormFile file, string folderName)
        {
            if (file.Length == 0)
            {
                return Result<string>.Failure("Файл порожній або не був завантажений.");
            }

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folderName,
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                return Result<string>.Failure(uploadResult.Error.Message);
            }

            return Result<string>.Success(uploadResult.SecureUrl.ToString());
        }
    }
}