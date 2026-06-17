using EventManagement.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EventManagement.Infrastructure.Services.UserServices;

/// <summary>
/// Класс предоставляет информацию о текущем пользователе
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _context;

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="httpContext">Контекст запроса</param>
    public CurrentUserService(IHttpContextAccessor httpContext)
    {
        _context = httpContext;
    }
   
    ///<inheritdoc/>
    public bool IsInRole(string role)
    {
        var user = _context.HttpContext.User;
        return user?.Identity?.IsAuthenticated ?? false 
            && user.IsInRole(role);
    }
}
