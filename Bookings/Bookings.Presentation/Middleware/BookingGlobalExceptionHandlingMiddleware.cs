using Bookings.Application.Exceptions;
using Bookings.Domain.Exceptions;
using CommonMiddleware;

namespace Bookings.Presentation.Middleware;

///<inheritdoc/>
public class BookingGlobalExceptionHandlingMiddleware: GlobalExceptionHandlingMiddleware
{
    ///<inheritdoc/>
    public BookingGlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<BookingGlobalExceptionHandlingMiddleware> logger) : base(next, logger)
    { }

    ///<inheritdoc/>
    protected override int? GetStatusCode(Exception ex)
    {
        var code = base.GetStatusCode(ex);
        if (code != null)
            return code;

        return ex switch
        {
            PastEventBookingException pee => StatusCodes.Status400BadRequest,
            ActiveBookingLimitException abe => StatusCodes.Status409Conflict,
            DbOperationException dboe => StatusCodes.Status500InternalServerError,
            NotFoundException nfe => StatusCodes.Status404NotFound,
            NoRightsException nre => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}
