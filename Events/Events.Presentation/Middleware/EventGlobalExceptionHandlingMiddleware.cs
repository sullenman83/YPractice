using CommonMiddleware;
using Events.Application.Exceptions;
using Events.Domain.Exceptions;

namespace Events.Presentation.Middleware;

///<inheritdoc/>
public class EventGlobalExceptionHandlingMiddleware: GlobalExceptionHandlingMiddleware
{
    ///<inheritdoc/>
    public EventGlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<EventGlobalExceptionHandlingMiddleware> logger) : base(next, logger)
    { }

    ///<inheritdoc/>
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
            DbOperationException dboe => StatusCodes.Status500InternalServerError,

            _ => StatusCodes.Status500InternalServerError
        };
    }
}
