using EventManagement.Middleware;

namespace EventManagement.Extensions.Middleware;

/// <summary>
/// Расширение для добавления промежуточного по в конвеер
/// </summary>
public static class GlobalExceptionHandlingMiddlewareExtensions
{
    /// <summary>
    /// добавить глобальный обработчик исключений в конвеер
    /// </summary>
    /// <param name="builder">экземпляр IapplicationBuilder</param>
    /// <returns>экземпляр IapplicationBuilder</returns>
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }
}
