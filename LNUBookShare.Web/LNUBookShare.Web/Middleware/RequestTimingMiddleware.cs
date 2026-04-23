using System.Diagnostics;

namespace LNUBookShare.Web.Middleware
{
    public class RequestTimingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestTimingMiddleware> _logger;

        public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Стартуємо секундомір
            var sw = Stopwatch.StartNew();

            try
            {
                // 2. Пропускаємо запит далі по конвеєру (в контролери, сервіси, базу)
                await _next(context);
            }
            finally
            {
                // 3. Зупиняємо, навіть якщо сталася помилка
                sw.Stop();
                var elapsedMs = sw.ElapsedMilliseconds;

                // 4. Логуємо. Зробимо його зеленим (\x1b[32m), щоб відрізняти від жовтого
                var path = context.Request.Path;
                var method = context.Request.Method;

                // Якщо виконується довше 500мс — це вже "дзвіночок"
                if (elapsedMs > 500)
                {
                    _logger.LogWarning(
                        "\x1b[31m[SLOW REQUEST] {Method} {Path} took {Elapsed} ms\x1b[0m", method, path, elapsedMs);
                }
                else
                {
                    _logger.LogInformation(
                        "\x1b[32m[PERF] {Method} {Path} processed in {Elapsed} ms\x1b[0m", method, path, elapsedMs);
                }
            }
        }
    }
}