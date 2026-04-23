using System.Security.Claims;
using System.Text;

namespace LNUBookShare.Web.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.ToString().ToLower();

            // 1. Пропускаємо статику (як і раніше)
            if (path.EndsWith(".css") || path.EndsWith(".js") || path.EndsWith(".png") ||
                path.Contains("_framework") || path.Contains("_vs"))
            {
                await _next(context);
                return;
            }

            context.Request.EnableBuffering();

            var method = context.Request.Method;
            var url = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";

            // 2. Маскуємо Кукі
            var headers = context.Request.Headers
                .Select(h => h.Key == "Cookie" ? $"{h.Key}: [Present]" : $"{h.Key}: {h.Value}");
            var headersString = string.Join("; ", headers);

            // 3. Маскуємо Тіло запиту, якщо це ЛОГІН
            string body;
            if (path.Contains("/account/login"))
            {
                body = "[REDACTED FOR SECURITY]"; // Не читаємо і не логуємо пароль
            }
            else
            {
                body = await ReadRequestBody(context.Request);
            }

            // 4. Логуємо (Параметри з нового рядка для StyleCop)
            _logger.LogInformation(
                "\x1b[33mHTTP Request Info (Secure):\x1b[0m\n" +
                "\x1b[33mUser: {UserId}\x1b[0m\n" +
                "\x1b[33mAction: {Method} {Url}\x1b[0m\n" +
                "\x1b[33mIP: {IP}\x1b[0m\n" +
                "\x1b[33mHeaders: {Headers}\x1b[0m\n" +
                "\x1b[33mBody: {Body}\x1b[0m",
                userId,
                method,
                url,
                ip,
                headersString,
                body);

            await _next(context);
        }

        private async Task<string> ReadRequestBody(HttpRequest request)
        {
            // Ставимо курсор у початок стріма
            request.Body.Position = 0;

            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();

            request.Body.Position = 0;

            return string.IsNullOrWhiteSpace(body) ? "[Empty Body]" : body;
        }
    }
}