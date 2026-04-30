using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace LNUBookShare.Web.Filters;

public class IpRateLimitAttribute : ActionFilterAttribute
{
    private const string CacheKeyPrefix = "RL_";

    private readonly int _requestLimit;
    public IpRateLimitAttribute(int requestsPerMinute)
    {
        _requestLimit = requestsPerMinute;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Отримуємо сервіс кешування з контейнера залежностей (DI)
        var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();

        // Визначаємо IP-адресу користувача
        var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var cacheKey = $"{CacheKeyPrefix}{ip}";

        // Перевіряємо, скільки запитів вже було
        if (cache.TryGetValue(cacheKey, out int requestCount))
        {
            if (requestCount >= _requestLimit)
            {
                // ПЕРЕВИЩЕНО: Редірект на помилку
                context.Result = new RedirectToActionResult("Error", "Home", new { message = "Занадто багато запитів. Відпочиньте хвилину!" });
                return;
            }

            // Оновлюємо лічильник
            cache.Set(cacheKey, requestCount + 1, TimeSpan.FromMinutes(1));
        }
        else
        {
            // ПЕРШИЙ ЗАПИТ: Створюємо запис у кеші на 1 хвилину
            cache.Set(cacheKey, 1, TimeSpan.FromMinutes(1));
        }

        base.OnActionExecuting(context);
    }
}