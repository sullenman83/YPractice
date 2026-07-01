using Auth.Application.Interfaces.Services;
using Auth.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Application.Extensions;

/// <summary>
/// Регистратор сервисов для application слоя
/// </summary>
public static class ApplicationDIExt
{
    /// <summary>
    /// Зарегистрировать сервисы
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="configuration">Конфигурация</param>
    /// <returns>Коллекция сервисов</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUserService, UserService>();        
        
        return services;
    }
}
