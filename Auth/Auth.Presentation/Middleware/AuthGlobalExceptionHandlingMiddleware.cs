using Auth.Application.Exceptions;
using Auth.Domain.Exceptions;
using CommonMiddleware;


namespace Auth.Presentation.Middleware;

///<inheritdoc/>
public class AuthGlobalExceptionHandlingMiddleware: GlobalExceptionHandlingMiddleware
{
    ///<inheritdoc/>
    public AuthGlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<AuthGlobalExceptionHandlingMiddleware> logger) : base(next, logger)
    { }

    ///<inheritdoc/>
    protected override int? GetStatusCode(Exception ex)
    {
        var code = base.GetStatusCode(ex);
        if (code != null)
            return code;

        return ex switch
        {
            InvalidCredentialsException ice => StatusCodes.Status401Unauthorized,
            DbOperationException dboe => StatusCodes.Status500InternalServerError,

            _ => StatusCodes.Status500InternalServerError
        };
    }
}
