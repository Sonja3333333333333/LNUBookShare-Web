using System.Security.Claims;
using LNUBookShare.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

public class UserStatusMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<UserStatusMiddleware> _logger;

    public UserStatusMiddleware(RequestDelegate next, IMemoryCache cache, ILogger<UserStatusMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, UserManager<User> userManager, SignInManager<User> signInManager)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            string cacheKey = $"user_active_status_{userId}";

            if (!_cache.TryGetValue(cacheKey, out bool isActive))
            {
                var user = await userManager.GetUserAsync(context.User);

                if (user == null)
                {
                    await signInManager.SignOutAsync();
                    return;
                }

                isActive = user.IsActive;

                // Логуємо MISS
                if (isActive)
                {
                    _logger.LogInformation("\x1b[35m[AUTH CACHE MISS] Статус юзера {UserId} НЕ знайдено. База каже: АКТИВНИЙ.\x1b[0m", userId);
                }
                else
                {
                    _logger.LogWarning("\x1b[31m[AUTH CACHE MISS] Статус юзера {UserId} НЕ знайдено. База каже: ЗАБЛОКОВАНИЙ!\x1b[0m", userId);
                }

                _cache.Set(cacheKey, isActive, TimeSpan.FromMinutes(5));
            }
            else
            {
                if (isActive)
                {
                    _logger.LogInformation("\x1b[36m[AUTH CACHE HIT] Юзер {UserId} АКТИВНИЙ (взято з кешу).\x1b[0m", userId);
                }
                else
                {
                    _logger.LogWarning("\x1b[31m[AUTH CACHE HIT] Юзер {UserId} ЗАБЛОКОВАНИЙ (взято з кешу)!\x1b[0m", userId);
                }
            }

            if (!isActive)
            {
                _logger.LogCritical("\x1b[31m[SECURITY] Доступ заборонено для заблокованого юзера {UserId}. Редірект на Login.\x1b[0m", userId);
                await signInManager.SignOutAsync();
                context.Response.Redirect("/Account/Login?error=blocked");
                return;
            }
        }

        await _next(context);
    }
}