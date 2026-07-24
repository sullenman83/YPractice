
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Security;
using Microsoft.AspNetCore.Mvc;


namespace CommonMiddleware;

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
        LogError(ex, httpContext);

        if (httpContext.Response.HasStarted)
        {
            return;
        }

        var statusCode = GetStatusCode(ex) ?? StatusCodes.Status500InternalServerError;

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
    
    private void LogError(Exception ex, HttpContext httpContext)
    {
        _logger.LogError(ex,
            "Необработанное исключение. Метод={Method}, путь={Path}", httpContext.Request.Method, httpContext.Request.Path);
    }

    /// <summary>
    /// Конвертировать исключение к статус код
    /// </summary>
    /// <param name="ex">Исключение</param>
    /// <returns>Статус код. Если нет точного сопоставления, то null</returns>
    protected virtual int? GetStatusCode(Exception ex)
    {
        return ex switch
        {
            ArgumentNullException ane => StatusCodes.Status400BadRequest,
            ArgumentException ae => StatusCodes.Status400BadRequest,
            InvalidOperationException ioe => StatusCodes.Status500InternalServerError,
            NullReferenceException nr => StatusCodes.Status400BadRequest,
            HttpRequestException hr => StatusCodes.Status400BadRequest,
            ValidationException ve => StatusCodes.Status400BadRequest,            
            IOException io => StatusCodes.Status500InternalServerError,            
            SecurityException se => StatusCodes.Status401Unauthorized,
            OperationCanceledException oce => StatusCodes.Status499ClientClosedRequest,
            //NoAvailableSeatsException nae => StatusCodes.Status409Conflict,
            //EventValidationException eve => StatusCodes.Status400BadRequest,
            //NotFoundException nfe => StatusCodes.Status404NotFound,
            //DbOperationException dboe => StatusCodes.Status500InternalServerError,
            //ActiveBookingLimitException abe => StatusCodes.Status409Conflict,
            //PastEventBookingException pee => StatusCodes.Status400BadRequest,
            //NoRightsException nre => StatusCodes.Status403Forbidden,
            //InvalidCredentialsException ice => StatusCodes.Status401Unauthorized,
            
            _ => null
        };
    }
    #endregion
}
