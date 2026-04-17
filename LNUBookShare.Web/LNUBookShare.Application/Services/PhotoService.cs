using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace LNUBookShare.Application.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly Cloudinary _cloudinary;
        private readonly PhotoSettings _photoSettings;

        public PhotoService(
            IOptions<CloudinarySettings> cloudinaryConfig,
            IOptions<PhotoSettings> photoConfig)
        {
            var acc = new Account(
                cloudinaryConfig.Value.CloudName,
                cloudinaryConfig.Value.ApiKey,
                cloudinaryConfig.Value.ApiSecret);

            _cloudinary = new Cloudinary(acc);
            _photoSettings = photoConfig.Value;
        }

        public async Task<Result<string>> AddPhotoAsync(IFormFile file, string folderName)
        {
            if (file.Length == 0)
            {
                return Result<string>.Failure("Файл порожній або не був завантажений.");
            }

            if (file.Length > _photoSettings.MaxFileSizeBytes)
            {
                return Result<string>.Failure($"Файл занадто великий. Максимальний розмір: {_photoSettings.MaxFileSizeBytes / 1024 / 1024} МБ.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_photoSettings.AllowedExtensions.Contains(extension))
            {
                return Result<string>.Failure($"Недопустимий формат файлу. Дозволені: {string.Join(", ", _photoSettings.AllowedExtensions)}");
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

            return uploadResult.SecureUrl.ToString();
        }
    }
}