using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LNUBookShare.Application.Services
{
    public class FacultyService : IFacultyService
    {
        private const string FacultyCacheKey = "Faculties_List_Key";

        private readonly IFacultyRepository _facultyRepository;
        private readonly IMemoryCache _cache;
        private readonly CacheSettings _cacheSettings;
        private readonly ILogger<FacultyService> _logger;

        public FacultyService(
            IFacultyRepository facultyRepository,
            IMemoryCache cache,
            IOptions<CacheSettings> cacheSettings,
            ILogger<FacultyService> logger)
        {
            _facultyRepository = facultyRepository;
            _cache = cache;
            _cacheSettings = cacheSettings.Value;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<Faculty>>> GetAllFacultiesAsync()
        {
            if (!_cache.TryGetValue(FacultyCacheKey, out IEnumerable<Faculty>? faculties) || faculties == null)
            {
                _logger.LogInformation("\x1b[35m[CACHE MISS] кеш порожній, звертаємось до бази...\x1b[0m");

                var result = await _facultyRepository.GetAllAsync();
                faculties = result ?? new List<Faculty>();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(_cacheSettings.ExpirationMinutes))

                    .SetSlidingExpiration(TimeSpan.FromMinutes(15));

                _cache.Set(FacultyCacheKey, faculties, cacheOptions);
            }
            else
            {
                _logger.LogInformation("\x1b[36m[CACHE HIT] взято кешовані дані про факультети.\x1b[0m");
            }

            return Result<IEnumerable<Faculty>>.Success(faculties);
        }

        public async Task<Result> AddFacultyAsync(Faculty faculty)
        {
            await _facultyRepository.AddAsync(faculty);

            _logger.LogWarning("\x1b[33m[CACHE INVALIDATION] Додано новий факультет. Старий кеш видалено для оновлення даних.\x1b[0m");

            _cache.Remove(FacultyCacheKey);
            return Result.Success();
        }
    }
}