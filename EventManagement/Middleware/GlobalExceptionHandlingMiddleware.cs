using EventManagement.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security;

namespace EventManagement.Middleware;

/// <summary>
/// Глобальный обрабюотчик исключений. Встраивается в конвеер обработки запросов
/// </summary>
public class GlobalExceptionHandlingMiddleware(RequestDelegate next,  ILogger<GlobalExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Метод обработки http запроса 
    /// </summary>
    /// <param name="httpContext">Контекст запроса</param>
    /// <returns>Задача, выполняющая обработку запроса</returns>
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleException(ex, httpContext);
        }
    }

    #region Закрытые методы

    private async Task HandleException(Exception ex, HttpContext httpContext)
    {
        logError(ex, httpContext);

        if (httpContext.Response.HasStarted)
        {
            return;
        }

        var statusCode = getStatusCode(ex);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        var error = new ProblemDetails
        {
            Title = "Необработанная ошибка",
            Detail = ex.Message,
            Status = statusCode,
        };

        await httpContext.Response.WriteAsJsonAsync(error);
    }
    
    private void logError(Exception ex, HttpContext httpContext)
    {
        _logger.LogError(ex,
            $"Unhandled exception. Method={httpContext.Request.Method}, Path={httpContext.Request.Path}");
    }

    private int getStatusCode(Exception ex)
    {
        return ex switch
        {            
            ArgumentException arg => StatusCodes.Status404NotFound,
            NullReferenceException nr => StatusCodes.Status400BadRequest,
            HttpRequestException hr => StatusCodes.Status400BadRequest,
            ValidationException ve => StatusCodes.Status400BadRequest,
            EventValidationException eve => StatusCodes.Status400BadRequest,
            BookingValidationException bve => StatusCodes.Status400BadRequest,
            IOException io => StatusCodes.Status500InternalServerError,
            
            SecurityException se => StatusCodes.Status401Unauthorized,

            _ => StatusCodes.Status500InternalServerError
        };
    }
    #endregion
}
