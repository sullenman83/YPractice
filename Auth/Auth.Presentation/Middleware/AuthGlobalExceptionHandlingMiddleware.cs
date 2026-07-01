using Auth.Application.Exceptions;
using Auth.Domain.Exceptions;
using CommonMiddleware;


namespace Auth.Presentation.Middleware;

public class AuthGlobalExceptionHandlingMiddleware: GlobalExceptionHandlingMiddleware
{
    public AuthGlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<AuthGlobalExceptionHandlingMiddleware> logger) : base(next, logger)
    { }

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
