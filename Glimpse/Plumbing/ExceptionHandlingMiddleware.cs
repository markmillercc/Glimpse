using System.Text.Json;

namespace Glimpse.Plumbing;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;
    private readonly bool _isDevelopment;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger logger, bool isDevelopment)
    {
        _next = next;
        _logger = logger;
        _isDevelopment = isDevelopment;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            string error;

            if (ex is BadHttpRequestException bex)
            {
                error = "Bad request";
                context.Response.StatusCode = bex.StatusCode;
            }
            else
            {
                var reference = Guid.NewGuid();
                _logger.LogError(ex, $"{reference}: {ex.Message}");

                error = $"An unexpected error occurred. Reference: {reference}";
                context.Response.StatusCode = 500;
            }            

            context.Response.ContentType = "application/json";
            var result = _isDevelopment
                ? JsonSerializer.Serialize(new { error, exception = ex.ToString() })
                : JsonSerializer.Serialize(new { error });

            await context.Response.WriteAsync(result);
        }
    }
}
