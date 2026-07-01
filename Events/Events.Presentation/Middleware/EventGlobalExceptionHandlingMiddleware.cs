using CommonMiddleware;
using Events.Domain.Exceptions;

namespace Events.Presentation.Middleware;

public class EventGlobalExceptionHandlingMiddleware: GlobalExceptionHandlingMiddleware
{
    public EventGlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<EventGlobalExceptionHandlingMiddleware> logger) : base(next, logger)
    { }

    protected override int? GetStatusCode(Exception ex)
    {
        var code = base.GetStatusCode(ex);
        if (code != null)
            return code;

        return ex switch
        {
            NoAvailableSeatsException nae => StatusCodes.Status409Conflict,
            EventValidationException eve => StatusCodes.Status400BadRequest,
            NotFoundException nfe => StatusCodes.Status404NotFound,

            _ => StatusCodes.Status500InternalServerError
        };
    }
}
