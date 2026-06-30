using CommonMiddleware;

namespace Events.Presentation.Middleware;

public class EventGlobalExceptionHandlingMiddleware: GlobalExceptionHandlingMiddleware
{
    public EventGlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<EventGlobalExceptionHandlingMiddleware> logger) : base(next, logger)
    { }

    protected override int? GetStatusCode(Exception ex)
    {
        if (var res = base.GetStatusCode(ex) == null)

    }
}
